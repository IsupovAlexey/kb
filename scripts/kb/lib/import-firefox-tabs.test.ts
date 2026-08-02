import { describe, it } from "node:test";
import assert from "node:assert/strict";
import {
  classifyTabFolder,
  dedupeTabs,
  isWorkPinnedTab,
  parseTabSessionJson,
} from "./import-firefox-tabs.js";

const SAMPLE_TSM_JSON = JSON.stringify([
  {
    windows: {
      "1": {
        "1": {
          id: 1,
          index: 0,
          pinned: true,
          url: "https://app.slack.com/client/E02UEHRD08M",
          title: "Slack",
        },
        "2": {
          id: 2,
          index: 1,
          pinned: false,
          url: "https://github.com/foo/bar",
          title: "GitHub Repo",
        },
        "3": {
          id: 3,
          index: 2,
          pinned: false,
          url: "moz-extension://abc/options",
          title: "Extension",
        },
      },
    },
  },
]);

describe("parseTabSessionJson", () => {
  it("extracts url, title, pinned, and index from TSM JSON", () => {
    const { tabs, skipped } = parseTabSessionJson(SAMPLE_TSM_JSON);

    assert.equal(tabs.length, 2);
    assert.deepEqual(tabs[0], {
      url: "https://app.slack.com/client/E02UEHRD08M",
      title: "Slack",
      pinned: true,
      index: 0,
      windowId: "1",
    });
    assert.equal(skipped.length, 1);
    assert.equal(skipped[0].reason, "skip_url_prefix");
  });

  it("throws on invalid JSON shape", () => {
    assert.throws(() => parseTabSessionJson('{"not":"array"}'), /top-level array/);
  });
});

describe("isWorkPinnedTab", () => {
  it("skips pinned Slack and Gmail URLs", () => {
    assert.equal(
      isWorkPinnedTab("https://app.slack.com/client/E02UEHRD08M", true),
      true,
    );
    assert.equal(isWorkPinnedTab("https://mail.google.com/mail/u/0/#inbox", true), true);
  });

  it("imports unpinned tabs on work domains", () => {
    assert.equal(
      isWorkPinnedTab("https://docs.google.com/spreadsheets/d/abc/edit", false),
      false,
    );
  });
});

describe("dedupeTabs", () => {
  it("keeps first tab per URL", () => {
    const tabs = [
      {
        url: "https://example.com/a",
        title: "A",
        pinned: false,
        index: 0,
        windowId: "1",
      },
      {
        url: "https://example.com/a",
        title: "A copy",
        pinned: false,
        index: 1,
        windowId: "1",
      },
    ];
    const { unique, duplicateCount } = dedupeTabs(tabs);
    assert.equal(unique.length, 1);
    assert.equal(duplicateCount, 1);
    assert.equal(unique[0].title, "A");
  });
});

describe("classifyTabFolder", () => {
  it("assigns thematic folders instead of flat tabs/", () => {
    assert.deepEqual(classifyTabFolder("https://github.com/foo", "Repo"), ["programming"]);
    assert.deepEqual(classifyTabFolder("https://www.youtube.com/watch?v=1", "F1 review"), [
      "interests",
      "sports",
    ]);
    assert.notDeepEqual(classifyTabFolder("https://example.com", "Example"), ["tabs"]);
  });
});
