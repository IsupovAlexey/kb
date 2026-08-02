import { describe, it } from "node:test";
import assert from "node:assert/strict";
import {
  buildReclassifyProposal,
  checkUrlHealth,
  dedupeBookmarks,
  extractSummaryFromHtml,
  folderPathToWikiSegments,
  parseBookmarkHtml,
  wikiDirFromFolder,
} from "./import-firefox.js";

const SAMPLE_HTML = `<!DOCTYPE NETSCAPE-Bookmark-file-1>
<DL><p>
    <DT><H3>Dev</H3>
    <DL><p>
        <DT><A HREF="https://example.com/article" ADD_DATE="123">Example Article</A>
        <DT><A HREF="place:type=6&sort=14" ADD_DATE="123">Recent Tags</A>
    </DL><p>
    <DT><H3>Mozilla Firefox</H3>
    <DL><p>
        <DT><A HREF="https://support.mozilla.org/products/firefox" ADD_DATE="123">Get Help</A>
    </DL><p>
    <DT><H3>Разное</H3>
    <DL><p>
        <DT><A HREF="https://github.com/foo/bar" ADD_DATE="123">GitHub Repo</A>
        <DT><A HREF="https://stackoverflow.com/q/1" ADD_DATE="123">Stack Question</A>
    </DL><p>
</DL><p>`;

describe("parseBookmarkHtml", () => {
  it("extracts url, title, and folder_path", () => {
    const { bookmarks, skipped } = parseBookmarkHtml(SAMPLE_HTML);

    assert.equal(bookmarks.length, 3);
    assert.deepEqual(bookmarks[0], {
      url: "https://example.com/article",
      title: "Example Article",
      folderPath: ["Dev"],
      addDate: 123,
    });
    assert.equal(skipped.some((s) => s.reason === "skip_url_prefix"), true);
    assert.equal(skipped.some((s) => s.reason === "skip_folder"), true);
  });
});

describe("dedupeBookmarks", () => {
  it("merges duplicate urls", () => {
    const entries = [
      { url: "https://example.com/a", title: "A", folderPath: ["Dev"] },
      { url: "https://example.com/a", title: "A copy", folderPath: ["Other"] },
    ];
    const { unique, duplicates } = dedupeBookmarks(entries);
    assert.equal(unique.length, 1);
    assert.equal(duplicates.length, 1);
    assert.equal(duplicates[0].alsoIn.length, 1);
  });
});

describe("folder mapping", () => {
  it("slugifies and caps folder depth", () => {
    const segments = folderPathToWikiSegments(
      ["Программирование", "Rust", "Async", "Deep"],
      3,
    );
    assert.equal(segments.length, 3);
    assert.equal(wikiDirFromFolder(["Программирование", "Rust"], 3), "wiki/bookmarks/programming/rust");
  });
});

describe("extractSummaryFromHtml", () => {
  it("prefers meta description", () => {
    const html = `<html><head>
      <meta name="description" content="Short page description." />
      <title>Ignored</title>
    </head><body><p>Body text here.</p></body></html>`;
    assert.equal(extractSummaryFromHtml(html), "Short page description.");
  });
});

describe("buildReclassifyProposal", () => {
  it("flags messy folder names", () => {
    const bookmarks = [
      { url: "https://github.com/a", title: "A", folderPath: ["Разное"] },
      { url: "https://stackoverflow.com/q", title: "Q", folderPath: ["Разное"] },
    ];
    const suggestions = buildReclassifyProposal(bookmarks);
    assert.equal(suggestions.length, 1);
    assert.equal(suggestions[0].reason, "generic_folder_name");
  });
});

describe("checkUrlHealth", () => {
  it("marks dead urls as not ok", async () => {
    const mockFetch: typeof fetch = async () =>
      ({
        status: 404,
        url: "https://example.com/missing",
      }) as Response;

    const result = await checkUrlHealth("https://example.com/missing", mockFetch);
    assert.equal(result.ok, false);
    assert.equal(result.reason, "http_error");
  });

  it("marks live urls as ok", async () => {
    const mockFetch: typeof fetch = async () =>
      ({
        status: 200,
        url: "https://example.com/ok",
      }) as Response;

    const result = await checkUrlHealth("https://example.com/ok", mockFetch);
    assert.equal(result.ok, true);
  });
});
