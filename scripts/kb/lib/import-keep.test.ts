import { describe, it } from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  buildKeepId,
  classifyKeepNote,
  isEmptyKeepNote,
  isUrlOnlyText,
  mapLabelsToTheme,
  parseKeepJson,
  parseKeepNotesFromFiles,
  resolveBookmarkImport,
  wikiDirForKeep,
} from "./import-keep.js";

const fixturesDir = path.join(path.dirname(fileURLToPath(import.meta.url)), "fixtures", "keep");

function loadFixture(name: string): string {
  return fs.readFileSync(path.join(fixturesDir, name), "utf8");
}

describe("parseKeepJson", () => {
  it("parses title, text, labels, and keepId", () => {
    const note = parseKeepJson(loadFixture("text-note.json"), "text-note.json");
    assert.equal(note.title, "Errands");
    assert.equal(note.textContent, "Remember to buy milk and call the dentist.");
    assert.deepEqual(note.labels, ["Personal reminders"]);
    assert.equal(note.keepId, buildKeepId("text-note.json", note.createdTimestampUsec));
  });
});

describe("isEmptyKeepNote", () => {
  it("detects truly empty notes", () => {
    const note = parseKeepJson(loadFixture("empty-note.json"), "empty-note.json");
    assert.equal(isEmptyKeepNote(note), true);
  });

  it("keeps notes with attachments", () => {
    const note = parseKeepJson(loadFixture("image-note.json"), "image-note.json");
    assert.equal(isEmptyKeepNote(note), false);
  });
});

describe("classifyKeepNote", () => {
  it("routes freeform text to notes/personal", () => {
    const note = parseKeepJson(loadFixture("text-note.json"), "text-note.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "note");
    assert.equal(classified.theme, "personal");
    assert.equal(wikiDirForKeep(classified.kind, classified.theme), "wiki/notes/personal");
  });

  it("routes WEBLINK annotations to bookmarks/programming", () => {
    const note = parseKeepJson(loadFixture("weblink-bookmark.json"), "weblink-bookmark.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "bookmark");
    assert.equal(classified.theme, "programming");
    assert.equal(classified.bookmarkTargets[0].url, "https://example.com/articles/");
    assert.equal(wikiDirForKeep(classified.kind, classified.theme), "wiki/bookmarks/programming");
  });

  it("routes URL-only text to bookmarks", () => {
    const note = parseKeepJson(loadFixture("url-only-bookmark.json"), "url-only-bookmark.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "bookmark");
    assert.equal(classified.bookmarkTargets[0].source, "url-only");
  });

  it("keeps mixed text and URLs as a single note", () => {
    const note = parseKeepJson(loadFixture("mixed-note.json"), "mixed-note.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "note");
    assert.equal(classified.bookmarkTargets.length, 0);
  });

  it("maps TOPDeck label to games theme", () => {
    const note = parseKeepJson(loadFixture("topdeck-note.json"), "topdeck-note.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.theme, "games");
    assert.equal(wikiDirForKeep(classified.kind, classified.theme), "wiki/notes/games");
  });

  it("maps ServiceTitan label to work bookmarks", () => {
    const note = parseKeepJson(loadFixture("servicetitan-bookmark.json"), "servicetitan-bookmark.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "bookmark");
    assert.equal(classified.theme, "work");
  });

  it("classifies image attachments as image-note", () => {
    const note = parseKeepJson(loadFixture("image-note.json"), "image-note.json");
    const classified = classifyKeepNote(note);
    assert.equal(classified.kind, "image-note");
  });
});

describe("mapLabelsToTheme", () => {
  it("falls back to personal for unmapped note labels", () => {
    assert.equal(mapLabelsToTheme(["Personal reminders"], "note"), "personal");
  });

  it("falls back to unsorted for unmapped bookmark labels", () => {
    assert.equal(mapLabelsToTheme(["Random"], "bookmark"), "unsorted");
  });
});

describe("parseKeepNotesFromFiles", () => {
  it("skips trashed and empty notes", () => {
    const files = [
      { sourceFile: "trashed-note.json", raw: loadFixture("trashed-note.json") },
      { sourceFile: "empty-note.json", raw: loadFixture("empty-note.json") },
      { sourceFile: "text-note.json", raw: loadFixture("text-note.json") },
    ];
    const result = parseKeepNotesFromFiles(files);
    assert.equal(result.notes.length, 1);
    assert.equal(result.skipped.some((s) => s.reason === "trashed"), true);
    assert.equal(result.skipped.some((s) => s.reason === "empty"), true);
  });
});

describe("isUrlOnlyText", () => {
  it("detects URL-only content", () => {
    assert.equal(isUrlOnlyText("https://example.com/guide"), true);
    assert.equal(isUrlOnlyText("Read https://example.com/guide first"), false);
  });
});

describe("resolveBookmarkImport", () => {
  it("returns dead when health check fails (Verify Decision 3)", async () => {
    const note = parseKeepJson(loadFixture("dead-url-bookmark.json"), "dead-url-bookmark.json");
    const classified = classifyKeepNote(note);
    const target = classified.bookmarkTargets[0];

    const outcome = await resolveBookmarkImport(target, {
      checkHealth: async () => ({ ok: false, status: 404, reason: "http_error" }),
      fetchPage: async () => {
        throw new Error("fetch should not run when health fails");
      },
    });

    assert.equal(outcome.status, "dead");
    assert.equal(outcome.reason, "http_error");
  });

  it("returns dead when fetch fails after health passes", async () => {
    const note = parseKeepJson(loadFixture("dead-url-bookmark.json"), "dead-url-bookmark.json");
    const target = classifyKeepNote(note).bookmarkTargets[0];

    const outcome = await resolveBookmarkImport(target, {
      checkHealth: async () => ({ ok: true, status: 200 }),
      fetchPage: async () => ({ ok: false, reason: "fetch_failed" }),
    });

    assert.equal(outcome.status, "dead");
    assert.equal(outcome.reason, "fetch_failed");
  });
});
