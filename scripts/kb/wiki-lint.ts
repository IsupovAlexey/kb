#!/usr/bin/env node
import path from "node:path";
import { lintWiki } from "./lib/wiki-lint.js";

function usage(): never {
  console.error(`Usage: wiki-lint [--wiki <path>]

Report semantic wiki issues: broken wikilinks, orphan pages, and index gaps.
Prints a human-readable report to stdout and exits 1 when any issue is found.

Examples:
  wiki-lint
  wiki-lint --wiki wiki`);
  process.exit(2);
}

function parseWikiRoot(argv: string[]): string {
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === "-h" || arg === "--help") usage();
    if (arg === "--wiki") {
      const value = argv[i + 1];
      if (!value) usage();
      return path.resolve(value);
    }
  }
  return path.resolve("wiki");
}

function main(): void {
  const wikiRoot = parseWikiRoot(process.argv.slice(2));
  const issues = lintWiki(wikiRoot);

  if (issues.length === 0) {
    console.log(`wiki-lint: no issues found under ${wikiRoot}`);
    process.exit(0);
  }

  console.log(`wiki-lint: ${issues.length} issue(s) under ${wikiRoot}\n`);

  for (const issue of issues) {
    console.log(`[${issue.kind}] ${issue.file}`);
    console.log(`  ${issue.detail}`);
  }

  process.exit(1);
}

main();
