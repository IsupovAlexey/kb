import { normalizeUrl, type BookmarkEntry } from "./import-firefox.js";

export interface TabEntry {
  url: string;
  title: string;
  pinned: boolean;
  index: number;
  windowId: string;
}

export interface TabSkippedEntry {
  url: string;
  title: string;
  pinned: boolean;
  reason: string;
}

export interface TabParseResult {
  tabs: TabEntry[];
  skipped: TabSkippedEntry[];
}

const DEFAULT_SKIP_PREFIXES = ["place:", "javascript:", "about:", "moz-extension:", "chrome:"];

const WORK_HOST_PATTERNS = [
  /(^|\.)slack\.com$/i,
  /(^|\.)mail\.google\.com$/i,
  /(^|\.)calendar\.google\.com$/i,
  /(^|\.)chatgpt\.com$/i,
  /(^|\.)gemini\.google\.com$/i,
  /(^|\.)docs\.google\.com$/i,
  /(^|\.)sheets\.google\.com$/i,
  /(^|\.)messages\.google\.com$/i,
  /(^|\.)translate\.google\.com$/i,
];

/** Tab Session Manager export: array of { windows: { [windowId]: { [tabId]: tab } } } */
export function parseTabSessionJson(json: string, skipPrefixes = DEFAULT_SKIP_PREFIXES): TabParseResult {
  const parsed = JSON.parse(json) as unknown;
  if (!Array.isArray(parsed)) {
    throw new Error("Tab Session Manager JSON must be a top-level array");
  }

  const tabs: TabEntry[] = [];
  const skipped: TabSkippedEntry[] = [];

  for (const session of parsed) {
    const windows = (session as { windows?: Record<string, Record<string, unknown>> }).windows ?? {};
    for (const [windowId, windowTabs] of Object.entries(windows)) {
      for (const tab of Object.values(windowTabs)) {
        if (!tab || typeof tab !== "object") {
          continue;
        }
        const record = tab as Record<string, unknown>;
        const url = typeof record.url === "string" ? record.url.trim() : "";
        const title = typeof record.title === "string" ? record.title.trim() : "";
        const pinned = Boolean(record.pinned);
        const index = typeof record.index === "number" ? record.index : 0;

        if (!url) {
          skipped.push({ url, title, pinned, reason: "empty_url" });
          continue;
        }

        const lower = url.toLowerCase();
        if (skipPrefixes.some((prefix) => lower.startsWith(prefix))) {
          skipped.push({ url, title, pinned, reason: "skip_url_prefix" });
          continue;
        }

        if (!/^https?:\/\//i.test(url)) {
          skipped.push({ url, title, pinned, reason: "invalid_url" });
          continue;
        }

        tabs.push({ url, title, pinned, index, windowId });
      }
    }
  }

  tabs.sort((a, b) => {
    const windowCmp = a.windowId.localeCompare(b.windowId, undefined, { numeric: true });
    if (windowCmp !== 0) {
      return windowCmp;
    }
    return a.index - b.index;
  });

  return { tabs, skipped };
}

export function isWorkPinnedTab(url: string, pinned: boolean): boolean {
  if (!pinned) {
    return false;
  }

  try {
    const hostname = new URL(url).hostname.toLowerCase();
    return WORK_HOST_PATTERNS.some((pattern) => pattern.test(hostname));
  } catch {
    return false;
  }
}

export function dedupeTabs(tabs: TabEntry[]): {
  unique: TabEntry[];
  duplicateCount: number;
} {
  const seen = new Set<string>();
  const unique: TabEntry[] = [];
  let duplicateCount = 0;

  for (const tab of tabs) {
    const key = normalizeUrl(tab.url);
    if (seen.has(key)) {
      duplicateCount += 1;
      continue;
    }
    seen.add(key);
    unique.push(tab);
  }

  return { unique, duplicateCount };
}

