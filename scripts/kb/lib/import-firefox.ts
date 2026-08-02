import { toSlugBase } from "./slug.js";

export interface BookmarkEntry {
  url: string;
  title: string;
  folderPath: string[];
  addDate?: number;
}

export interface ParseOptions {
  skipFolderNames?: string[];
  skipUrlPrefixes?: string[];
}

export interface SkippedEntry {
  url: string;
  title: string;
  folderPath: string[];
  reason: string;
}

export interface ParseResult {
  bookmarks: BookmarkEntry[];
  skipped: SkippedEntry[];
}

export interface HealthResult {
  ok: boolean;
  status?: number;
  reason?: string;
}

export interface FetchResult {
  ok: boolean;
  status?: number;
  finalUrl?: string;
  html?: string;
  reason?: string;
}

const DEFAULT_SKIP_FOLDERS = ["mozilla firefox"];
const DEFAULT_SKIP_PREFIXES = ["place:", "javascript:"];

/** Strip huge ICON attributes so regex parsing stays fast. */
export function stripBookmarkIcons(html: string): string {
  return html.replace(/\s+ICON="[^"]*"/gi, "").replace(/\s+ICON_URI="[^"]*"/gi, "");
}

/**
 * Parse Netscape bookmark HTML into bookmark entries with folder paths.
 */
export function parseBookmarkHtml(html: string, options: ParseOptions = {}): ParseResult {
  const skipFolders = new Set(
    (options.skipFolderNames ?? DEFAULT_SKIP_FOLDERS).map((f) => f.toLowerCase()),
  );
  const skipPrefixes = options.skipUrlPrefixes ?? DEFAULT_SKIP_PREFIXES;

  const cleaned = stripBookmarkIcons(html);
  const bookmarks: BookmarkEntry[] = [];
  const skipped: SkippedEntry[] = [];

  const folderStack: string[] = [];
  const tagRegex =
    /<DT><H3[^>]*>([^<]*)<\/H3>|<\/DL>|<DT><A\s+([^>]*)>([^<]*)<\/A>/gi;

  let match: RegExpExecArray | null;
  while ((match = tagRegex.exec(cleaned)) !== null) {
    if (match[1] !== undefined && match[2] === undefined) {
      const folderName = decodeHtmlEntities(match[1].trim());
      folderStack.push(folderName);
      continue;
    }

    if (match[0] === "</DL>") {
      if (folderStack.length > 0) {
        folderStack.pop();
      }
      continue;
    }

    if (match[2] !== undefined && match[3] !== undefined) {
      const attrs = match[2];
      const title = decodeHtmlEntities(match[3].trim());
      const hrefMatch = attrs.match(/HREF="([^"]*)"/i);
      if (!hrefMatch) {
        continue;
      }

      const url = hrefMatch[1].trim();
      const addDateMatch = attrs.match(/ADD_DATE="(\d+)"/i);
      const addDate = addDateMatch ? Number(addDateMatch[1]) : undefined;
      const folderPath = [...folderStack];

      const inSkippedFolder = folderPath.some((f) => skipFolders.has(f.toLowerCase()));
      const hasSkippedPrefix = skipPrefixes.some((p) => url.toLowerCase().startsWith(p));

      if (inSkippedFolder) {
        skipped.push({ url, title, folderPath, reason: "skip_folder" });
        continue;
      }

      if (hasSkippedPrefix) {
        skipped.push({ url, title, folderPath, reason: "skip_url_prefix" });
        continue;
      }

      if (!url || !/^https?:\/\//i.test(url)) {
        skipped.push({ url, title, folderPath, reason: "invalid_url" });
        continue;
      }

      bookmarks.push({ url, title, folderPath, addDate });
    }
  }

  return { bookmarks, skipped };
}

