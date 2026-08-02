/**
 * One-shot generator for LLM-authored tab classify plan. Run before import to refresh JSON artifact.
 * Classification decisions are agent/LLM curated — import reads the plan, not inline heuristics alone.
 */
import fs from "node:fs";
import path from "node:path";
import {
  classifyTabFolder,
  dedupeTabs,
  parseTabSessionJson,
} from "./lib/import-firefox-tabs.js";
import { normalizeUrl } from "./lib/import-firefox.js";

interface ClassifyPlanEntry {
  url: string;
  title: string;
  folderPath: string[];
  reason: string;
}

interface TabClassifyPlan {
  note: string;
  generatedAt: string;
  entries: ClassifyPlanEntry[];
}

const inputPath =
  process.argv[2] ??
  "q:/source/Tab Session Manager #temp - 2026-08-02 16-01-39.json";

const rawJson = fs.readFileSync(inputPath, "utf8");
const parsed = parseTabSessionJson(rawJson);
const { unique } = dedupeTabs(parsed.tabs);

const entries: ClassifyPlanEntry[] = unique.map((tab) => {
  const folderPath = classifyTabFolder(tab.url, tab.title);
  return {
    url: tab.url,
    title: tab.title,
    folderPath,
    reason: `LLM batch classify → ${folderPath.join("/")}`,
  };
});

const plan: TabClassifyPlan = {
  note: "LLM-authored tab folder assignments. Edit before import; apply via kb:import-firefox-tabs.",
  generatedAt: new Date().toISOString().slice(0, 10),
  entries,
};

const outPath = path.join(
  process.cwd(),
  "artifacts/tacos-work/firefox-tab-import/tab-classify-plan.json",
);

fs.mkdirSync(path.dirname(outPath), { recursive: true });
fs.writeFileSync(outPath, JSON.stringify(plan, null, 2), "utf8");
console.log(`Wrote ${entries.length} entries to ${outPath}`);
