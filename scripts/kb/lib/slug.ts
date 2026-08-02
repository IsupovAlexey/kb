import fs from "node:fs";
import path from "node:path";

const FILLER_WORDS = new Set([
  "a",
  "an",
  "and",
  "at",
  "by",
  "for",
  "in",
  "of",
  "on",
  "or",
  "the",
  "to",
  "with",
]);

const MAX_SLUG_LENGTH = 64;

export function toSlugBase(title: string): string {
  const normalized = title
    .toLowerCase()
    .normalize("NFKD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9\s-]/g, " ")
    .trim()
    .split(/\s+/)
    .filter((word) => word.length > 0 && !FILLER_WORDS.has(word))
    .join("-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");

  if (!normalized) {
    return "untitled";
  }

  return normalized.slice(0, MAX_SLUG_LENGTH).replace(/-+$/g, "");
}

export function resolveSlugFilename(
  title: string,
  targetDir: string,
  extension = ".md",
): string {
  const base = toSlugBase(title);
  let candidate = `${base}${extension}`;
  let suffix = 2;

  while (fs.existsSync(path.join(targetDir, candidate))) {
    const suffixed = `${base}-${suffix}`;
    const trimmed =
      suffixed.length > MAX_SLUG_LENGTH
        ? suffixed.slice(0, MAX_SLUG_LENGTH).replace(/-+$/g, "")
        : suffixed;
    candidate = `${trimmed}${extension}`;
    suffix += 1;
  }

  return candidate;
}
