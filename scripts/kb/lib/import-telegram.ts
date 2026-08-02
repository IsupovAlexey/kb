import { toSlugBase } from "./slug.js";

export interface ParsedTelegramMessage {
  messageId: string;
  channelName: string;
  messageDate: string;
  title: string;
  textMarkdown: string;
  forwardedFrom?: string;
  forwardedDate?: string;
  hasMissingMedia: boolean;
}

export interface ParseTelegramResult {
  channelName: string;
  messages: ParsedTelegramMessage[];
  skipped: Array<{ messageId: string; reason: string }>;
}

const MESSAGE_START_RE = /<div class="message default[^"]*" id="message(\d+)">/g;

const CHANNEL_NAME_RE =
  /<div class="page_header">[\s\S]*?<div class="text bold">\s*([\s\S]*?)\s*<\/div>/;

const MESSAGE_DATE_RE =
  /<div class="pull_right date details" title="([^"]+)"/;

const FORWARDED_FROM_RE =
  /<div class="from_name">\s*([^<\n]+)(?:<span class="date details" title="([^"]+)")?/;

function contentHtmlForMessage(blockHtml: string): string {
  const forwardedIdx = blockHtml.indexOf('<div class="forwarded body">');
  if (forwardedIdx >= 0) {
    return blockHtml.slice(forwardedIdx);
  }
  return blockHtml;
}

const TEXT_BLOCK_RE = /<div class="text">\s*([\s\S]*?)\s*<\/div>/g;

const MISSING_MEDIA_RE = /Not included, change data exporting settings to download/;

export function decodeHtmlEntities(text: string): string {
  return text
    .replace(/&quot;/g, '"')
    .replace(/&#33;/g, "!")
    .replace(/&amp;/g, "&")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&laquo;/g, "«")
    .replace(/&raquo;/g, "»");
}

export function parseTelegramDate(title: string): string | undefined {
  const match = title.match(/^(\d{2})\.(\d{2})\.(\d{4})/);
  if (!match) {
    return undefined;
  }
  const [, day, month, year] = match;
  return `${year}-${month}-${day}`;
}

export function htmlTextToMarkdown(html: string): string {
  let text = html.trim();
  text = text.replace(/<br\s*\/?>/gi, "\n\n");
  text = text.replace(/<a href="([^"]+)">([\s\S]*?)<\/a>/gi, (_match, href, label) => {
    const cleanLabel = decodeHtmlEntities(label.replace(/<[^>]+>/g, "").trim());
    const cleanHref = decodeHtmlEntities(href.trim());
    if (cleanLabel === cleanHref || !cleanLabel) {
      return cleanHref;
    }
    return `[${cleanLabel}](${cleanHref})`;
  });
  text = text.replace(/<s>([\s\S]*?)<\/s>/gi, "~~$1~~");
  text = text.replace(/<[^>]+>/g, "");
  text = decodeHtmlEntities(text);
  return text.replace(/\n{3,}/g, "\n\n").trim();
}

export function titleFromText(text: string, maxLength = 80): string {
  const firstLine = text.split(/\n+/).find((line) => line.trim().length > 0) ?? "";
  const trimmed = firstLine.trim();
  if (trimmed.length <= maxLength) {
    return trimmed || "untitled";
  }
  return `${trimmed.slice(0, maxLength - 1).trim()}…`;
}

export function slugTitleForMessage(title: string, messageId: string): string {
  const base = toSlugBase(title);
  if (base === "untitled") {
    return `telegram-${messageId}`;
  }
  return base;
}

function extractTextBlocks(bodyHtml: string): string[] {
  const blocks: string[] = [];
  for (const match of bodyHtml.matchAll(TEXT_BLOCK_RE)) {
    blocks.push(match[1] ?? "");
  }
  return blocks;
}

function parseMessageBlock(
  messageId: string,
  blockHtml: string,
  channelName: string,
): ParsedTelegramMessage | { skip: string } {
  const dateMatch = blockHtml.match(MESSAGE_DATE_RE);
  const messageDate = dateMatch ? parseTelegramDate(dateMatch[1]) : undefined;
  if (!messageDate) {
    return { skip: "missing_date" };
  }

  const hasForward = blockHtml.includes('<div class="forwarded body">');
  const contentHtml = hasForward ? contentHtmlForMessage(blockHtml) : blockHtml;
  const forwardedMatch = hasForward ? contentHtml.match(FORWARDED_FROM_RE) : null;
  const forwardedFrom = forwardedMatch?.[1]?.trim();
  const forwardedDate = forwardedMatch?.[2]
    ? parseTelegramDate(forwardedMatch[2])
    : undefined;

  const textBlocks = extractTextBlocks(hasForward ? contentHtmlForMessage(blockHtml) : blockHtml);
  const textHtml = textBlocks.join("\n\n");
  const textMarkdown = htmlTextToMarkdown(textHtml);
  const hasMissingMedia = MISSING_MEDIA_RE.test(blockHtml);

  if (!textMarkdown) {
    if (hasMissingMedia) {
      return { skip: "media_only" };
    }
    return { skip: "empty_text" };
  }

  return {
    messageId,
    channelName,
    messageDate,
    title: titleFromText(textMarkdown),
    textMarkdown,
    forwardedFrom: forwardedFrom || undefined,
    forwardedDate,
    hasMissingMedia,
  };
}

export function parseTelegramHtml(html: string): ParseTelegramResult {
  const channelMatch = html.match(CHANNEL_NAME_RE);
  const channelName = channelMatch ? decodeHtmlEntities(channelMatch[1].trim()) : "unknown";

  const messages: ParsedTelegramMessage[] = [];
  const skipped: Array<{ messageId: string; reason: string }> = [];

  const markers = [...html.matchAll(MESSAGE_START_RE)];
  for (let i = 0; i < markers.length; i += 1) {
    const match = markers[i];
    const messageId = match[1];
    const start = (match.index ?? 0) + match[0].length;
    const end = i + 1 < markers.length ? (markers[i + 1].index ?? html.length) : html.length;
    const blockHtml = html.slice(start, end);
    const parsed = parseMessageBlock(messageId, blockHtml, channelName);

    if ("skip" in parsed) {
      skipped.push({ messageId, reason: parsed.skip });
      continue;
    }

    messages.push(parsed);
  }

  return { channelName, messages, skipped };
}

export function buildTelegramId(messageId: string): string {
  return `telegram:${messageId}`;
}

export function renderTelegramNotePage(message: ParsedTelegramMessage): string {
  const lines: string[] = [
    "---",
    `date: ${message.messageDate}`,
    "type: note",
    "tags: [\"import\", \"telegram\", \"dutch-film\"]",
    `telegram_message_id: ${message.messageId}`,
  ];

  if (message.forwardedFrom) {
    lines.push(`forwarded_from: ${JSON.stringify(message.forwardedFrom)}`);
  }
  if (message.forwardedDate) {
    lines.push(`forwarded_date: ${message.forwardedDate}`);
  }
  if (message.hasMissingMedia) {
    lines.push("telegram_missing_media: true");
  }

  lines.push("---", "");

  if (message.forwardedFrom) {
    lines.push(
      `> Forwarded from ${message.forwardedFrom}${message.forwardedDate ? ` (${message.forwardedDate})` : ""}`,
      "",
    );
  }

  lines.push(message.textMarkdown);
  return `${lines.join("\n")}\n`;
}

export const WIKI_DIR_DUTCH_FILM_REVIEWS = "wiki/notes/dutch-film-reviews";