/** Thematic folder segments under wiki/bookmarks/ (not flat tabs/). */
export function classifyTabFolder(url: string, title: string): string[] {
  let hostname = "";
  let pathname = "";
  try {
    const parsed = new URL(url);
    hostname = parsed.hostname.toLowerCase();
    pathname = parsed.pathname.toLowerCase();
  } catch {
    return ["personal"];
  }

  const t = title.toLowerCase();

  if (
    hostname.includes("servicetitan") ||
    hostname.includes("atlassian.net") ||
    hostname.includes("nadpo.ru") ||
    hostname.includes("sdo.nadpo") ||
    hostname.includes("sdo.tiei") ||
    hostname.includes("85.198.116.9")
  ) {
    return ["work", "nadpo"];
  }

  if (hostname.includes("cursor.com")) {
    return ["programming", "ai"];
  }
  if (hostname.includes("tailscale.com")) {
    return ["programming", "devops"];
  }
  if (hostname.includes("github.com")) {
    return ["programming"];
  }
  if (hostname.includes("gitnation.com") || hostname.includes("wearedevelopers.com")) {
    return ["careers", "interviews"];
  }

  if (hostname.includes("howlongtobeat.com") || hostname.includes("neoseeker.com")) {
    return ["games"];
  }
  if (hostname.includes("gamewithpixels.com")) {
    return ["games"];
  }
  if (t.includes("mtg") || (hostname.includes("hobby.am") && pathname.includes("mtg"))) {
    return ["magic", "mtg"];
  }
  if (hostname.includes("hobby.am")) {
    return ["games"];
  }

  if (hostname.includes("bararan.am")) {
    return ["languages", "armenian"];
  }
  if (hostname.includes("ankiweb.net") && /[\u0530-\u058F]/.test(title)) {
    return ["languages", "armenian"];
  }
  if (t.includes("dutch") || hostname.includes("reverso.net")) {
    return ["languages", "dutch"];
  }

  if (hostname.includes("morow.com")) {
    return ["pets"];
  }

  if (
    hostname.includes("mcs-citizenship.am") ||
    hostname.includes("list.am") ||
    hostname.includes("kinodaran.com") ||
    hostname.includes("armfilm.co")
  ) {
    return ["personal", "armenia"];
  }
  if (hostname.includes("joinsherpa") || hostname.includes("sherpa")) {
    return ["personal", "travel"];
  }

  if (hostname.includes("skool.com")) {
    return ["careers", "job-search"];
  }

  if (
    hostname.includes("vkvideo.ru") &&
    (t.includes("snooker") || t.includes("formula") || t.includes("f1"))
  ) {
    return ["interests", "sports"];
  }

  if (
    hostname.includes("imdb.com") ||
    hostname.includes("wcostream.tv") ||
    hostname.includes("navalny-film.io") ||
    hostname.includes("tv.apple.com")
  ) {
    return ["film-and-tv"];
  }

  if (hostname.includes("fantasy-worlds.org")) {
    return ["books"];
  }

  if (hostname.includes("bunkr.cr")) {
    return ["adult"];
  }

  if (hostname.includes("wikipedia.org")) {
    return ["interests"];
  }

  if (hostname.includes("youtube.com")) {
    if (t.includes("armen") || /[\u0530-\u058F]/.test(title)) {
      return ["languages", "armenian"];
    }
    if (t.includes("f1") || t.includes("formula") || t.includes("quake")) {
      return ["interests", "sports"];
    }
    if (t.includes("gilmour") || t.includes("oblivion") || t.includes("music")) {
      return ["music"];
    }
    if (t.includes("gaming pc")) {
      return ["hardware"];
    }
    return ["interests"];
  }

  if (hostname.includes("reddit.com") || hostname.includes("addons.mozilla.org")) {
    return ["social"];
  }

  if (hostname.includes("127.0.0.1") || hostname.includes("localhost")) {
    return ["tools"];
  }

  if (hostname.includes("google.com") && pathname.startsWith("/search")) {
    return ["interests"];
  }

  if (hostname.includes("sooplive.com")) {
    return ["interests"];
  }

  if (hostname.includes("docs.google.com")) {
    if (t.includes("founder") || t.includes("stock plan")) {
      return ["careers", "equity"];
    }
    if (t.includes("беклог") || t.includes("game") || t.includes("игр")) {
      return ["games"];
    }
    if (t.includes("mtg")) {
      return ["magic", "mtg"];
    }
    return ["personal"];
  }

  return ["personal"];
}

export interface TabClassifyPlanEntry {
  url: string;
  title: string;
  folderPath: string[];
  reason: string;
}

export interface TabClassifyPlan {
  entries: TabClassifyPlanEntry[];
}

export function resolveTabFolder(
  url: string,
  title: string,
  plan?: TabClassifyPlan,
): string[] {
  if (plan) {
    const key = normalizeUrl(url);
    const match = plan.entries.find((entry) => normalizeUrl(entry.url) === key);
    if (match && match.folderPath.length > 0) {
      return match.folderPath;
    }
  }
  return classifyTabFolder(url, title);
}

export function tabToBookmarkEntry(tab: TabEntry, plan?: TabClassifyPlan): BookmarkEntry {
  return {
    url: tab.url,
    title: tab.title,
    folderPath: resolveTabFolder(tab.url, tab.title, plan),
  };
}
