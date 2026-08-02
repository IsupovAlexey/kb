# Wiki schema

Conventions for pages under `wiki/`. The `/kb` skill reads this file before ingest.

## Frontmatter

Every wiki page MUST include YAML frontmatter with at least:

|Field|Required|Notes|
|-|-|-|
|`date`|yes|ISO date (`YYYY-MM-DD`) of creation/update|
|`type`|yes|Open set; suggested: bookmark, note, topic|
|`tags`|yes|Array of lowercase tags|
|`url`|when URL|Source URL for bookmarks|

## Folder policy

- No fixed taxonomy at init. The `/kb` skill creates subfolders on first need (e.g. `wiki/bookmarks/`, `wiki/notes/`, `wiki/topics/`).
- Nested paths (e.g. `wiki/bookmarks/llm/`) are valid without prior planning updates.
- Images and attachments go under `wiki/assets/`.

## Capture vs derived

- **Captures** — long-form and URL content live under `sources/` at the repo root (outside this vault). After initial commit, capture bodies are immutable unless the user explicitly requests a correction.
- **Derived pages** — summaries and short notes live under `wiki/`. Short notes that do not warrant a capture are written only here.
- Derived pages MAY reference their capture path in frontmatter or a footer link.

## Commit messages

Content adds use the `kb:` prefix (e.g. `kb: add bookmark karpathy-llm-wiki`).

## Slug CLI

Generate filenames with the TypeScript slug CLI — do not invent paths in prose:

```bash
npm run kb:slug -- "<title>" --dir wiki/bookmarks
npm run kb:slug -- "<title>" --dir sources
```

Rules: kebab-case (`[a-z0-9-]`), lowercase, drop filler words, max 64 chars. On collision, numeric suffix (`-2`, `-3`, …).

## Ingest modes

- **Light (default):** Primary page + index/log update + wikilinks to existing high-confidence matches only.
- **Deep integrate:** User passes `--integrate` or explicitly asks — agent MAY create/update multiple topic pages.

## Navigation files

- `wiki/index.md` — content catalog; updated on every add.
- `wiki/log.md` — append-only activity log; one line per add.
