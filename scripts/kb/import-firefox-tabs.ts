import fs from "node:fs";
import path from "node:path";
import {
  checkUrlHealth,
  extractSummaryFromHtml,
  fetchPage,
  normalizeUrl,
  wikiDirFromFolder,
} from "./lib/import-firefox.js";
import {
  dedupeTabs,
  isWorkPinnedTab,
  parseTabSessionJson,
  tabToBookmarkEntry,
  type TabClassifyPlan,
} from "./lib/import-firefox-tabs.js";
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
  importDate: string;
  totals: {
    parsed: number;
    parseSkipped: number;
    unique: number;
    imported: number;
    dead: number;
    skipped: number;
    duplicates: number;
    workPinned: number;
    existingWiki: number;
  };
  entries: ManifestEntry[];
  skipped: Array<{ url: string; title: string; pinned: boolean; reason: string }>;
}

interface PipelineCounts {
  parsed: number;
  parseSkipped: number;
  unique: number;
  duplicates: number;
  existingWiki: number;
  workPinned: number;
  toImport: number;
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
    console.error(
      "Usage: npm run kb:import-firefox-tabs -- <tab-session.json> [--dry-run] [--concurrency N]",
    );
    process.exit(1);
  }

  return {
    inputPath: path.resolve(inputPath),
    repoRoot: process.cwd(),
    dryRun,
    concurrency,
    captureDate: "2026-08-02",
  };
}

function ensureDir(dir: string): void {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
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
      if (!entry.name.endsWith(".md")) {
        continue;
      }
      const content = fs.readFileSync(full, "utf8");
      const match = content.match(/^url:\s*"([^"]+)"/m);
      if (!match) {
        continue;
      }
      const rel = path.relative(repoRoot, full).replace(/\\/g, "/");
      byUrl.set(match[1], rel);
      byUrl.set(normalizeUrl(match[1]), rel);
    }
  }

  walk(wikiBookmarks);
  return byUrl;
}

function loadClassifyPlan(repoRoot: string): TabClassifyPlan | undefined {
  const planPath = path.join(
    repoRoot,
    "artifacts/tacos-work/firefox-tab-import/tab-classify-plan.json",
  );
  if (!fs.existsSync(planPath)) {
    return undefined;
  }
  return JSON.parse(fs.readFileSync(planPath, "utf8")) as TabClassifyPlan;
}

function readPriorManifestEntries(repoRoot: string, captureDate: string): Map<string, ManifestEntry> {
  const byUrl = new Map<string, ManifestEntry>();
  const manifestPath = path.join(repoRoot, "artifacts", "kb-import", `firefox-tabs-${captureDate}.manifest.json`);
  if (!fs.existsSync(manifestPath)) {
    return byUrl;
  }

  try {
    const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8")) as ImportManifest;
    for (const entry of manifest.entries) {
      byUrl.set(entry.url, entry);
      byUrl.set(normalizeUrl(entry.url), entry);
    }
  } catch {
    // ignore corrupt manifest
  }

  return byUrl;
}

function isSettledEntry(entry: ManifestEntry | undefined): boolean {
  return entry?.status === "imported" || entry?.status === "dead";
}

function buildWikiFrontmatter(
  url: string,
  title: string,
  folderPath: string[],
  captureDate: string,
): string {
  const tags = [
    "tab-import",
    "firefox",
    "import",
    captureDate,
    ...folderPath.slice(0, 2),
  ];

  return [
    "---",
    `date: ${captureDate}`,
    "type: tab",
    `tags: [${tags.map((t) => JSON.stringify(t)).join(", ")}]`,
    `url: ${JSON.stringify(url)}`,
    "---",
  ].join("\n");
}

