import fs from "node:fs";
import path from "node:path";

export type WikiLintIssue = {
  kind: "orphan" | "broken-wikilink" | "index-gap";
  file: string;
  detail: string;
};

const NAV_FILES = new Set(["index.md", "log.md", "schema.md"]);
const WIKILINK_RE = /\[\[([^\]|#]+)(?:#[^\]|]+)?(?:\|[^\]]+)?\]\]/g;
const MARKDOWN_LINK_RE = /\[[^\]]+\]\(([^)]+)\)/g;

function listWikiMarkdownFiles(wikiRoot: string): string[] {
  const results: string[] = [];

  function walk(dir: string): void {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const fullPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "assets") continue;
        walk(fullPath);
        continue;
      }
      if (entry.isFile() && entry.name.endsWith(".md")) {
        results.push(path.relative(wikiRoot, fullPath).replace(/\\/g, "/"));
      }
    }
  }

  walk(wikiRoot);
  return results;
}

function readFile(wikiRoot: string, relPath: string): string {
  return fs.readFileSync(path.join(wikiRoot, relPath), "utf8");
}

function normalizeLinkTarget(target: string): string {
  const withoutAnchor = target.split("#")[0]?.trim() ?? "";
  const withoutQuery = withoutAnchor.split("?")[0]?.trim() ?? "";
  if (!withoutQuery) return "";
  if (withoutQuery.endsWith(".md")) return withoutQuery.replace(/\\/g, "/");
  return `${withoutQuery.replace(/\\/g, "/")}.md`;
}

function resolveWikiTarget(
  wikiRoot: string,
  sourceRelPath: string,
  target: string,
): string | null {
  const normalized = normalizeLinkTarget(target);
  if (!normalized || normalized.startsWith("http://") || normalized.startsWith("https://")) {
    return null;
  }

  const sourceDir = path.dirname(sourceRelPath);
  const candidates = [
    path.join(wikiRoot, normalized),
    path.join(wikiRoot, sourceDir, normalized),
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate) && fs.statSync(candidate).isFile()) {
      return path.relative(wikiRoot, candidate).replace(/\\/g, "/");
    }
  }

  return null;
}

function collectInboundLinks(wikiRoot: string, files: string[]): Map<string, Set<string>> {
  const inbound = new Map<string, Set<string>>();

  for (const file of files) {
    const content = readFile(wikiRoot, file);

    for (const match of content.matchAll(WIKILINK_RE)) {
      const target = match[1]?.trim();
      if (!target) continue;
      const resolved = resolveWikiTarget(wikiRoot, file, target);
      if (!resolved) continue;
      if (!inbound.has(resolved)) inbound.set(resolved, new Set());
      inbound.get(resolved)!.add(file);
    }

    for (const match of content.matchAll(MARKDOWN_LINK_RE)) {
      const target = match[1]?.trim();
      if (!target || target.startsWith("http://") || target.startsWith("https://")) {
        continue;
      }
      const resolved = resolveWikiTarget(wikiRoot, file, target);
      if (!resolved) continue;
      if (!inbound.has(resolved)) inbound.set(resolved, new Set());
      inbound.get(resolved)!.add(file);
    }
  }

  return inbound;
}

function collectBrokenWikilinks(wikiRoot: string, files: string[]): WikiLintIssue[] {
  const issues: WikiLintIssue[] = [];

  for (const file of files) {
    const content = readFile(wikiRoot, file);

    for (const match of content.matchAll(WIKILINK_RE)) {
      const rawTarget = match[1]?.trim();
      if (!rawTarget) continue;
      const resolved = resolveWikiTarget(wikiRoot, file, rawTarget);
      if (resolved) continue;
      issues.push({
        kind: "broken-wikilink",
        file,
        detail: `unresolved wikilink [[${rawTarget}]]`,
      });
    }
  }

  return issues;
}

function isFirefoxImportPage(wikiRoot: string, file: string): boolean {
  const content = readFile(wikiRoot, file);
  return /tags:\s*\[[^\]]*["']import["']/.test(content);
}

function hasBatchImportIndexLine(wikiRoot: string): boolean {
  const indexPath = path.join(wikiRoot, "index.md");
  if (!fs.existsSync(indexPath)) return false;
  return readFile(wikiRoot, "index.md").includes("Firefox import:");
}

function collectOrphans(
  wikiRoot: string,
  files: string[],
  inbound: Map<string, Set<string>>,
): WikiLintIssue[] {
  const issues: WikiLintIssue[] = [];
  const batchImport = hasBatchImportIndexLine(wikiRoot);

  for (const file of files) {
    const base = path.basename(file).toLowerCase();
    if (NAV_FILES.has(base)) continue;
    if (file.startsWith("assets/")) continue;
    if (batchImport && isFirefoxImportPage(wikiRoot, file)) continue;
    const refs = inbound.get(file);
    if (!refs || refs.size === 0) {
      issues.push({
        kind: "orphan",
        file,
        detail: "no inbound wikilinks from index.md or other pages",
      });
    }
  }

  return issues;
}

function collectIndexGaps(wikiRoot: string, files: string[]): WikiLintIssue[] {
  const issues: WikiLintIssue[] = [];
  const indexPath = path.join(wikiRoot, "index.md");
  if (!fs.existsSync(indexPath)) {
    return [
      {
        kind: "index-gap",
        file: "index.md",
        detail: "missing wiki/index.md",
      },
    ];
  }

  const indexContent = readFile(wikiRoot, "index.md").toLowerCase();
  const batchImport = indexContent.includes("firefox import:");

  for (const file of files) {
    const base = path.basename(file).toLowerCase();
    if (NAV_FILES.has(base)) continue;
    if (file.startsWith("assets/")) continue;
    if (batchImport && isFirefoxImportPage(wikiRoot, file)) continue;

    const stem = path.basename(file, ".md");
    const relLower = file.toLowerCase();
    const mentioned =
      indexContent.includes(relLower) ||
      indexContent.includes(stem.toLowerCase()) ||
      indexContent.includes(`[[${stem.toLowerCase()}]]`);

    if (!mentioned) {
      issues.push({
        kind: "index-gap",
        file,
        detail: "missing entry in wiki/index.md",
      });
    }
  }

  return issues;
}

export function lintWiki(wikiRoot: string): WikiLintIssue[] {
  const files = listWikiMarkdownFiles(wikiRoot);
  const inbound = collectInboundLinks(wikiRoot, files);

  return [
    ...collectBrokenWikilinks(wikiRoot, files),
    ...collectOrphans(wikiRoot, files, inbound),
    ...collectIndexGaps(wikiRoot, files),
  ];
}
