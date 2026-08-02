import fs from "node:fs";
import path from "node:path";

interface RenameEntry {
  from: string;
  to: string;
  reason?: string;
}

interface RenamePlan {
  renames: RenameEntry[];
}

function main(): void {
  const planPath =
    process.argv[2] ??
    "artifacts/tacos-work/firefox-bookmark-import/nl-slug-renames.json";
  const root = process.cwd();
  const plan = JSON.parse(fs.readFileSync(path.join(root, planPath), "utf8")) as RenamePlan;

  for (const entry of plan.renames) {
    const from = path.join(root, entry.from);
    const to = path.join(root, entry.to);
    if (!fs.existsSync(from)) {
      console.log(`skip missing: ${entry.from}`);
      continue;
    }
    if (fs.existsSync(to)) {
      console.log(`skip exists: ${entry.to}`);
      continue;
    }
    fs.mkdirSync(path.dirname(to), { recursive: true });
    fs.renameSync(from, to);
    console.log(`${entry.from} -> ${entry.to}`);
  }
}

main();