export function dedupeBookmarks(bookmarks: BookmarkEntry[]): {
  unique: BookmarkEntry[];
  duplicates: Array<{ canonical: BookmarkEntry; alsoIn: string[] }>;
} {
  const byUrl = new Map<string, BookmarkEntry>();
  const folderSets = new Map<string, Set<string>>();

  for (const entry of bookmarks) {
    const key = normalizeUrl(entry.url);
    const folderKey = entry.folderPath.join("/");

    if (!byUrl.has(key)) {
      byUrl.set(key, entry);
      folderSets.set(key, new Set([folderKey]));
    } else {
      folderSets.get(key)!.add(folderKey);
    }
  }

  const unique: BookmarkEntry[] = [];
  const duplicates: Array<{ canonical: BookmarkEntry; alsoIn: string[] }> = [];

  for (const [key, entry] of byUrl) {
    const folders = folderSets.get(key)!;
    const alsoIn = [...folders]
      .map((f) => f || "(root)")
      .filter((f) => f !== (entry.folderPath.join("/") || "(root)"));

    unique.push(entry);
    if (alsoIn.length > 0) {
      duplicates.push({ canonical: entry, alsoIn });
    }
  }

  return { unique, duplicates };
}

const FOLDER_SEGMENT_ALIASES: Record<string, string> = {
  bladwijzerwerkbalk: "toolbar",
  bladwijzermenu: "menu",
  ".net": "dotnet",
  net: "dotnet",
  programmirovanie: "programming",
  "программирование": "programming",
  poleznosti: "utilities",
  "полезности": "utilities",
  igry: "games",
  "игры": "games",
  poisk: "job-search",
  "поиск": "job-search",
  interesnosti: "interests",
  "интересности": "interests",
  knigi: "books",
  "книги": "books",
  muzyka: "music",
  "музыка": "music",
  magiya: "magic",
  "магия": "magic",
  "kino-i-serialy": "film-and-tv",
  "кино и сериалы": "film-and-tv",
  zhelezki: "hardware",
  "железки": "hardware",
  porno: "adult",
  "порно": "adult",
  raznoe: "misc",
  "разное": "misc",
  nederlands: "dutch",
  nadpo: "nadpo",
  "надпо": "nadpo",
  jobs: "jobs",
  mtg: "mtg",
  lk: "personal",
  "лк": "personal",
  dota: "dota",
  "на поиграть?": "to-play",
  mapnaam: "maps",
  "[mapnaam]": "maps",
  h: "armenian",
  "հայերեն": "armenian",
  "центр карьеры": "career-center",
  "документация по сдо и б24": "sdo-b24-docs",
};

export function mapFolderSegment(segment: string): string {
  const trimmed = segment.trim();
  const lower = trimmed.toLowerCase();
  if (FOLDER_SEGMENT_ALIASES[lower]) {
    return FOLDER_SEGMENT_ALIASES[lower];
  }
  if (FOLDER_SEGMENT_ALIASES[trimmed]) {
    return FOLDER_SEGMENT_ALIASES[trimmed];
  }
  return toSlugBase(transliterateToLatin(trimmed));
}

export function folderPathToWikiSegments(folderPath: string[], maxDepth = 3): string[] {
  const segments = folderPath
    .map((part) => mapFolderSegment(part))
    .filter((s) => s.length > 0 && s !== "untitled");

  if (segments.length <= maxDepth) {
    return segments;
  }

  return segments.slice(0, maxDepth);
}

export function wikiDirFromFolder(folderPath: string[], maxDepth = 3): string {
  const segments = folderPathToWikiSegments(folderPath, maxDepth);
  return segments.length > 0 ? `wiki/bookmarks/${segments.join("/")}` : "wiki/bookmarks";
}

export function extractSummaryFromHtml(html: string, maxLength = 500): string {
  const title = extractMetaOrTag(html, "title");
  const description =
    extractMetaContent(html, "description") ||
    extractMetaProperty(html, "og:description") ||
    extractMetaProperty(html, "twitter:description");

  if (description) {
    return truncate(cleanText(description), maxLength);
  }

  const bodyText = extractBodyText(html);
  if (bodyText) {
    return truncate(bodyText, maxLength);
  }

  if (title) {
    return truncate(cleanText(title), maxLength);
  }

  return "";
}

