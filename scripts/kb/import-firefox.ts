import fs from "node:fs";
import path from "node:path";
import {
  buildReclassifyProposal,
  dedupeBookmarks,
  extractSummaryFromHtml,
  fetchPage,
  parseBookmarkHtml,
  wikiDirFromFolder,
  type BookmarkEntry,
} from "./lib/import-firefox.js";
import { resolveSlugFilename } from "./lib/slug.js";

interface CliOptions {
  inputPath: string;
  repoRoot: string;
  dryRun: boolean;
  concurrency: number;
  captureDate: string;
}

interface ManifestEntry {
  url: string;
  title: string;
  folderPath: string[];
  wikiPath: string;
  capturePath?: string;
  status: "imported" | "dead" | "skipped";
  reason?: string;
}

interface ImportManifest {
  sourceCapture: string;
  importDate: string;
  totals: {
    parsed: number;
    unique: number;
    imported: number;
    dead: number;
    skipped: number;
    duplicates: number;
  };
  entries: ManifestEntry[];
  skipped: Array<{ url: string; title: string; folderPath: string[]; reason: string }>;
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
    console.error("Usage: npm run kb:import-firefox -- <bookmarks.html> [--dry-run] [--concurrency N]");
    process.exit(1);
  }

  const repoRoot = process.cwd();
  const captureDate = new Date().toISOString().slice(0, 10);

  return {
    inputPath: path.resolve(inputPath),
    repoRoot,
    dryRun,
    concurrency,
    captureDate,
  };
}

function ensureDir(dir: string, dryRun: boolean): void {
  if (!dryRun && !fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function readExistingUrls(repoRoot: string, captureDate: string): Set<string> {
  const urls = new Set<string>();

  for (const url of scanExistingWikiUrls(repoRoot).keys()) {
    urls.add(url);
  }

  const manifestPath = path.join(repoRoot, "sources", `firefox-bookmarks-${captureDate}.manifest.json`);

  if (fs.existsSync(manifestPath)) {
    try {
      const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8")) as ImportManifest;
      for (const entry of manifest.entries) {
        urls.add(entry.url);
      }
    } catch {
      // ignore corrupt manifest
    }
  }

  return urls;
}

function scanExistingWikiUrls(repoRoot: string): Map<string, string> {
  const wikiBookmarks = path.join(repoRoot, "wiki", "bookmarks");
  const byUrl = new Map<string, string>();
  if (!fs.existsSync(wikiBookmarks)) {
    return byUrl;
  }

  function walk(dir: string): void {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        walk(full);
        continue;
      }
      if (!entry.name.endsWith(".md")) continue;
      const content = fs.readFileSync(full, "utf8");
      const match = content.match(/^url:\s*"([^"]+)"/m);
      if (!match) continue;
      const rel = path.relative(repoRoot, full).replace(/\\/g, "/");
      byUrl.set(match[1], rel);
    }
  }

  walk(wikiBookmarks);
  return byUrl;
}

function buildWikiFrontmatter(
  entry: BookmarkEntry,
  captureDate: string,
  alsoIn: string[],
): string {
  const tags = [
    "bookmark",
    "import",
    "firefox",
    ...entry.folderPath.map((f) => f.toLowerCase().replace(/\s+/g, "-")).slice(0, 3),
  ];

  const lines = [
    "---",
    `date: ${captureDate}`,
    "type: bookmark",
    `tags: [${tags.map((t) => JSON.stringify(t.replace(/"/g, ""))).join(", ")}]`,
    `url: ${JSON.stringify(entry.url)}`,
    `source_folder: ${JSON.stringify(entry.folderPath.join("/"))}`,
  ];

  if (alsoIn.length > 0) {
    lines.push(`also_in: [${alsoIn.map((f) => JSON.stringify(f)).join(", ")}]`);
  }

  lines.push("---");
  return lines.join("\n");
}

