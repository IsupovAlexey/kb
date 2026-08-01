## Context

`q:\source\kb` is a fresh tacos/OpenSpec scaffold. The user wants a personal LLM wiki (Karpathy pattern) without the compile-pipeline weight of the reference `aisupov-llm-kb` repo. Private GitHub remote; Obsidian on desktop; Android read/capture via Obsidian mobile.

## Goals

- Git-backed markdown vault at `wiki/` browsable in Obsidian
- Immutable captures at `sources/` for URLs and long pastes; short notes live only in `wiki/`
- One `/kb` skill: ingest, query, lint — classify, summarize, light cross-link, commit, push on add
- qmd search over `wiki/`; on-demand lint via TypeScript CLI
- TypeScript scripts for slug generation and semantic lint; Prettier for markdown format
- Folder taxonomy emerges with content — no fixed bucket list in planning artifacts

## Boundaries

- No C# `kb` CLI, batch `wiki synthesize`, slack/bragbook ingest, or content migration from reference repo
- No Python scripts; TypeScript only under `scripts/`
- No GitHub Actions, CI workflows, or PR automation — direct push to private repo; local AGENTS.md gates only
- Mobile: document Obsidian Android + Obsidian Git only

## Decisions

### 1. Two-layer content layout

`sources/` at repo root holds immutable captures. `wiki/` is the Obsidian vault for derived and short-form pages. Rationale: provenance and safe re-synthesis without a separate compile step; captures are outside the vault so they do not appear in Obsidian graph (accepted). Alternative considered: `wiki/captures/` inside vault — rejected to keep immutability visually separate from browsable wiki.

### 2. Organic folders with frontmatter typing

No pre-seeded bucket list. The `/kb` skill creates paths like `wiki/bookmarks/`, `wiki/notes/`, `wiki/topics/` on first use. Every page carries `date`, `type`, `tags` in frontmatter; `url` when applicable. Rationale: user explicitly deferred fixed taxonomy; bookmarks may gain subfolders (e.g. `wiki/bookmarks/llm/`) as volume grows.

### 3. Light ingest as default contract

Default `/kb` behavior per **Requirement: Light ingest default** in `kb-add` spec: primary page + index + log + links to existing high-confidence matches only. Deep integrate via `--integrate` or explicit user language. Rationale: avoids wiki churn and duplicate topic stubs on every bookmark.

### 4. Schema split

Repo-root `AGENTS.md` remains tacos orchestration plus implementation gates for Prettier and wiki-lint. Wiki-specific rules live in `wiki/SCHEMA.md` (types, capture policy, slug CLI, commit prefix `kb:`, mobile sync notes). Rationale: Karpathy "schema file" pattern without overloading tacos agent entry.

### 5. qmd wiki-only, reindex per add

Single qmd collection over `wiki/`; `.qmd/` gitignored; reindex after each successful add when `qmd` is available. Rationale: matches reference repo pattern at lower complexity; `sources/` search deferred.

### 6. Prettier for format, TypeScript CLI for semantic lint

Follow personalization-hub pattern: isolated `scripts/formatting/` package with pinned Prettier + compact-markdown-table plugin; root `.prettierrc` and `.prettierignore`. Semantic wiki lint (orphans, broken wikilinks, index gaps) lives in `scripts/kb/` TypeScript CLI — separate from Prettier. Rationale: Prettier catches formatting only; deterministic semantic checks save LLM tokens on `/kb lint`. Alternative considered: LLM checklist — rejected after update refinement.

### 7. Slug CLI for deterministic filenames

`scripts/kb/` exposes a slug command (kebab-case, `[a-z0-9-]`, collision suffix). `/kb` invokes it for every new wiki/capture filename. Rationale: consistent naming, fewer tokens than LLM-invented paths. Rules align with tacos-work slug-resolution pattern (lowercase, hyphenated, drop filler words).

### 8. Token-light ingest reads

`/kb` uses qmd top-5 or index catalog for cross-links; bounded URL text for summarization; append-only index/log lines. Rationale: keeps per-add token spend low without a heavy pipeline.

### 9. Query as `/kb` sub-workflow (not a separate skill)

Wiki query uses qmd (or `index.md`) to find relevant pages, reads only top-ranked hits, and synthesizes a cited answer. Invoked as `/kb query "…"` or natural-language question to `/kb`. Does not commit by default; MAY file the answer as a new wiki page when the user asks. Rationale: Karpathy's query operation complements ingest; reuses qmd for token-light retrieval instead of reading the whole vault. Same skill keeps the "one skill" contract.

## Diagrams

```
/kb ingest
  ├─ slug CLI (scripts/kb)     → filename
  ├─ LLM classify + summarize  → page content
  ├─ qmd search top-5          → link candidates
  ├─ write files + append index/log
  ├─ git commit/push
  └─ qmd reindex

/kb lint
  ├─ scripts/kb wiki-lint CLI  → semantic report
  └─ prettier --check wiki/**  → format report

/kb query "…"
  ├─ qmd search top-N (or index.md)
  ├─ read only ranked pages
  └─ cited synthesis (optional file-back to wiki)
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Captures invisible in Obsidian graph | Derived wiki pages link back to capture path in frontmatter or footer |
| Agent under-links on light ingest | User nudges `--integrate`; semantic lint catches orphans |
| Obsidian Git on Android friction | Documented in SCHEMA; user chooses sync strategy |
| Organic folders complicate index | `index.md` updated on every add; wiki-lint catches index gaps |
| Node dependency for scripts | Isolated `scripts/*` packages; `node_modules/` gitignored |

## Migration Plan

Greenfield — no migration. Init creates empty skeleton; first `/kb` add populates content.

## Open Questions

- Forgotten explore item `d)` — unknown
