import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { execFileSync } from "node:child_process";
import { checkUrlHealth, fetchPage, normalizeUrl } from "./lib/import-firefox.js";
import {
  classifyKeepNote,
  noteTitleForWiki,
  parseKeepNotesFromFiles,
  renderNoteBody,
  resolveBookmarkImport,
  wikiDirForKeep,
  type ClassifiedKeepNote,
  type KeepNoteKind,
  type ParsedKeepNote,
} from "./lib/import-keep.js";
import { resolveSlugFilename } from "./lib/slug.js";

interface CliOptions {
  inputPath: string;
  repoRoot: string;
  dryRun: boolean;
  concurrency: number;
  captureDate: string;
}

interface ManifestEntry {
  keepId: string;
  sourceFile: string;
  title: string;
  kind: KeepNoteKind;
  url?: string;
  wikiPath: string;
  status: "imported" | "dead" | "skipped" | "error";
  reason?: string;
}

interface KeepImportManifest {
  importDate: string;
  sourceZip: string;
  totals: {
    parsed: number;
    imported: number;
    dead: number;
    skipped: number;
    duplicate: number;
    error: number;
  };
  entries: ManifestEntry[];
  skipped: Array<{ sourceFile: string; reason: string }>;
}

function parseArgs(argv: string[]): CliOptions {
  const args = argv.slice(2);
  let inputPath = "";
  let dryRun = false;
  let concurrency = 5;

  for (let i = 0; i < args.length; i += 1) {
    const arg = args[i];
    if (arg === "--dry-run") {
      dryRun = true;
    } else if (arg === "--concurrency" && args[i + 1]) {
      concurrency = Number(args[i + 1]);
      i += 1;
    } else if (!arg.startsWith("-") && !inputPath) {
      inputPath = arg;
    }
  }

  if (!inputPath) {
    console.error("Usage: npm run kb:import-keep -- <takeout.zip> [--dry-run] [--concurrency N]");
    process.exit(1);
  }

  return {
    inputPath: path.resolve(inputPath),
    repoRoot: process.cwd(),
    dryRun,
    concurrency,
    captureDate: new Date().toISOString().slice(0, 10),
  };
}

function ensureDir(dir: string, dryRun: boolean): void {
  if (!dryRun && !fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function extractZipToTemp(zipPath: string): string {
  const tempDir = fs.mkdtempSync(path.join(os.tmpdir(), "kb-keep-import-"));

  if (process.platform === "win32") {
    const ps1 = path.join(tempDir, "extract.ps1");
    const escapedZip = zipPath.replace(/'/g, "''");
    const escapedDir = tempDir.replace(/'/g, "''");
    fs.writeFileSync(
      ps1,
      `Expand-Archive -LiteralPath '${escapedZip}' -DestinationPath '${escapedDir}' -Force`,
      "utf8",
    );
    execFileSync("powershell", ["-NoProfile", "-File", ps1], { stdio: "inherit" });
  } else {
    execFileSync("unzip", ["-q", zipPath, "-d", tempDir], { stdio: "inherit" });
  }

  return tempDir;
}

function findKeepRoot(extractDir: string): string {
  const candidates = [
    path.join(extractDir, "Takeout", "Google Keep"),
    path.join(extractDir, "Google Keep"),
    extractDir,
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate)) {
      return candidate;
    }
  }

  throw new Error(`Could not find Google Keep folder in extracted archive: ${extractDir}`);
}

function readKeepFiles(keepRoot: string): Array<{ sourceFile: string; raw: string }> {
  const files: Array<{ sourceFile: string; raw: string }> = [];

  for (const entry of fs.readdirSync(keepRoot, { withFileTypes: true })) {
    if (!entry.isFile() || !entry.name.endsWith(".json")) {
      continue;
    }
    const abs = path.join(keepRoot, entry.name);
    files.push({
      sourceFile: entry.name,
      raw: fs.readFileSync(abs, "utf8"),
    });
  }

  return files;
}

function scanExistingWikiUrls(repoRoot: string): Map<string, string> {
  const byUrl = new Map<string, string>();
  const roots = [path.join(repoRoot, "wiki", "bookmarks"), path.join(repoRoot, "wiki", "notes")];

  for (const root of roots) {
    if (!fs.existsSync(root)) continue;
    walkMarkdown(root, repoRoot, byUrl);
  }

  return byUrl;
}

function walkMarkdown(dir: string, repoRoot: string, byUrl: Map<string, string>): void {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walkMarkdown(full, repoRoot, byUrl);
      continue;
    }
    if (!entry.name.endsWith(".md")) continue;
    const content = fs.readFileSync(full, "utf8");
    const match = content.match(/^url:\s*"([^"]+)"/m);
    if (!match) continue;
    const rel = path.relative(repoRoot, full).replace(/\\/g, "/");
    byUrl.set(normalizeUrl(match[1]), rel);
  }
}

