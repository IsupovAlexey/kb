import { describe, it } from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  buildTelegramId,
  htmlTextToMarkdown,
  parseTelegramDate,
  parseTelegramHtml,
  renderTelegramNotePage,
  slugTitleForMessage,
  titleFromText,
} from "./import-telegram.js";

const fixturesDir = path.join(path.dirname(fileURLToPath(import.meta.url)), "fixtures", "telegram");

describe("parseTelegramHtml", () => {
  it("extracts message id, date, text, and forwarded_from from sample HTML", () => {
    const html = fs.readFileSync(path.join(fixturesDir, "sample-export.html"), "utf8");
    const result = parseTelegramHtml(html);

    assert.equal(result.channelName, "Test Channel");
    assert.equal(result.messages.length, 2);
    assert.equal(result.skipped.length, 1);
    assert.equal(result.skipped[0]?.reason, "media_only");

    const forwarded = result.messages[0];
    assert.equal(forwarded?.messageId, "2");
    assert.equal(forwarded?.messageDate, "2024-05-20");
    assert.equal(forwarded?.forwardedFrom, "Alexey Isupov");
    assert.equal(forwarded?.forwardedDate, "2023-01-19");
    assert.match(forwarded?.textMarkdown ?? "", /Forwarded review text about a Dutch film/);

    const direct = result.messages[1];
    assert.equal(direct?.messageId, "3");
    assert.match(direct?.textMarkdown ?? "", /https:\/\/www\.npostart\.nl/);
    assert.equal(direct?.forwardedFrom, undefined);
  });
});

describe("htmlTextToMarkdown", () => {
  it("converts links and line breaks", () => {
    const html =
      'Line one<br><br>See <a href="https://example.com">https://example.com</a> and <a href="https://x.com">label</a>';
    const markdown = htmlTextToMarkdown(html);
    assert.match(markdown, /Line one/);
    assert.match(markdown, /https:\/\/example\.com/);
    assert.match(markdown, /\[label\]\(https:\/\/x\.com\)/);
  });
});

describe("title and slug helpers", () => {
  it("parses telegram export dates", () => {
    assert.equal(parseTelegramDate("20.05.2024 21:27:55 UTC+04:00"), "2024-05-20");
  });

  it("uses telegram message id slug fallback for cyrillic titles", () => {
    assert.equal(slugTitleForMessage("Посмотрел сериал", "42"), "telegram-42");
    assert.equal(slugTitleForMessage("English review title", "7"), "english-review-title");
  });

  it("builds stable telegram ids", () => {
    assert.equal(buildTelegramId("55"), "telegram:55");
  });

  it("truncates long titles", () => {
    const long = "A".repeat(100);
    assert.equal(titleFromText(long).length, 80);
  });
});

describe("renderTelegramNotePage", () => {
  it("renders note frontmatter without summary section", () => {
    const page = renderTelegramNotePage({
      messageId: "2",
      channelName: "Test",
      messageDate: "2024-05-20",
      title: "Review",
      textMarkdown: "Body text",
      forwardedFrom: "Alexey Isupov",
      forwardedDate: "2023-01-19",
      hasMissingMedia: false,
    });

    assert.match(page, /^---\n/);
    assert.match(page, /type: note/);
    assert.match(page, /tags:.*import/);
    assert.match(page, /forwarded_from: "Alexey Isupov"/);
    assert.doesNotMatch(page, /## Summary/);
    assert.match(page, /> Forwarded from Alexey Isupov/);
    assert.match(page, /Body text/);
  });
});
