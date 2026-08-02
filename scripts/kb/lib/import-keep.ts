import { toSlugBase } from "./slug.js";
import {
  extractSummaryFromHtml,
  normalizeUrl,
  type FetchResult,
  type HealthResult,
} from "./import-firefox.js";

export interface KeepLabel {
  name: string;
}

export interface KeepAnnotation {
  description?: string;
  source?: string;
  title?: string;
  url?: string;
}

export interface KeepAttachment {
  filePath: string;
  mimetype?: string;
}

export interface KeepListItem {
  text?: string;
  textHtml?: string;
  isChecked?: boolean;
}

export interface KeepNoteJson {
  color?: string;
  isTrashed?: boolean;
  isPinned?: boolean;
  isArchived?: boolean;
  title?: string;
  textContent?: string;
  textContentHtml?: string;
  labels?: KeepLabel[];
  annotations?: KeepAnnotation[];
  attachments?: KeepAttachment[];
  listContent?: KeepListItem[];
  createdTimestampUsec?: number;
  userEditedTimestampUsec?: number;
}

export interface ParsedKeepNote {
  sourceFile: string;
  keepId: string;
  title: string;
  textContent: string;
  labels: string[];
  annotations: KeepAnnotation[];
  attachments: KeepAttachment[];
  listContent: KeepListItem[];
  isTrashed: boolean;
  createdTimestampUsec: number;
  userEditedTimestampUsec: number;
}

export type KeepNoteKind = "bookmark" | "note" | "image-note";

export interface BookmarkTarget {
  url: string;
  title: string;
  source: "annotation" | "url-only" | "url-in-text";
}

export interface ClassifiedKeepNote {
  note: ParsedKeepNote;
  kind: KeepNoteKind;
  bookmarkTargets: BookmarkTarget[];
  theme: string;
  tags: string[];
}

export interface ParseKeepResult {
  notes: ParsedKeepNote[];
  skipped: Array<{ sourceFile: string; reason: string }>;
}

const URL_REGEX = /https?:\/\/[^\s<>"')\]]+/gi;

const LABEL_THEME_MAP: Record<string, string> = {
  it: "programming",
  topdeck: "games",
  servicetitan: "work",
};

const DEFAULT_NOTE_THEME = "personal";
const DEFAULT_BOOKMARK_THEME = "unsorted";

export function buildKeepId(sourceFile: string, createdTimestampUsec: number): string {
  return `${sourceFile}#${createdTimestampUsec}`;
}

export function parseKeepJson(raw: string, sourceFile: string): ParsedKeepNote {
  const data = JSON.parse(raw) as KeepNoteJson;
  const created = data.createdTimestampUsec ?? data.userEditedTimestampUsec ?? 0;
  const labels = (data.labels ?? []).map((label) => label.name.trim()).filter(Boolean);

  return {
    sourceFile,
    keepId: buildKeepId(sourceFile, created),
    title: (data.title ?? "").trim(),
    textContent: (data.textContent ?? "").trim(),
    labels,
    annotations: data.annotations ?? [],
    attachments: data.attachments ?? [],
    listContent: data.listContent ?? [],
    isTrashed: Boolean(data.isTrashed),
    createdTimestampUsec: created,
    userEditedTimestampUsec: data.userEditedTimestampUsec ?? created,
  };
}

export function isEmptyKeepNote(note: ParsedKeepNote): boolean {
  if (note.title) return false;
  if (note.textContent) return false;
  if (note.labels.length > 0) return false;
  if (note.annotations.length > 0) return false;
  if (note.attachments.length > 0) return false;
  if (note.listContent.length > 0) return false;
  return true;
}

export function extractUrlsFromText(text: string): string[] {
  const matches = text.match(URL_REGEX) ?? [];
  return [...new Set(matches.map((url) => url.replace(/[.,;:!?)]+$/, "")))];
}

export function isUrlOnlyText(text: string): boolean {
  const trimmed = text.trim();
  if (!trimmed) return false;
  return /^https?:\/\/\S+$/i.test(trimmed);
}

export function labelsToTags(labels: string[]): string[] {
  const tags = new Set<string>(["import", "keep"]);
  for (const label of labels) {
    tags.add(toSlugBase(label));
  }
  return [...tags].filter(Boolean);
}

export function mapLabelsToTheme(labels: string[], kind: "bookmark" | "note"): string {
  for (const label of labels) {
    const mapped = LABEL_THEME_MAP[label.trim().toLowerCase()];
    if (mapped) {
      return mapped;
    }
  }
  return kind === "bookmark" ? DEFAULT_BOOKMARK_THEME : DEFAULT_NOTE_THEME;
}