export async function checkUrlHealth(
  url: string,
  fetchImpl: typeof fetch = fetch,
  timeoutMs = 10000,
): Promise<HealthResult> {
  try {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);

    const response = await fetchImpl(url, {
      method: "GET",
      redirect: "follow",
      signal: controller.signal,
      headers: {
        "User-Agent": "kb-firefox-import/1.0",
        Accept: "text/html,application/xhtml+xml",
      },
    });

    clearTimeout(timer);

    if (response.status >= 400) {
      return { ok: false, status: response.status, reason: "http_error" };
    }

    return { ok: true, status: response.status };
  } catch {
    return { ok: false, reason: "network_error" };
  }
}

export async function fetchPage(
  url: string,
  fetchImpl: typeof fetch = fetch,
  timeoutMs = 15000,
): Promise<FetchResult> {
  try {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);

    const response = await fetchImpl(url, {
      method: "GET",
      redirect: "follow",
      signal: controller.signal,
      headers: {
        "User-Agent": "kb-firefox-import/1.0",
        Accept: "text/html,application/xhtml+xml",
      },
    });

    clearTimeout(timer);

    if (response.status >= 400) {
      return { ok: false, status: response.status, reason: "http_error" };
    }

    const html = await response.text();
    return { ok: true, status: response.status, finalUrl: response.url, html };
  } catch {
    return { ok: false, reason: "network_error" };
  }
}

export interface ReclassifySuggestion {
  fromFolder: string;
  reason: string;
  bookmarkCount: number;
  topDomains: string[];
  suggestedTarget?: string;
}

const MESSY_FOLDER_PATTERNS = [
  /^разное$/i,
  /^other$/i,
  /^unsorted$/i,
  /^misc$/i,
  /^mapnaam$/i,
];

export function buildReclassifyProposal(bookmarks: BookmarkEntry[]): ReclassifySuggestion[] {
  const byFolder = new Map<string, BookmarkEntry[]>();

  for (const entry of bookmarks) {
    const key = entry.folderPath.join("/") || "(root)";
    const list = byFolder.get(key) ?? [];
    list.push(entry);
    byFolder.set(key, list);
  }

  const suggestions: ReclassifySuggestion[] = [];

  for (const [folder, entries] of byFolder) {
    const leaf = folder.split("/").pop() ?? folder;
    const domains = domainCounts(entries);
    const topDomains = [...domains.entries()]
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([d]) => d);

    const isMessyName = MESSY_FOLDER_PATTERNS.some((re) => re.test(leaf));
    const highDiversity = topDomains.length >= 4 && entries.length >= 10;

    if (!isMessyName && !highDiversity) {
      continue;
    }

    const reason = isMessyName
      ? "generic_folder_name"
      : "high_domain_diversity";

    suggestions.push({
      fromFolder: folder,
      reason,
      bookmarkCount: entries.length,
      topDomains,
      suggestedTarget: suggestTarget(topDomains),
    });
  }

  return suggestions.sort((a, b) => b.bookmarkCount - a.bookmarkCount);
}

function suggestTarget(topDomains: string[]): string | undefined {
  if (topDomains.length === 0) {
    return undefined;
  }

  const domain = topDomains[0];
  if (domain.includes("github.com")) return "dev/github";
  if (domain.includes("stackoverflow.com")) return "dev/stackoverflow";
  if (domain.includes("youtube.com")) return "media/youtube";
  if (domain.includes("notion.")) return "notes/notion";
  if (domain.includes("medium.com")) return "articles/medium";
  if (domain.includes("habr.com")) return "articles/habr";

  const base = domain.replace(/^www\./, "").split(".")[0];
  return `by-domain/${toSlugBase(base)}`;
}

function domainCounts(entries: BookmarkEntry[]): Map<string, number> {
  const counts = new Map<string, number>();
  for (const entry of entries) {
    try {
      const host = new URL(entry.url).hostname.toLowerCase();
      counts.set(host, (counts.get(host) ?? 0) + 1);
    } catch {
      // ignore invalid
    }
  }
  return counts;
}