function buildWikiBody(title: string, url: string, summary: string): string {
  const safeTitle = title || url;
  const summaryText = summary || "No excerpt available from fetch.";
  return `# ${safeTitle}\n\n${url}\n\n## Summary\n\n${summaryText}\n`;
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

  const html = fs.readFileSync(options.inputPath, "utf8");
  const parsed = parseBookmarkHtml(html);
  const { unique, duplicates } = dedupeBookmarks(parsed.bookmarks);

  const alsoInByUrl = new Map<string, string[]>();
  for (const dup of duplicates) {
    alsoInByUrl.set(dup.canonical.url, dup.alsoIn);
  }

  const sourceCaptureName = `firefox-bookmarks-${options.captureDate}.html`;
  const sourceCapturePath = path.join(options.repoRoot, "sources", sourceCaptureName);
  const manifestPath = path.join(
    options.repoRoot,
    "sources",
    `firefox-bookmarks-${options.captureDate}.manifest.json`,
  );
  const proposalPath = path.join(
    options.repoRoot,
    "artifacts",
    "tacos-work",
    "firefox-bookmark-import",
    "reclassify-proposal.json",
  );

  if (!options.dryRun) {
    ensureDir(path.dirname(sourceCapturePath), options.dryRun);
    fs.copyFileSync(options.inputPath, sourceCapturePath);
  }

  const existingUrls = readExistingUrls(options.repoRoot, options.captureDate);
  const existingWikiByUrl = scanExistingWikiUrls(options.repoRoot);
  const priorManifestEntries: ManifestEntry[] = [];

  const manifestPathOnDisk = path.join(
    options.repoRoot,
    "sources",
    `firefox-bookmarks-${options.captureDate}.manifest.json`,
  );
  if (fs.existsSync(manifestPathOnDisk)) {
    try {
      const prior = JSON.parse(fs.readFileSync(manifestPathOnDisk, "utf8")) as ImportManifest;
      priorManifestEntries.push(...prior.entries);
    } catch {
      // ignore
    }
  }

  const manifestEntries: ManifestEntry[] = [...priorManifestEntries];
  let imported = 0;
  let dead = 0;
  let skippedExisting = 0;

  const toProcess = unique.filter((entry) => {
    if (existingUrls.has(entry.url)) {
      skippedExisting += 1;
      manifestEntries.push({
        url: entry.url,
        title: entry.title,
        folderPath: entry.folderPath,
        wikiPath: "",
        status: "skipped",
        reason: "already_imported",
      });
      return false;
    }
    return true;
  });

  console.log(
    `Parsed ${parsed.bookmarks.length} links, ${unique.length} unique, processing ${toProcess.length} (${skippedExisting} already imported)`,
  );

  if (options.dryRun) {
    console.log("\nDry run — no files written, no network fetch.");
    console.log(`  parse skipped: ${parsed.skipped.length}`);
    console.log(`  duplicates: ${duplicates.length}`);
    console.log(`  proposal folders flagged: ${buildReclassifyProposal(unique).length}`);
    return;
  }

  await mapPool(toProcess, options.concurrency, async (entry) => {
    if (existingWikiByUrl.has(entry.url)) {
      skippedExisting += 1;
      return;
    }

    const fetchResult = await fetchPage(entry.url);

    if (!fetchResult.ok || !fetchResult.html) {
      dead += 1;
      manifestEntries.push({
        url: entry.url,
        title: entry.title,
        folderPath: entry.folderPath,
        wikiPath: "",
        status: "dead",
        reason: fetchResult.reason ?? "fetch_failed",
      });
      return;
    }

    const summary = extractSummaryFromHtml(fetchResult.html);
    const wikiRelDir = wikiDirFromFolder(entry.folderPath);
    const wikiAbsDir = path.join(options.repoRoot, wikiRelDir);
    const captureRelDir = "sources";
    const captureAbsDir = path.join(options.repoRoot, captureRelDir);

    if (!options.dryRun) {
      ensureDir(wikiAbsDir, options.dryRun);
      ensureDir(captureAbsDir, options.dryRun);
    }

    const wikiFilename = resolveSlugFilename(entry.title || entry.url, wikiAbsDir);
    const wikiRelPath = path.posix.join(wikiRelDir.replace(/\\/g, "/"), wikiFilename);
    const wikiAbsPath = path.join(wikiAbsDir, wikiFilename);

    const captureFilename = resolveSlugFilename(entry.title || entry.url, captureAbsDir, ".html");
    const captureRelPath = path.posix.join(captureRelDir, captureFilename);
    const captureAbsPath = path.join(captureAbsDir, captureFilename);

    const alsoIn = alsoInByUrl.get(entry.url) ?? [];
    const frontmatter = buildWikiFrontmatter(entry, options.captureDate, alsoIn);
    const wikiContent = `${frontmatter}\n\n${buildWikiBody(entry.title, entry.url, summary)}`;

    const truncatedHtml = fetchResult.html.slice(0, 200_000);

    if (!options.dryRun) {
      fs.writeFileSync(wikiAbsPath, wikiContent, "utf8");
      fs.writeFileSync(captureAbsPath, truncatedHtml, "utf8");
    }

    imported += 1;
    manifestEntries.push({
      url: entry.url,
      title: entry.title,
      folderPath: entry.folderPath,
      wikiPath: wikiRelPath.replace(/\\/g, "/"),
      capturePath: captureRelPath,
      status: "imported",
    });

    process.stdout.write(`Imported: ${entry.title}\n`);
  });

  const manifest: ImportManifest = {
    sourceCapture: `sources/${sourceCaptureName}`,
    importDate: options.captureDate,
    totals: {
      parsed: parsed.bookmarks.length,
      unique: unique.length,
      imported: dedupedManifestEntries(manifestEntries).filter((e) => e.status === "imported").length,
      dead: dedupedManifestEntries(manifestEntries).filter((e) => e.status === "dead").length,
      skipped:
        parsed.skipped.length +
        dedupedManifestEntries(manifestEntries).filter((e) => e.status === "skipped").length,
      duplicates: duplicates.length,
    },
    entries: dedupedManifestEntries(manifestEntries),
    skipped: parsed.skipped,
  };

  const proposal = buildReclassifyProposal(unique);

  if (!options.dryRun) {
    ensureDir(path.dirname(proposalPath), options.dryRun);
    fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf8");
    fs.writeFileSync(proposalPath, JSON.stringify(proposal, null, 2), "utf8");

    appendNavigation(options.repoRoot, options.captureDate, manifest.totals);
  }

  console.log("\nImport complete:");
  console.log(`  imported: ${imported}`);
  console.log(`  dead: ${dead}`);
  console.log(`  skipped (parse): ${parsed.skipped.length}`);
  console.log(`  skipped (existing): ${skippedExisting}`);
  console.log(`  manifest: ${manifestPath}`);
  console.log(`  proposal: ${proposalPath}`);
}

