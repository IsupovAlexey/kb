import fs from "node:fs";
import path from "node:path";

const root = process.cwd();
const bookmarksRoot = path.join(root, "wiki/bookmarks");

interface Entry {
  path: string;
  title: string;
  url: string;
  source_folder: string;
}

const entries: Entry[] = [];

function walk(dir: string): void {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walk(full);
      continue;
    }
    if (!entry.name.endsWith(".md")) continue;
    const content = fs.readFileSync(full, "utf8");
    const rel = path.relative(root, full).replace(/\\/g, "/");
    const titleMatch = content.match(/^# (.+)/m);
    const urlMatch = content.match(/^url: "([^"]+)"/m);
    const folderMatch = content.match(/^source_folder: "([^"]+)"/m);
    entries.push({
      path: rel,
      title: (titleMatch?.[1] ?? entry.name).slice(0, 120),
      url: urlMatch?.[1] ?? "",
      source_folder: folderMatch?.[1] ?? "",
    });
  }
}

walk(bookmarksRoot);

const out = path.join(
  root,
  "artifacts/tacos-work/firefox-bookmark-import/bookmark-inventory.json",
);
fs.writeFileSync(out, JSON.stringify({ count: entries.length, entries }, null, 2));
console.log(`Wrote ${entries.length} entries to ${out}`);

// folder summary
const byFolder: Record<string, number> = {};
for (const e of entries) {
  const folder = e.path.replace(/^wiki\/bookmarks\//, "").replace(/\/[^/]+$/, "");
  byFolder[folder] = (byFolder[folder] ?? 0) + 1;
}
console.log("\nFolder counts:");
for (const [f, c] of Object.entries(byFolder).sort((a, b) => b[1] - a[1])) {
  console.log(`${c}\t${f}`);
}