function transliterateToLatin(text: string): string {
  const map: Record<string, string> = {
    а: "a", б: "b", в: "v", г: "g", д: "d", е: "e", ё: "e", ж: "zh", з: "z",
    и: "i", й: "y", к: "k", л: "l", м: "m", н: "n", о: "o", п: "p", р: "r",
    с: "s", т: "t", у: "u", ф: "f", х: "h", ц: "c", ч: "ch", ш: "sh", щ: "sch",
    ъ: "", ы: "y", ь: "", э: "e", ю: "yu", я: "ya",
    Ա: "a", Բ: "b", Գ: "g", Դ: "d", Ե: "e", Զ: "z", Է: "e", Ը: "e", Թ: "t",
    Ժ: "zh", Ի: "i", Լ: "l", Խ: "kh", Ծ: "c", Կ: "k", Հ: "h", Ձ: "dz", Ղ: "gh",
    Ճ: "ch", Մ: "m", Յ: "y", Ն: "n", Շ: "sh", Ո: "o", Չ: "ch", Պ: "p", Ջ: "j",
    Ռ: "r", Ս: "s", Վ: "v", Տ: "t", Ր: "r", Ց: "c", Ւ: "w", Փ: "p", Ք: "k",
    Օ: "o", Ֆ: "f",
  };

  return text
    .split("")
    .map((ch) => {
      const lower = ch.toLowerCase();
      if (map[lower] !== undefined) {
        return map[lower];
      }
      if (map[ch] !== undefined) {
        return map[ch];
      }
      return ch;
    })
    .join("");
}

function normalizeUrl(url: string): string {
  try {
    const u = new URL(url);
    u.hash = "";
    return u.toString();
  } catch {
    return url.trim().toLowerCase();
  }
}

function decodeHtmlEntities(text: string): string {
  return text
    .replace(/&amp;/g, "&")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&quot;/g, '"')
    .replace(/&#39;/g, "'")
    .replace(/&#x([0-9a-f]+);/gi, (_, hex) => String.fromCharCode(parseInt(hex, 16)))
    .replace(/&#(\d+);/g, (_, num) => String.fromCharCode(parseInt(num, 10)));
}

function extractMetaContent(html: string, name: string): string | undefined {
  const re = new RegExp(
    `<meta[^>]+name=["']${name}["'][^>]+content=["']([^"']*)["']`,
    "i",
  );
  const alt = new RegExp(
    `<meta[^>]+content=["']([^"']*)["'][^>]+name=["']${name}["']`,
    "i",
  );
  const match = html.match(re) ?? html.match(alt);
  return match?.[1];
}

function extractMetaProperty(html: string, property: string): string | undefined {
  const re = new RegExp(
    `<meta[^>]+property=["']${property}["'][^>]+content=["']([^"']*)["']`,
    "i",
  );
  const alt = new RegExp(
    `<meta[^>]+content=["']([^"']*)["'][^>]+property=["']${property}["']`,
    "i",
  );
  const match = html.match(re) ?? html.match(alt);
  return match?.[1];
}

function extractMetaOrTag(html: string, tag: string): string | undefined {
  const re = new RegExp(`<${tag}[^>]*>([^<]*)</${tag}>`, "i");
  const match = html.match(re);
  return match?.[1];
}

function extractBodyText(html: string): string {
  const withoutScripts = html
    .replace(/<script[\s>][\s\S]*?<\/script>/gi, " ")
    .replace(/<style[\s>][\s\S]*?<\/style>/gi, " ");
  const bodyMatch = withoutScripts.match(/<body[^>]*>([\s\S]*)<\/body>/i);
  const chunk = bodyMatch?.[1] ?? withoutScripts;
  return cleanText(chunk.replace(/<[^>]+>/g, " "));
}

function cleanText(text: string): string {
  return text.replace(/\s+/g, " ").trim();
}

function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength) {
    return text;
  }
  return `${text.slice(0, maxLength - 1).trimEnd()}…`;
}