function dedupedManifestEntries(entries: ManifestEntry[]): ManifestEntry[] {
  const byUrl = new Map<string, ManifestEntry>();
  const rank = (entry: ManifestEntry): number => {
    if (entry.status === "imported") return 3;
    if (entry.status === "dead") return 2;
    return 1;
  };

  for (const entry of entries) {
    const existing = byUrl.get(entry.url);
    if (!existing || rank(entry) > rank(existing)) {
      byUrl.set(entry.url, entry);
    }
  }

  return [...byUrl.values()];
}

function appendNavigation(
  repoRoot: string,
  date: string,
  totals: ImportManifest["totals"],
): void {
  const indexPath = path.join(repoRoot, "wiki", "index.md");
  const logPath = path.join(repoRoot, "wiki", "log.md");

  const indexLine = `- ${date} — Firefox import: ${totals.imported} bookmarks imported, ${totals.dead} dead links excluded → see \`wiki/bookmarks/\``;
  const logLine = `- ${date} — firefox-bookmark-import: ${totals.imported} imported, ${totals.dead} dead, ${totals.skipped} skipped`;

  const indexContent = fs.readFileSync(indexPath, "utf8");
  if (!indexContent.includes("Firefox import:")) {
    fs.appendFileSync(indexPath, `\n${indexLine}\n`, "utf8");
  }

  const logContent = fs.readFileSync(logPath, "utf8");
  if (!logContent.includes("firefox-bookmark-import:")) {
    fs.appendFileSync(logPath, `\n${logLine}\n`, "utf8");
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