function loadPriorManifest(manifestPath: string): {
  importedNoteKeepIds: Set<string>;
  importedBookmarkKeys: Set<string>;
  entries: ManifestEntry[];
} {
  const importedNoteKeepIds = new Set<string>();
  const importedBookmarkKeys = new Set<string>();
  const entries: ManifestEntry[] = [];

  if (!fs.existsSync(manifestPath)) {
    return { importedNoteKeepIds, importedBookmarkKeys, entries };
  }

  try {
    const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8")) as KeepImportManifest;
    for (const entry of manifest.entries) {
      entries.push(entry);
      if (entry.status !== "imported") {
        continue;
      }
      if (entry.kind === "bookmark" && entry.url) {
        importedBookmarkKeys.add(`${entry.keepId}|${normalizeUrl(entry.url)}`);
      } else if (entry.kind !== "bookmark") {
        importedNoteKeepIds.add(entry.keepId);
      }
    }
  } catch {
    // ignore corrupt manifest
  }

  return { importedNoteKeepIds, importedBookmarkKeys, entries };
}

function buildNoteFrontmatter(
  classified: ClassifiedKeepNote,
  captureDate: string,
  url?: string,
): string {
  const tags = [...classified.tags];
  if (classified.kind === "bookmark") {
    tags.unshift("bookmark");
  } else {
    tags.unshift("note");
  }

  const lines = [
    "---",
    `date: ${captureDate}`,
    `type: ${classified.kind === "bookmark" ? "bookmark" : "note"}`,
    `tags: [${[...new Set(tags)].map((t) => JSON.stringify(t)).join(", ")}]`,
  ];

  if (url) {
    lines.push(`url: ${JSON.stringify(url)}`);
  }

  lines.push("---");
  return lines.join("\n");
}

function buildBookmarkBody(title: string, url: string, summary: string): string {
  const safeTitle = title || url;
  const summaryText = summary || "No excerpt available from fetch.";
  return `# ${safeTitle}\n\n${url}\n\n## Summary\n\n${summaryText}\n`;
}

function buildNoteWikiBody(title: string, body: string): string {
  return `# ${title}\n\n${body}\n`;
}

function copyAttachment(
  keepRoot: string,
  attachmentName: string,
  repoRoot: string,
  dryRun: boolean,
): string {
  const src = path.join(keepRoot, attachmentName);
  const assetsDir = path.join(repoRoot, "wiki", "assets");
  ensureDir(assetsDir, dryRun);

  const baseName = path.basename(attachmentName);
  const destName = resolveSlugFilename(baseName.replace(/\.[^.]+$/, ""), assetsDir, path.extname(baseName));
  const destAbs = path.join(assetsDir, destName);
  const rel = `wiki/assets/${destName}`;

  if (!dryRun && fs.existsSync(src)) {
    fs.copyFileSync(src, destAbs);
  }

  return rel;
}

async function mapPool<T, R>(
  items: T[],
  concurrency: number,
  worker: (item: T, index: number) => Promise<R>,
): Promise<R[]> {
  const results: R[] = new Array(items.length);
  let nextIndex = 0;

  async function run(): Promise<void> {
    while (nextIndex < items.length) {
      const index = nextIndex;
      nextIndex += 1;
      results[index] = await worker(items[index], index);
    }
  }

  const runners = Array.from({ length: Math.min(concurrency, items.length) }, () => run());
  await Promise.all(runners);
  return results;
}

