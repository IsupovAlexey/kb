import fs from "node:fs";
import path from "node:path";

interface DirectoryMove {
  from: string;
  to: string;
  reason?: string;
}

interface FileMove {
  from: string;
  to: string;
  reason?: string;
}

interface ReclassifyPlan {
  note?: string;
  directoryMoves: DirectoryMove[];
  fileMoves: FileMove[];
}

function ensureDir(dir: string): void {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function moveDirectory(fromRel: string, toRel: string, root: string): void {
  const from = path.join(root, fromRel);
  const to = path.join(root, toRel);
  if (!fs.existsSync(from)) {
    console.log(`skip missing dir: ${fromRel}`);
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
          console.log(`skip file exists: ${path.relative(root, destPath)}`);
        } else {
          fs.renameSync(srcPath, destPath);
          console.log(
            `merged file: ${path.join(fromRel, entry.name)} -> ${path.join(toRel, entry.name)}`,
          );
        }
      }
    }
    fs.rmSync(from, { recursive: true, force: true });
  } else {
    fs.renameSync(from, to);
  }
  console.log(`moved dir: ${fromRel} -> ${toRel}`);
}

function applyFileMove(root: string, fromRel: string, toRel: string): void {
  const from = path.join(root, fromRel);
  if (!fs.existsSync(from)) {
    console.log(`skip missing file: ${fromRel}`);
    return;
  }
  const to =
    toRel.endsWith("/") || toRel.endsWith(path.sep)
      ? path.join(root, toRel, path.basename(fromRel))
      : path.join(root, toRel);
  if (from === to) return;
  if (fs.existsSync(to)) {
    console.log(`skip exists: ${path.relative(root, to)}`);
    return;
  }
  ensureDir(path.dirname(to));
  fs.renameSync(from, to);
  console.log(`${fromRel} -> ${path.relative(root, to).replace(/\\/g, "/")}`);
}

function removeEmptyDirs(dir: string): void {
  if (!fs.existsSync(dir)) return;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      removeEmptyDirs(path.join(dir, entry.name));
    }
  }
  if (fs.readdirSync(dir).length === 0) {
    fs.rmSync(dir, { recursive: true, force: true });
  }
}

function main(): void {
  const planPath =
    process.argv[2] ??
    "artifacts/tacos-work/firefox-bookmark-import/llm-reclassify-plan.json";
  const root = process.cwd();
  const plan = JSON.parse(
    fs.readFileSync(path.join(root, planPath), "utf8"),
  ) as ReclassifyPlan;

  for (const move of plan.fileMoves) {
    applyFileMove(root, move.from, move.to);
  }

  for (const move of plan.directoryMoves) {
    moveDirectory(move.from, move.to, root);
  }

  removeEmptyDirs(path.join(root, "wiki/bookmarks/utilities"));
  removeEmptyDirs(path.join(root, "wiki/bookmarks/toolbar"));

  console.log("LLM reclassify apply complete");
}

main();
