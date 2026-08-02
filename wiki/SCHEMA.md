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

## Wiki-only content

All ingested content lives under `wiki/`. There is no separate `sources/` capture layer. URL bookmarks and long pastes are stored as wiki pages with frontmatter and a `## Summary` section.

## Commit messages

Content adds use the `kb:` prefix (e.g. `kb: add bookmark karpathy-llm-wiki`).

## Slug CLI

Generate filenames with the TypeScript slug CLI — do not invent paths in prose:

```bash
npm run kb:slug -- "<title>" --dir wiki/bookmarks
```

Rules: kebab-case (`[a-z0-9-]`), lowercase, drop filler words, max 64 chars. On collision, numeric suffix (`-2`, `-3`, …).

## Ingest modes

- **Light (default):** Primary page + index/log update + wikilinks to existing high-confidence matches only.
- **Deep integrate:** User passes `--integrate` or explicitly asks — agent MAY create/update multiple topic pages.

## Navigation files

- `wiki/index.md` — content catalog; updated on every add.
- `wiki/log.md` — append-only activity log; one line per add.
- `wiki/bookmarks/**/index.md` — optional per-folder indexes listing child pages.

## Search (qmd)

Local search indexes `wiki/` only — not `openspec/` or `.agents/`. Index artifacts live in `.qmd/` (gitignored).

**First-time bootstrap** (from repo root):

```bash
qmd init
qmd collection add wiki --name wiki
```

**Reindex after content changes:**

```bash
qmd update
```

**Search:**

```bash
qmd search "<term>" -c wiki -n 5
```

The `/kb` skill runs reindex after each successful ingest when `qmd` is on PATH. Use `/kb query` for cited synthesis over search results.

## Mobile sync

Read and capture on Android via [Obsidian mobile](https://obsidian.md/mobile). Sync with the [Obsidian Git](https://github.com/Vinzent03/obsidian-git) community plugin — configure vault path to `wiki/` inside this repo.

**Private repo caveat:** mobile git credentials differ from desktop; set up SSH keys or a personal access token in Obsidian Git settings. See upstream [Obsidian Git docs](https://github.com/Vinzent03/obsidian-git#readme) for setup.

No plugin config is stored in this repository.