export function classifyKeepNote(note: ParsedKeepNote): ClassifiedKeepNote {
  const weblinkAnnotations = note.annotations.filter(
    (annotation) => annotation.source === "WEBLINK" && annotation.url,
  );
  const urlsInText = extractUrlsFromText(note.textContent);
  const hasNonUrlText =
    note.textContent.replace(URL_REGEX, "").trim().length > 0 ||
    note.listContent.length > 0;
  const hasImages = note.attachments.length > 0;

  if (hasImages && !note.textContent && note.listContent.length === 0 && weblinkAnnotations.length === 0) {
    return {
      note,
      kind: "image-note",
      bookmarkTargets: [],
      theme: mapLabelsToTheme(note.labels, "note"),
      tags: labelsToTags(note.labels),
    };
  }

  if (weblinkAnnotations.length > 0 && !hasNonUrlText) {
    return {
      note,
      kind: "bookmark",
      bookmarkTargets: weblinkAnnotations.map((annotation) => ({
        url: annotation.url!,
        title: annotation.title?.trim() || note.title || annotation.url!,
        source: "annotation" as const,
      })),
      theme: mapLabelsToTheme(note.labels, "bookmark"),
      tags: labelsToTags(note.labels),
    };
  }

  if (isUrlOnlyText(note.textContent) && weblinkAnnotations.length === 0) {
    return {
      note,
      kind: "bookmark",
      bookmarkTargets: [
        {
          url: note.textContent.trim(),
          title: note.title || note.textContent.trim(),
          source: "url-only",
        },
      ],
      theme: mapLabelsToTheme(note.labels, "bookmark"),
      tags: labelsToTags(note.labels),
    };
  }

  if (urlsInText.length > 0 && hasNonUrlText) {
    return {
      note,
      kind: "note",
      bookmarkTargets: [],
      theme: mapLabelsToTheme(note.labels, "note"),
      tags: labelsToTags(note.labels),
    };
  }

  if (weblinkAnnotations.length > 0 && hasNonUrlText) {
    return {
      note,
      kind: "note",
      bookmarkTargets: [],
      theme: mapLabelsToTheme(note.labels, "note"),
      tags: labelsToTags(note.labels),
    };
  }

  return {
    note,
    kind: hasImages ? "image-note" : "note",
    bookmarkTargets: [],
    theme: mapLabelsToTheme(note.labels, "note"),
    tags: labelsToTags(note.labels),
  };
}

export function wikiDirForKeep(kind: KeepNoteKind, theme: string): string {
  if (kind === "bookmark") {
    return `wiki/bookmarks/${theme}`;
  }
  return `wiki/notes/${theme}`;
}

export function noteTitleForWiki(note: ParsedKeepNote, classified: ClassifiedKeepNote): string {
  if (note.title) return note.title;
  if (classified.bookmarkTargets[0]?.title) return classified.bookmarkTargets[0].title;
  if (note.textContent) {
    const firstLine = note.textContent.split("\n")[0]?.trim() ?? "";
    if (firstLine) return firstLine.slice(0, 80);
  }
  const base = note.sourceFile.replace(/\.json$/i, "");
  return base || "untitled";
}

export function renderNoteBody(note: ParsedKeepNote, assetPaths: string[] = []): string {
  const parts: string[] = [];

  if (note.textContent) {
    parts.push(note.textContent);
  }

  if (note.listContent.length > 0) {
    const listLines = note.listContent.map((item) => {
      const box = item.isChecked ? "[x]" : "[ ]";
      return `- ${box} ${(item.text ?? "").trim()}`;
    });
    parts.push(listLines.join("\n"));
  }

  for (const annotation of note.annotations) {
    if (annotation.url) {
      parts.push(`${annotation.title ?? "Link"}: ${annotation.url}`);
    }
  }

  for (const assetPath of assetPaths) {
    parts.push(`![attachment](${assetPath.replace(/\\/g, "/")})`);
  }

  return parts.join("\n\n").trim();
}

export function summaryFallback(title: string, url: string): string {
  try {
    const host = new URL(url).hostname.replace(/^www\./, "");
    if (title && title !== url) {
      return `${title} — ${host}`;
    }
    return host;
  } catch {
    return title || url;
  }
}

export function normalizeKeepUrl(url: string): string {
  return normalizeUrl(url);
}

export interface BookmarkImportDeps {
  checkHealth: (url: string) => Promise<HealthResult>;
  fetchPage: (url: string) => Promise<FetchResult>;
}

export type BookmarkImportOutcome =
  | { status: "dead"; reason: string }
  | { status: "imported"; summary: string };

export async function resolveBookmarkImport(
  target: BookmarkTarget,
  deps: BookmarkImportDeps,
): Promise<BookmarkImportOutcome> {
  const health = await deps.checkHealth(target.url);
  if (!health.ok) {
    return { status: "dead", reason: health.reason ?? "health_check_failed" };
  }

  const fetchResult = await deps.fetchPage(target.url);
  if (!fetchResult.ok || !fetchResult.html) {
    return { status: "dead", reason: fetchResult.reason ?? "fetch_failed" };
  }

  const summary =
    extractSummaryFromHtml(fetchResult.html) || summaryFallback(target.title, target.url);

  return { status: "imported", summary };
}

export function parseKeepNotesFromFiles(
  files: Array<{ sourceFile: string; raw: string }>,
): ParseKeepResult {
  const notes: ParsedKeepNote[] = [];
  const skipped: Array<{ sourceFile: string; reason: string }> = [];

  for (const file of files) {
    try {
      const note = parseKeepJson(file.raw, file.sourceFile);
      if (note.isTrashed) {
        skipped.push({ sourceFile: file.sourceFile, reason: "trashed" });
        continue;
      }
      if (isEmptyKeepNote(note)) {
        skipped.push({ sourceFile: file.sourceFile, reason: "empty" });
        continue;
      }
      notes.push(note);
    } catch {
      skipped.push({ sourceFile: file.sourceFile, reason: "parse_error" });
    }
  }

  return { notes, skipped };
}
