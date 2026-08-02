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

### 9. Failure handling

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

## Done when

- Primary page(s) written with correct frontmatter
- `wiki/index.md` and `wiki/log.md` updated
- Filenames produced by `npm run kb:slug`
- Git commit with `kb:` prefix (unless no-publish flags)
- User informed of any fetch/write/push failures
