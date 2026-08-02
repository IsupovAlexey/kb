# Query workflow

Answer questions from the wiki using token-light retrieval. No git commit or push by default.

## Procedure

### 1. Parse query

Extract the question from `/kb query "…"` or a natural-language question routed to query mode.

Flags:

|Flag|Effect|
|-|-|
|`--file`|Write answer as a new wiki page (see step 6)|
|`--no-commit`|File-back without commit (with `--file`)|
|`--no-push`|File-back with commit but no push (with `--file`)|

Natural-language file-back triggers: "file this", "save to wiki", "add this as a note", or similar explicit save intent.

### 2. Retrieve candidates

Prefer qmd when available (from repo root):

```bash
qmd search "<question>" -c wiki -n 5
```

Fallback when `qmd` is not installed or returns no results: scan `wiki/index.md` catalog entries for keyword matches.

If no confident matches exist, state that the wiki has no answer — do not synthesize from unrelated pages.

### 3. Read ranked pages only

Open only the top-ranked hits from step 2. Do not read the entire vault.

### 4. Synthesize cited answer

Return an answer in chat with citations to wiki paths (e.g. `wiki/bookmarks/example.md`). Quote or paraphrase from retrieved pages only.

### 5. Default — no publish

Unless file-back was requested (step 1), do **not** commit or push.

### 6. Optional file-back

When `--file` or natural-language save intent is present:

1. Read `wiki/SCHEMA.md` for frontmatter and folder policy
2. Generate filename via `npm run kb:slug -- "<title>" --dir wiki/notes` (or appropriate subfolder)
3. Write the answer as a new wiki page with `date`, `type`, `tags` frontmatter
4. Append `wiki/index.md` and `wiki/log.md`
5. Publish per ingest git rules unless `--no-commit` / `--no-push`
6. Run `qmd update` when `qmd` is on PATH

## qmd bootstrap (first time)

See `wiki/SCHEMA.md` ## Search (qmd) for bootstrap and collection setup.

Reindex after content changes:

```bash
qmd update
```

## Done when

- Answer returned with wiki path citations, or user informed no confident match exists
- No git commit/push unless file-back was requested
- File-back page written with correct frontmatter when requested