async function main(): Promise<void> {
  const options = parseArgs(process.argv);

  if (!fs.existsSync(options.inputPath)) {
    console.error(`Input not found: ${options.inputPath}`);
    process.exit(1);
  }

  const tempDir = extractZipToTemp(options.inputPath);
  const keepRoot = findKeepRoot(tempDir);
  const rawFiles = readKeepFiles(keepRoot);
  const { notes, skipped: parseSkipped } = parseKeepNotesFromFiles(rawFiles);

  const manifestPath = path.join(
    options.repoRoot,
    "artifacts",
    "kb-import",
    `keep-${options.captureDate}.manifest.json`,
  );

  const { importedNoteKeepIds, importedBookmarkKeys, entries: priorEntries } =
    loadPriorManifest(manifestPath);
  const existingWikiByUrl = scanExistingWikiUrls(options.repoRoot);

  const manifestEntries: ManifestEntry[] = [...priorEntries];
  let imported = 0;
  let dead = 0;
  let duplicate = 0;
  let errors = 0;

  const classifiedNotes = notes.map((note) => classifyKeepNote(note));

  const bookmarkJobs: Array<{
    classified: ClassifiedKeepNote;
    target: { url: string; title: string; source: string };
  }> = [];

  const noteJobs: ClassifiedKeepNote[] = [];

  for (const classified of classifiedNotes) {
    const note = classified.note;

    if (classified.kind === "bookmark") {
      for (const target of classified.bookmarkTargets) {
        const normalized = normalizeUrl(target.url);
        const bookmarkKey = `${note.keepId}|${normalized}`;
        if (importedBookmarkKeys.has(bookmarkKey) || existingWikiByUrl.has(normalized)) {
          duplicate += 1;
          manifestEntries.push({
            keepId: note.keepId,
            sourceFile: note.sourceFile,
            title: target.title,
            kind: "bookmark",
            url: target.url,
            wikiPath: existingWikiByUrl.get(normalized) ?? "",
            status: "skipped",
            reason: importedBookmarkKeys.has(bookmarkKey) ? "already_imported" : "duplicate_url",
          });
          continue;
        }
        bookmarkJobs.push({ classified, target });
      }
      continue;
    }

    if (importedNoteKeepIds.has(note.keepId)) {
      duplicate += 1;
      manifestEntries.push({
        keepId: note.keepId,
        sourceFile: note.sourceFile,
        title: noteTitleForWiki(note, classified),
        kind: classified.kind,
        wikiPath: "",
        status: "skipped",
        reason: "already_imported",
      });
      continue;
    }

    noteJobs.push(classified);
  }

  console.log(
    `Parsed ${rawFiles.length} JSON files, ${notes.length} importable, ${bookmarkJobs.length} bookmarks, ${noteJobs.length} notes (${duplicate} skipped as duplicates)`,
  );

  if (!options.dryRun) {
    await mapPool(bookmarkJobs, options.concurrency, async ({ classified, target }) => {
      const note = classified.note;

      try {
        const outcome = await resolveBookmarkImport(target, {
          checkHealth: checkUrlHealth,
          fetchPage,
        });

        if (outcome.status === "dead") {
          dead += 1;
          manifestEntries.push({
            keepId: note.keepId,
            sourceFile: note.sourceFile,
            title: target.title,
            kind: "bookmark",
            url: target.url,
            wikiPath: "",
            status: "dead",
            reason: outcome.reason,
          });
          return;
        }

        const summary = outcome.summary;

        const wikiRelDir = wikiDirForKeep("bookmark", classified.theme);
        const wikiAbsDir = path.join(options.repoRoot, wikiRelDir);
        ensureDir(wikiAbsDir, options.dryRun);

        const wikiFilename = resolveSlugFilename(target.title || target.url, wikiAbsDir);
        const wikiRelPath = path.posix.join(wikiRelDir.replace(/\\/g, "/"), wikiFilename);
        const wikiAbsPath = path.join(wikiAbsDir, wikiFilename);

        const frontmatter = buildNoteFrontmatter(classified, options.captureDate, target.url);
        const content = `${frontmatter}\n\n${buildBookmarkBody(target.title, target.url, summary)}`;
        fs.writeFileSync(wikiAbsPath, content, "utf8");

        imported += 1;
        importedBookmarkKeys.add(`${note.keepId}|${normalizeUrl(target.url)}`);
        existingWikiByUrl.set(normalizeUrl(target.url), wikiRelPath);
        manifestEntries.push({
          keepId: note.keepId,
          sourceFile: note.sourceFile,
          title: target.title,
          kind: "bookmark",
          url: target.url,
          wikiPath: wikiRelPath,
          status: "imported",
        });

        process.stdout.write(`Imported bookmark: ${target.title}\n`);
      } catch (error) {
        errors += 1;
        manifestEntries.push({
          keepId: note.keepId,
          sourceFile: note.sourceFile,
          title: target.title,
          kind: "bookmark",
          url: target.url,
          wikiPath: "",
          status: "error",
          reason: error instanceof Error ? error.message : "unknown_error",
        });
      }
    });

    for (const classified of noteJobs) {
      const note = classified.note;
      const title = noteTitleForWiki(note, classified);

      try {
        const assetPaths = note.attachments.map((attachment) =>
          copyAttachment(keepRoot, attachment.filePath, options.repoRoot, options.dryRun),
        );

        const wikiRelDir = wikiDirForKeep(classified.kind, classified.theme);
        const wikiAbsDir = path.join(options.repoRoot, wikiRelDir);
        ensureDir(wikiAbsDir, options.dryRun);

        const wikiFilename = resolveSlugFilename(title, wikiAbsDir);
        const wikiRelPath = path.posix.join(wikiRelDir.replace(/\\/g, "/"), wikiFilename);
        const wikiAbsPath = path.join(wikiAbsDir, wikiFilename);

        const body = renderNoteBody(note, assetPaths);
        const frontmatter = buildNoteFrontmatter(classified, options.captureDate);
        const content = `${frontmatter}\n\n${buildNoteWikiBody(title, body)}`;
        fs.writeFileSync(wikiAbsPath, content, "utf8");

        imported += 1;
        importedNoteKeepIds.add(note.keepId);
        manifestEntries.push({
          keepId: note.keepId,
          sourceFile: note.sourceFile,
          title,
          kind: classified.kind,
          wikiPath: wikiRelPath,
          status: "imported",
        });

        process.stdout.write(`Imported note: ${title}\n`);
      } catch (error) {
        errors += 1;
        manifestEntries.push({
          keepId: note.keepId,
          sourceFile: note.sourceFile,
          title,
          kind: classified.kind,
          wikiPath: "",
          status: "error",
          reason: error instanceof Error ? error.message : "unknown_error",
        });
      }
    }
  } else {
    for (const { classified, target } of bookmarkJobs) {
      manifestEntries.push({
        keepId: classified.note.keepId,
        sourceFile: classified.note.sourceFile,
        title: target.title,
        kind: "bookmark",
        url: target.url,
        wikiPath: wikiDirForKeep("bookmark", classified.theme),
        status: "skipped",
        reason: "dry_run",
      });
    }
    for (const classified of noteJobs) {
      manifestEntries.push({
        keepId: classified.note.keepId,
        sourceFile: classified.note.sourceFile,
        title: noteTitleForWiki(classified.note, classified),
        kind: classified.kind,
        wikiPath: wikiDirForKeep(classified.kind, classified.theme),
        status: "skipped",
        reason: "dry_run",
      });
    }
  }

  const manifest: KeepImportManifest = {
    importDate: options.captureDate,
    sourceZip: options.inputPath,
    totals: {
      parsed: rawFiles.length,
      imported,
      dead,
      skipped: parseSkipped.length + manifestEntries.filter((e) => e.status === "skipped").length,
      duplicate,
      error: errors,
    },
    entries: dedupeManifestEntries(manifestEntries),
    skipped: parseSkipped,
  };

  ensureDir(path.dirname(manifestPath), options.dryRun);
  if (!options.dryRun) {
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf8");
    appendNavigation(options.repoRoot, options.captureDate, manifest.totals);
  } else {
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf8");
    console.log("\nDry run — manifest written, no wiki files or network fetch.");
  }

  try {
    fs.rmSync(tempDir, { recursive: true, force: true });
  } catch {
    // ignore cleanup errors
  }

  console.log("\nImport complete:");
  console.log(`  imported: ${imported}`);
  console.log(`  dead: ${dead}`);
  console.log(`  duplicate/skipped: ${duplicate}`);
  console.log(`  parse skipped: ${parseSkipped.length}`);
  console.log(`  errors: ${errors}`);
  console.log(`  manifest: ${manifestPath}`);
}