function buildWikiBody(title: string, url: string, summary: string): string {
  const safeTitle = title || url;
  let summaryText = summary;
  if (!summaryText) {
    try {
      const host = new URL(url).hostname.replace(/^www\./, "");
      summaryText = `${safeTitle} — captured from open tab (${host}).`;
    } catch {
      summaryText = `${safeTitle} — captured from open tab.`;
    }
  }
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

function planPipeline(
  tabs: ReturnType<typeof parseTabSessionJson>["tabs"],
  existingWikiByUrl: Map<string, string>,
  priorEntries: Map<string, ManifestEntry>,
  classifyPlan?: TabClassifyPlan,
): {
  counts: PipelineCounts;
  toProcess: ReturnType<typeof parseTabSessionJson>["tabs"];
  manifestEntries: ManifestEntry[];
  skipped: ImportManifest["skipped"];
} {
  const { unique, duplicateCount } = dedupeTabs(tabs);
  const manifestEntries: ManifestEntry[] = [];
  const skipped: ImportManifest["skipped"] = [];
  let existingWiki = 0;
  let workPinned = 0;

  const toProcess = unique.filter((tab) => {
    const prior =
      priorEntries.get(tab.url) ?? priorEntries.get(normalizeUrl(tab.url));
    if (isSettledEntry(prior)) {
      return false;
    }

    const folderPath = tabToBookmarkEntry(tab, classifyPlan).folderPath;

    const existingPath =
      existingWikiByUrl.get(tab.url) ?? existingWikiByUrl.get(normalizeUrl(tab.url));
    if (existingPath) {
      existingWiki += 1;
      manifestEntries.push({
        url: tab.url,
        title: tab.title,
        folderPath,
        wikiPath: existingPath,
        status: "skipped",
        reason: "existing-wiki",
      });
      return false;
    }

    if (isWorkPinnedTab(tab.url, tab.pinned)) {
      workPinned += 1;
      skipped.push({ url: tab.url, title: tab.title, pinned: tab.pinned, reason: "work-pinned" });
      manifestEntries.push({
        url: tab.url,
        title: tab.title,
        folderPath,
        wikiPath: "",
        status: "skipped",
        reason: "work-pinned",
      });
      return false;
    }

    return true;
  });

  return {
    counts: {
      parsed: tabs.length,
      parseSkipped: 0,
      unique: unique.length,
      duplicates: duplicateCount,
      existingWiki,
      workPinned,
      toImport: toProcess.length,
    },
    toProcess,
    manifestEntries,
    skipped,
  };
}

function printDryRun(
  parseSkipped: number,
  counts: PipelineCounts,
  toProcess: ReturnType<typeof parseTabSessionJson>["tabs"],
  classifyPlan?: TabClassifyPlan,
): void {
  console.log("\nDry run — pipeline stage counts:");
  console.log(`  1. parsed tabs: ${counts.parsed}`);
  console.log(`  2. parse skipped: ${parseSkipped}`);
  console.log(`  3. unique after dedupe: ${counts.unique} (duplicates: ${counts.duplicates})`);
  console.log(`  4. existing-wiki skipped: ${counts.existingWiki}`);
  console.log(`  5. work-pinned skipped: ${counts.workPinned}`);
  console.log(`  6. to-import (fetch+wiki): ${counts.toImport}`);

  const themes = new Set<string>();
  for (const tab of toProcess) {
    const folder = tabToBookmarkEntry(tab, classifyPlan).folderPath;
    themes.add(folder[0]);
  }
  console.log(`  7. thematic folders (top-level): ${[...themes].sort().join(", ")}`);
  console.log("\nNo files written, no network fetch.");
}

function computeTotals(
  parsedTabCount: number,
  parseSkipped: number,
  unique: number,
  duplicateCount: number,
  entries: ManifestEntry[],
): ImportManifest["totals"] {
  const imported = entries.filter((e) => e.status === "imported").length;
  const dead = entries.filter((e) => e.status === "dead").length;
  const workPinned = entries.filter((e) => e.reason === "work-pinned").length;
  const existingWiki = entries.filter((e) => e.reason === "existing-wiki").length;
  const pipelineSkipped = entries.filter((e) => e.status === "skipped").length;

  return {
    parsed: parsedTabCount,
    parseSkipped,
    unique,
    imported,
    dead,
    skipped: parseSkipped + pipelineSkipped,
    duplicates: duplicateCount,
    workPinned,
    existingWiki,
  };
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

function appendNavigation(repoRoot: string, date: string, totals: ImportManifest["totals"]): void {
  const indexPath = path.join(repoRoot, "wiki", "index.md");
  const logPath = path.join(repoRoot, "wiki", "log.md");

  const indexLine = `- ${date} — Firefox tab import: ${totals.imported} tabs imported, ${totals.dead} dead, ${totals.workPinned} work-pinned skipped → see \`wiki/bookmarks/\``;
  const logLine = `- ${date} — firefox-tab-import: ${totals.imported} imported, ${totals.dead} dead, ${totals.skipped} skipped, ${totals.workPinned} work-pinned`;

  const indexContent = fs.readFileSync(indexPath, "utf8");
  if (!indexContent.includes("Firefox tab import:")) {
    fs.appendFileSync(indexPath, `\n${indexLine}\n`, "utf8");
  }

  const logContent = fs.readFileSync(logPath, "utf8");
  if (!logContent.includes("firefox-tab-import:")) {
    fs.appendFileSync(logPath, `\n${logLine}\n`, "utf8");
  }
}

async function main(): Promise<void> {
  const options = parseArgs(process.argv);

  if (!fs.existsSync(options.inputPath)) {
    console.error(`Input not found: ${options.inputPath}`);
    process.exit(1);
  }

  const rawJson = fs.readFileSync(options.inputPath, "utf8");
  const parsed = parseTabSessionJson(rawJson);
  const { unique, duplicateCount } = dedupeTabs(parsed.tabs);

  const manifestPath = path.join(
    options.repoRoot,
    "artifacts",
    "kb-import",
    `firefox-tabs-${options.captureDate}.manifest.json`,
  );

  const classifyPlan = loadClassifyPlan(options.repoRoot);
  const existingWikiByUrl = scanExistingWikiUrls(options.repoRoot);
  const priorEntries = readPriorManifestEntries(options.repoRoot, options.captureDate);

  const planned = planPipeline(unique, existingWikiByUrl, priorEntries, classifyPlan);
  planned.counts.parseSkipped = parsed.skipped.length;

  if (options.dryRun) {
    printDryRun(parsed.skipped.length, planned.counts, planned.toProcess, classifyPlan);
    return;
  }


  const manifestEntries: ManifestEntry[] = [
    ...[...priorEntries.values()].filter(
      (entry, index, all) => all.findIndex((e) => e.url === entry.url) === index,
    ),
    ...planned.manifestEntries,
  ];
  const skipped = [...parsed.skipped.map((s) => ({ ...s })), ...planned.skipped];
  let imported = 0;
  let dead = 0;

  console.log(
    `Parsed ${parsed.tabs.length} tabs, ${unique.length} unique, processing ${planned.toProcess.length}`,
  );

  await mapPool(planned.toProcess, options.concurrency, async (tab) => {
    const folderPath = tabToBookmarkEntry(tab, classifyPlan).folderPath;
    const wikiRelDir = wikiDirFromFolder(folderPath);
    if (wikiRelDir.includes("/tabs")) {
      throw new Error(`Refusing flat tabs/ layout for ${tab.url}`);
    }

    const health = await checkUrlHealth(tab.url);
    if (!health.ok) {
      dead += 1;
      manifestEntries.push({
        url: tab.url,
        title: tab.title,
        folderPath,
        wikiPath: "",
        status: "dead",
        reason: health.reason ?? "health_check_failed",
      });
      return;
    }

    const fetchResult = await fetchPage(tab.url);
    if (!fetchResult.ok || !fetchResult.html) {
      dead += 1;
      manifestEntries.push({
        url: tab.url,
        title: tab.title,
        folderPath,
        wikiPath: "",
        status: "dead",
        reason: fetchResult.reason ?? "fetch_failed",
      });
      return;
    }

    const summary = extractSummaryFromHtml(fetchResult.html);
    const wikiAbsDir = path.join(options.repoRoot, wikiRelDir);
    ensureDir(wikiAbsDir);

    const wikiFilename = resolveSlugFilename(tab.title || tab.url, wikiAbsDir);
    const wikiRelPath = path.posix.join(wikiRelDir.replace(/\\/g, "/"), wikiFilename);
    const wikiAbsPath = path.join(wikiAbsDir, wikiFilename);

    const frontmatter = buildWikiFrontmatter(tab.url, tab.title, folderPath, options.captureDate);
    const wikiContent = `${frontmatter}\n\n${buildWikiBody(tab.title, tab.url, summary)}`;

    fs.writeFileSync(wikiAbsPath, wikiContent, "utf8");

    imported += 1;
    manifestEntries.push({
      url: tab.url,
      title: tab.title,
      folderPath,
      wikiPath: wikiRelPath.replace(/\\/g, "/"),
      status: "imported",
    });

    process.stdout.write(`Imported [${folderPath.join("/")}]: ${tab.title}\n`);
  });

  const finalEntries = dedupedManifestEntries(manifestEntries);
  const totals = computeTotals(
    parsed.tabs.length,
    parsed.skipped.length,
    unique.length,
    duplicateCount,
    finalEntries,
  );

  const manifest: ImportManifest = {
    importDate: options.captureDate,
    totals,
    entries: finalEntries,
    skipped,
  };

  fs.mkdirSync(path.dirname(manifestPath), { recursive: true });
  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf8");
  appendNavigation(options.repoRoot, options.captureDate, totals);

  const balanceOk =
    totals.unique ===
    totals.imported + totals.dead + totals.workPinned + totals.existingWiki;

  console.log("\nImport complete:");
  console.log(`  imported: ${imported}`);
  console.log(`  dead: ${dead}`);
  console.log(`  work-pinned skipped: ${totals.workPinned}`);
  console.log(`  existing-wiki skipped: ${totals.existingWiki}`);
  console.log(`  parse skipped: ${parsed.skipped.length}`);
  console.log(`  manifest: ${manifestPath}`);
  console.log(
    `  balance check: unique ${totals.unique} = imported ${totals.imported} + dead ${totals.dead} + work-pinned ${totals.workPinned} + existing-wiki ${totals.existingWiki} (${balanceOk ? "ok" : "mismatch"})`,
  );
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
