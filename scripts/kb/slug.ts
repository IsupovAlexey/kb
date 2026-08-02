#!/usr/bin/env node
import path from "node:path";
import { resolveSlugFilename } from "./lib/slug.js";

function usage(): never {
  console.error(`Usage: slug <title> [--dir <path>] [--ext <extension>]

Generate a kebab-case markdown filename for a wiki or capture title.
Collision suffixes use -2, -3, ... when the base name already exists.

Examples:
  slug "Karpathy LLM Wiki"
  slug "Karpathy LLM Wiki" --dir wiki/bookmarks
  slug "My Note" --dir sources --ext .md`);
  process.exit(2);
}

function parseArgs(argv: string[]): {
  title: string;
  dir: string;
  ext: string;
} {
  if (argv.length === 0 || argv[0] === "-h" || argv[0] === "--help") {
    usage();
  }

  let title = "";
  let dir = ".";
  let ext = ".md";
  const titleParts: string[] = [];

  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === "--dir") {
      const value = argv[i + 1];
      if (!value) usage();
      dir = value;
      i += 1;
      continue;
    }
    if (arg === "--ext") {
      const value = argv[i + 1];
      if (!value) usage();
      ext = value.startsWith(".") ? value : `.${value}`;
      i += 1;
      continue;
    }
    titleParts.push(arg);
  }

  title = titleParts.join(" ").trim();
  if (!title) usage();

  return { title, dir: path.resolve(dir), ext };
}

function main(): void {
  const { title, dir, ext } = parseArgs(process.argv.slice(2));
  const filename = resolveSlugFilename(title, dir, ext);
  process.stdout.write(`${filename}\n`);
}

main();