function dedupeManifestEntries(entries: ManifestEntry[]): ManifestEntry[] {
  const byKey = new Map<string, ManifestEntry>();
  const rank = (entry: ManifestEntry): number => {
    if (entry.status === "imported") return 4;
    if (entry.status === "dead") return 3;
    if (entry.status === "error") return 2;
    return 1;
  };

  for (const entry of entries) {
    const key = `${entry.keepId}|${entry.url ?? ""}|${entry.kind}`;
    const existing = byKey.get(key);
    if (!existing || rank(entry) > rank(existing)) {
      byKey.set(key, entry);
    }
  }

  return [...byKey.values()];
}

function appendNavigation(
  repoRoot: string,
  date: string,
  totals: KeepImportManifest["totals"],
): void {
  const indexPath = path.join(repoRoot, "wiki", "index.md");
  const logPath = path.join(repoRoot, "wiki", "log.md");

  const indexLine = `- ${date} — Google Keep import: ${totals.imported} pages imported, ${totals.dead} dead links excluded → see \`wiki/notes/\` and \`wiki/bookmarks/\``;
  const logLine = `- ${date} — google-keep-import: ${totals.imported} imported, ${totals.dead} dead, ${totals.skipped} skipped`;

  if (fs.existsSync(indexPath)) {
    const indexContent = fs.readFileSync(indexPath, "utf8");
    if (!indexContent.includes("Google Keep import:")) {
      fs.appendFileSync(indexPath, `\n${indexLine}\n`, "utf8");
    }
  }

  if (fs.existsSync(logPath)) {
    const logContent = fs.readFileSync(logPath, "utf8");
    if (!logContent.includes("google-keep-import:")) {
      fs.appendFileSync(logPath, `\n${logLine}\n`, "utf8");
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
