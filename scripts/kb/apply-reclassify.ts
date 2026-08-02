import fs from "node:fs";
import path from "node:path";

/** Deterministic theme merges — parent creates dest, moves all files from src tree. */
const DIRECTORY_MOVES: Array<{ from: string; to: string }> = [
  { from: "wiki/bookmarks/toolbar/mtg", to: "wiki/bookmarks/magic/mtg" },
  { from: "wiki/bookmarks/toolbar/misc/nederlands", to: "wiki/bookmarks/languages/dutch" },
  { from: "wiki/bookmarks/toolbar/misc/h", to: "wiki/bookmarks/languages/armenian" },
  { from: "wiki/bookmarks/job-search", to: "wiki/bookmarks/careers/job-search" },
  { from: "wiki/bookmarks/lk", to: "wiki/bookmarks/personal/lk" },
];

const KEYWORD_MOVES: Array<{ pattern: RegExp; destDir: string }> = [
  {
    pattern: /untapped|mtg|magic|star city|starcitygames|goldfish|manabase|draft/i,
    destDir: "wiki/bookmarks/magic",
  },
  {
    pattern: /dutch|nederlands|nt2|inburgering/i,
    destDir: "wiki/bookmarks/languages/dutch",
  },
  {
    pattern: /armenian|armtv|հայ|hay/i,
    destDir: "wiki/bookmarks/languages/armenian",
  },
  {
    pattern: /interview|salary|career|relocation|job|hiring|whiteboard/i,
    destDir: "wiki/bookmarks/careers",
  },
];

function ensureDir(dir: string): void {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function moveDirectory(fromRel: string, toRel: string, root: string): void {
  const from = path.join(root, fromRel);
  const to = path.join(root, toRel);
  if (!fs.existsSync(from)) {
    console.log(`skip missing: ${fromRel}`);
    return;
  }
  ensureDir(path.dirname(to));
  if (fs.existsSync(to)) {
    for (const entry of fs.readdirSync(from, { withFileTypes: true })) {
      const srcPath = path.join(from, entry.name);
      const destPath = path.join(to, entry.name);
      if (entry.isDirectory()) {
        moveDirectory(
          path.join(fromRel, entry.name).replace(/\\/g, "/"),
          path.join(toRel, entry.name).replace(/\\/g, "/"),
          root,
        );
      } else {
        ensureDir(path.dirname(destPath));
        if (fs.existsSync(destPath)) {
          console.log(`skip file exists: ${destPath}`);
        } else {
          fs.renameSync(srcPath, destPath);
        }
      }
    }
    fs.rmSync(from, { recursive: true, force: true });
  } else {
    fs.renameSync(from, to);
  }
  console.log(`moved dir: ${fromRel} -> ${toRel}`);
}

function listMarkdownFiles(dir: string): string[] {
  const results: string[] = [];
  if (!fs.existsSync(dir)) return results;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...listMarkdownFiles(full));
    } else if (entry.name.endsWith(".md")) {
      results.push(full);
    }
  }
  return results;
}

function keywordReclassify(root: string): void {
  const bookmarksRoot = path.join(root, "wiki/bookmarks");
  const sources = [
    path.join(bookmarksRoot, "toolbar/misc"),
    path.join(bookmarksRoot, "utilities"),
    path.join(bookmarksRoot, "toolbar/misc"),
  ];

  for (const sourceDir of sources) {
    if (!fs.existsSync(sourceDir)) continue;
    for (const file of listMarkdownFiles(sourceDir)) {
      const content = fs.readFileSync(file, "utf8");
      const titleMatch = content.match(/^# (.+)/m);
      const urlMatch = content.match(/^url: "([^"]+)"/m);
      const haystack = `${titleMatch?.[1] ?? ""} ${urlMatch?.[1] ?? ""} ${content}`;

      for (const rule of KEYWORD_MOVES) {
        if (!rule.pattern.test(haystack)) continue;
        const destDir = path.join(root, rule.destDir);
        ensureDir(destDir);
        const dest = path.join(destDir, path.basename(file));
        if (dest === file || fs.existsSync(dest)) break;
        fs.renameSync(file, dest);
        console.log(`keyword move: ${path.relative(root, file)} -> ${rule.destDir}`);
        break;
      }
    }
  }
}

function main(): void {
  const root = process.cwd();
  for (const move of DIRECTORY_MOVES) {
    moveDirectory(move.from, move.to, root);
  }
  keywordReclassify(root);
  console.log("reclassify moves complete");
}

main();
