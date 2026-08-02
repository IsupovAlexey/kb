# Ingest workflow

Read `wiki/SCHEMA.md` before every ingest.

## Procedure

### 1. Read conventions

Read `wiki/SCHEMA.md` for frontmatter, folder policy, capture rules, and commit prefix.

### 2. Classify input

Determine `type` and `tags` when the user does not supply them. Suggested types: `bookmark`, `note`, `image`, `video`, `idea`, `citation` (open set). Set `url` in frontmatter for bookmarks.

### 3. Choose ingest mode

**Light ingest (default):** Write the primary page(s), append `wiki/index.md` and `wiki/log.md`, and add wikilinks only to **existing** topic or people pages when match confidence is high. Do **not** create new topic stubs on every bookmark.

**Deep integrate:** When the user passes `--integrate` or explicitly asks to integrate into topics, you MAY create or update multiple entity pages across the wiki.

### 4. Generate filenames (slug CLI)

Never invent filenames in prose. Run the slug CLI for every new wiki or capture file:

```bash
npm run kb:slug -- "<title>" --dir wiki/bookmarks
npm run kb:slug -- "<title>" --dir sources
```

The CLI returns kebab-case names (`[a-z0-9-]`) and appends `-2`, `-3`, … on collision.

### 5. Organic folders

Create subfolders under `wiki/` on first need (e.g. `wiki/bookmarks/`, `wiki/notes/`, `wiki/topics/`). No fixed bucket list — nested paths like `wiki/bookmarks/llm/` are valid.

### 6. Write content

|Input|Capture (`sources/`)|Wiki page (`wiki/`)|
|-|-|-|
|URL or long paste|Immutable capture file|Derived summary page|
|Short note|Skip|Wiki page only|

Every wiki page needs frontmatter: `date`, `type`, `tags`; add `url` for bookmarks. Bookmarks and media pages include a `## Summary` section.

Derived pages MAY link back to their capture path in frontmatter or a footer.

### 7. Token-light reads

Before opening full page bodies for cross-links:

1. Prefer qmd search (top 5) when available, **or**
2. Scan `wiki/index.md` catalog entries

Truncate fetched URL text to a bounded length before LLM summarization. Append index and log lines — do not rewrite entire files.

### 8. Git publish

Unless `--no-commit`, `--no-push`, or `--dry-run`:

1. `git add` changed paths under `wiki/` and `sources/`
2. Commit with prefix `kb:` (e.g. `kb: add bookmark karpathy-llm-wiki`)
3. Push to remote (unless `--no-push`)

### 9. qmd reindex

After successful ingest (when `qmd` is on PATH):

```bash
qmd update
```

When `qmd` is not installed, note that search reindex was skipped — ingest still succeeds. See [query.md](query.md) for first-time qmd bootstrap.

### 10. Failure handling

|Failure|Behavior|
|-|-|
|URL fetch fails|Write stub with URL + fetch-failure note; do not claim summary success|
|File write fails|Do not commit; report error|
|Commit succeeds, push fails|Report explicit error with retry-push instructions|

Capture bodies are immutable after initial commit unless the user explicitly requests a correction.

## Flags

|Flag|Effect|
|-|-|
|`--integrate`|Deep integrate — multi-page entity updates allowed|
|`--dry-run`|Preview only; no commit|
|`--no-commit`|Write files; skip git commit|
|`--no-push`|Commit locally; skip push|

Default: commit and push after successful ingest.

## Bulk Firefox import

For large bookmark corpora, use the import CLI (not one-at-a-time `/kb`):

```bash
npm run kb:import-firefox -- "path/to/bookmarks.html"
npm run kb:import-firefox -- "path/to/bookmarks.html" --dry-run
```

Behavior:

1. Copy HTML export to `sources/firefox-bookmarks-<date>.html` (immutable capture)
2. Parse folders and URLs; skip Firefox default folders and `place:` pseudo-URLs
3. Dedupe by URL; fetch live pages; exclude dead links (logged in manifest)
4. Write captures to `sources/` and wiki pages to `wiki/bookmarks/<folder-path>/` with `## Summary` from meta/excerpt
5. Write manifest to `sources/firefox-bookmarks-<date>.manifest.json`
6. Write reclassification proposal to `artifacts/tacos-work/firefox-bookmark-import/reclassify-proposal.json` for user review before bulk folder moves
7. Append one summary line to `wiki/index.md` and `wiki/log.md`
8. Re-run on same date skips already-imported URLs (idempotent)
9. Optional LLM reclassify: edit `artifacts/tacos-work/firefox-bookmark-import/llm-reclassify-plan.json` (or regenerate via `npx tsx scripts/kb/generate-llm-reclassify-plan.ts`), then `npm run kb:reclassify` and `npx tsx scripts/kb/sync-firefox-manifest-paths.ts`. Legacy keyword script: `npm run kb:reclassify:legacy`.

Folder paths use English aliases (not Dutch `bladwijzerwerkbalk` or Russian transliterations). See `mapFolderSegment` in `scripts/kb/lib/import-firefox.ts`.

After import: run `npm run kb:lint`, `npm run format:check` (or `format:write`), and `qmd update` when available.

## Bulk Firefox tab import (Tab Session Manager)

For open-tab snapshots exported from [Tab Session Manager](https://addons.mozilla.org/firefox/addon/tab-session-manager/):

```bash
npm run kb:generate-tab-classify-plan -- "path/to/tab-session.json"   # optional: refresh LLM classify plan
npm run kb:import-firefox-tabs -- "path/to/tab-session.json"
npm run kb:import-firefox-tabs -- "path/to/tab-session.json" --dry-run
```

Behavior:

1. Read thematic folder assignments from `artifacts/tacos-work/firefox-tab-import/tab-classify-plan.json` (generate/refresh via `kb:generate-tab-classify-plan` before first import)
2. Copy JSON export to `sources/firefox-tabs-<date>.json` (immutable capture)
3. Dedupe by URL; skip pinned work-service tabs (Slack, Gmail, Calendar, ChatGPT, Gemini, Google Docs/Sheets, Messages, Translate when pinned)
4. Skip URLs already in `wiki/bookmarks/**` (manifest links to existing `wikiPath`)
5. Classify each tab into existing thematic folders (`programming/`, `games/`, `personal/`, etc.) — not a flat `tabs/` dump
6. Health-check and fetch live pages; exclude dead links; write wiki pages with `type: tab` and `## Summary`
7. Write manifest to `sources/firefox-tabs-<date>.manifest.json`
8. Append one summary line to `wiki/index.md` and `wiki/log.md`
9. Re-run on same date skips already-imported URLs (idempotent)

After import: run `npm run kb:lint`, `npm run format:check` (or `format:write`), and `qmd update` when available.

## Done when

- Primary page(s) written with correct frontmatter
- `wiki/index.md` and `wiki/log.md` updated
- Filenames produced by `npm run kb:slug`
- Git commit with `kb:` prefix (unless no-publish flags)
- `qmd update` run when qmd is available (or user noted reindex skipped)
- User informed of any fetch/write/push failures
