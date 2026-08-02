import fs from "node:fs";
import path from "node:path";

interface ManifestEntry {
  url: string;
  wikiPath: string;
  status: string;
}

interface ImportManifest {
  entries: ManifestEntry[];
}

function scanWikiUrlsByPath(repoRoot: string): Map<string, string> {
  const wikiBookmarks = path.join(repoRoot, "wiki", "bookmarks");
  const byUrl = new Map<string, string>();
  if (!fs.existsSync(wikiBookmarks)) return byUrl;

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

function main(): void {
  const captureDate = process.argv[2] ?? "2026-08-02";
  const root = process.cwd();
  const manifestPath = path.join(
    root,
    "sources",
    `firefox-bookmarks-${captureDate}.manifest.json`,
  );
  const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8")) as ImportManifest;
  const byUrl = scanWikiUrlsByPath(root);
  let updated = 0;

  for (const entry of manifest.entries) {
    if (entry.status !== "imported") continue;
    const wikiRel = byUrl.get(entry.url);
    if (!wikiRel) continue;
    const normalized = wikiRel.replace(/\\/g, "/");
    if (entry.wikiPath !== normalized) {
      entry.wikiPath = normalized;
      updated += 1;
    }
  }

  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf8");
  console.log(`Updated ${updated} manifest wikiPath entries`);
}

main();
