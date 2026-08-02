## Why

Personal knowledge scattered across bookmarks, notes, and chat history does not compound. This change establishes a git-backed LLM-maintained wiki where each add integrates into a persistent, interlinked markdown vault.

## What Changes

- Initial wiki vault under `wiki/` with navigation files, organic folder policy, and `assets/` for attachments
- Immutable capture layer under `sources/` for URLs, long pastes, and clipped content
- Single `/kb` Cursor skill: classify, summarize, light cross-linking, commit, and push by default
- Light ingest on every add; deep entity integration only when the user nudges
- Wiki query workflow via `/kb` (qmd retrieval + cited synthesis); qmd search over `wiki/` with reindex after each add
- TypeScript tooling under `scripts/kb/` for slug/filename generation and semantic wiki lint
- Prettier markdown formatting via `scripts/formatting/` (personalization-hub pattern)
- Wiki health-check for orphans, broken wikilinks, and index gaps (semantic lint script, not LLM)
- Mobile access documented (Obsidian Android + Obsidian Git)
- No GitHub CI or PR workflows — solo direct-push repo

## Capabilities

### New Capabilities

- `kb-add`: Cursor skill `/kb` for ingest — parse input, write captures and wiki pages, update index/log, git publish
- `wiki-vault`: Vault layout, sources immutability policy, SCHEMA conventions, organic folders, index and log maintenance
- `kb-lint`: Prettier format check plus TypeScript semantic wiki lint (orphans, broken wikilinks, index gaps)
- `kb-search`: qmd indexing, search, and wiki query (retrieval + cited answers via `/kb`)

### Modified Capabilities

- _(none — greenfield repo)_

## Impact

- New content directories (`wiki/`, `sources/`)
- New agent skills under `.agents/skills/`
- TypeScript packages under `scripts/formatting/` and `scripts/kb/`
- Root `.prettierrc`, `.prettierignore`, and AGENTS.md implementation gates for markdown diffs
- Repo `.gitignore` updates for `.obsidian/`, `.qmd/`, and `node_modules/`
- `openspec/config.yaml` project context (wiki conventions summary for future artifacts)
