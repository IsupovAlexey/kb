---
grill_mode: assumptions
grill:
  planning: complete
  proposal: complete
  specs: complete
  design: complete
  tasks: complete
  update: complete
---

# Grill summaries

## proposal

### Summary

Personal LLM wiki (Karpathy pattern) for `q:\source\kb`: git-backed markdown, Obsidian on `wiki/`, LLM-routed ingest via single `/kb` skill, light synthesis by default, private GitHub.

### Decisions

- `sources/` at repo root for immutable captures; `wiki/` is the Obsidian vault for derived pages
- Short notes skip `sources/` and live only under `wiki/`
- Single `/kb` skill; no C# CLI or batch synthesize pipeline from reference repo
- Light ingest default; deep integrate only on explicit user nudge
- Organic folder buckets — no pre-planned taxonomy; agent creates folders on first add
- qmd indexes `wiki/` only; reindex after each successful `/kb` add
- Init includes lint/health-check capability (user added to scope)
- Query skill deferred; mobile = document Obsidian Android + Obsidian Git only
- Repo root keeps tacos/openspec/agentic tooling separate from vault

### Open questions

- Forgotten item `d)` from original explore list — still unknown
- Binary asset limits (PDF, voice, large images) — defer specifics to apply

### User inputs

- Confirmed layout default: `sources/` at repo root (immutable captures); `wiki/` = Obsidian vault; short notes skip `sources/`
- Added lint/health-check to init scope (not deferred)
- Confirmed organic folders: no pre-planned buckets; agent creates folders on first add; frontmatter `type` + `tags` always
- Confirmed mobile doc-only: document Obsidian Android + Obsidian Git; no plugin config in repo
- Confirmed qmd wiki-only: index `wiki/` only; reindex after each `/kb` add

## specs

### Summary

Delta specs for four capabilities: `kb-add`, `wiki-vault`, `kb-lint`, `kb-search`.

### Decisions

- Capture immutability enforced by convention + skill policy (no separate tooling layer)
- Frontmatter minimum: `date`, `type`, `tags`; `url` for bookmarks; open `type` set with suggested values in schema
- Lint covers orphans, broken wikilinks, index staleness — not full contradiction analysis on day one

### Open questions

- Whether `sources/` gets a second qmd collection later for agent-only search — deferred (wiki-only for now)

### User inputs

- Lint in init scope per proposal grill decision
- Light ingest: wikilinks to existing topics only; new topic pages on high confidence

## design

### Summary

Flat two-layer content model under repo root; agentic schema split between repo `AGENTS.md` (tacos) and `wiki/SCHEMA.md` (wiki conventions).

### Decisions

- Vault path: open `wiki/` in Obsidian; `sources/` outside vault (invisible in graph — accepted tradeoff)
- `wiki/assets/` for images and attachments
- `wiki/index.md` + `wiki/log.md` as navigation aids per Karpathy pattern
- Commit message prefix: `kb:` for content adds
- Deep integrate nudge: natural language or `--integrate` flag on `/kb`

### Open questions

- Sample bookmark page in init skeleton — optional, likely skip

### User inputs

- Organic subfolders for bookmarks allowed as content grows (e.g. `wiki/bookmarks/llm/`)
- No empty seed folders except `wiki/assets/` if needed for Obsidian attachment path

## tasks

### Summary

Three implementation stages: vault skeleton, `/kb` skill, lint + qmd search.

### Decisions

- Stage 1: directories, SCHEMA, gitignore, sources README
- Stage 2: `.agents/skills/kb/SKILL.md` with full ingest contract
- Stage 3: lint skill or kb-lint reference + qmd bootstrap docs/hook

### Open questions

- None blocking

### User inputs

- Default commit+push on `/kb` unless `--no-commit` / `--no-push`
- Private GitHub repo — document mobile git credential caveat in wiki docs section

## update

### Summary

Refinement: add TypeScript scripts for slug/filename generation and semantic wiki lint; adopt personalization-hub Prettier pattern for markdown formatting (separate from semantic lint).

### Decisions

- TypeScript for repo scripts (`scripts/kb/`); no Python
- Prettier via isolated `scripts/formatting/` mini-package (personalization-hub pattern) — format check/write, not semantic lint
- Semantic wiki lint (orphans, broken wikilinks, index gaps) via TypeScript CLI in `scripts/kb/`
- `/kb` skill calls slug script for filenames; calls wiki-lint script on lint requests (not LLM checklist)
- Local dev gates in AGENTS.md; CI workflows deferred

### Open questions

- None blocking

### User inputs

- Add scripts for lint and slug/filename
- Use TypeScript for scripting generally
- Use Prettier for linting (format), not Python — reference personalization-hub pattern
- No GitHub CI — solo direct-push repo, no PR workflow
- Add wiki query to plans — `/kb query` sub-workflow using qmd retrieval + cited synthesis (same skill, not separate)

## apply

### Stage 1

#### Summary

Short apply grill (mode: short). Minimal SCHEMA skeleton; empty vault; Prettier scoped to `wiki/**/*.md` at init.

#### Decisions

- SCHEMA.md at init: frontmatter fields, folder policy, capture rules, commit prefix only — defer slug CLI and ingest modes to Stage 2
- No sample bookmark page in skeleton
- `index.md` / `log.md`: headers and brief usage notes only (no example rows)
- Prettier check scoped to `wiki/**/*.md` only at init (not openspec or AGENTS.md)

#### Open questions

- None blocking Stage 1

#### User inputs

- Grill mode: short
- SCHEMA depth: minimal skeleton (Recommended)
- Sample page: skip (Recommended)
- index/log starter: headers + empty sections with usage notes (Recommended)
- Prettier scope: wiki/**/*.md only at init (Recommended)

### Stage 2

#### Summary

Short apply grill (mode: short). TypeScript CLIs via tsx at repo root; numeric slug collision suffix; Stage 2 skill covers ingest + slug only.

#### Decisions

- Slug collision: numeric suffix (`-2`, `-3`, …) per tacos-work pattern
- CLI runtime: `tsx` via root `package.json` scripts (no separate `scripts/kb/package.json`)
- Stage 2 skill scope: ingest + slug only; `/kb query`, `/kb lint` wiring, and qmd docs deferred to Stage 3
- Wiki-lint: human-readable stdout report; exit code 1 when any issue found

#### Open questions

- None blocking Stage 2

#### User inputs

- Grill mode: short
- Slug collision: numeric suffix (Recommended)
- CLI runtime: tsx at root package.json (user requested root-level deps, not nested package)
- Skill scope: ingest-only — defer query/lint/qmd to Stage 3 (Recommended)
- Wiki-lint output: human-readable + exit 1 on issues (Recommended)

### Stage 3

#### Summary

#### Decisions

#### Open questions

#### User inputs

## explore

### Summary

Prior explore session established Karpathy wiki direction, rejected reference-repo script heaviness, confirmed Obsidian + Android, discussed immutable sources tradeoffs.

### Decisions

- Explore conclusions carried into assumptions grill; not substituted for planning interview

### Open questions

- Item `d)` forgotten by user

### User inputs

- User accepted `sources/` folder in follow-up propose message
- Lighter ingest confirmed
- Organic buckets: add as content is added, bookmarks may need subfolders later
