## 1. Vault skeleton, Prettier, and schema

**Testable outcome:** `wiki/` opens in Obsidian with navigation files; root `package.json` Prettier scripts work; `.prettierrc` / `.prettierignore` present; `sources/README.md` explains immutability.

- [x] Stage grill: per [task-stage-contract.md](task-stage-contract.md) ## Stage grill line — **this stage only** (`## 1` + unchecked below); **User inputs** under `## apply` ### Stage 1 in `grill-summaries.md`
- [x] 1.1 Create `wiki/index.md`, `wiki/log.md`, `wiki/SCHEMA.md`, `wiki/assets/` per **Requirement: Vault root and navigation files** and **Decision 4**
- [x] 1.2 Create `sources/README.md` describing immutable capture policy per **Requirement: Sources immutability**
- [x] 1.3 Add root `package.json` + `package-lock.json` (pinned prettier + compact-markdown-table), `.prettierrc`, `.prettierignore` per **Requirement: Prettier markdown formatting** and **Decision 6**
- [x] 1.4 Update `.gitignore` for `.obsidian/`, `.qmd/`, `node_modules/` per **Requirement: qmd artifacts gitignored**
- [x] 1.5 Add wiki context summary to `openspec/config.yaml` `context` block
- [x] Verify Decision 1: `wiki/` and `sources/` exist; `sources/README.md` states immutability; Obsidian can open `wiki/` as vault root
- [x] Verify Decision 4: `wiki/SCHEMA.md` exists; repo-root `AGENTS.md` has no wiki ingest rules duplicated in full
- [x] Verify Decision 6: `npm ci` succeeds; `npm run format:check` runs on `wiki/**/*.md`
- [x] Tests: N/A — markdown scaffold and formatter package only
- [x] Apply review: parallel Task — `tacos-apply-review` + `tacos-additional-apply-review` when applicable (parent merge; **MUST NOT** core-only); write `artifacts/openspec-reviews/init-llm-wiki/apply-review-1.md`
- [x] Human review: Pause for human sign-off before stage 2

## 2. TypeScript kb scripts and kb-add skill

**Testable outcome:** `scripts/kb/` provides slug and wiki-lint CLIs; `.agents/skills/kb/SKILL.md` documents ingest, slug invocation, token-light reads, and git publish.

- [x] Stage grill: per [task-stage-contract.md](task-stage-contract.md) ## Stage grill line — **this stage only** (`## 2` + unchecked below); **User inputs** under `## apply` ### Stage 2 in `grill-summaries.md`
- [x] 2.1 Create `scripts/kb/` TypeScript package with `slug` and `wiki-lint` CLIs per **Decision 6**, **Decision 7**, and **Requirement: Semantic wiki lint CLI**
- [x] 2.2 Implement slug: kebab-case, collision suffix per **Requirement: Deterministic slug filenames**
- [x] 2.3 Implement wiki-lint: orphans, broken wikilinks, index gaps per **Requirement: Semantic wiki lint CLI**
- [x] 2.4 Create `.agents/skills/kb/SKILL.md` as user-invocable `/kb` per **Requirement: Single ingest entry point**
- [x] 2.5 Document light vs deep integrate, slug CLI usage, token-light reads, and git flags per **Decision 3**, **Decision 8**, and **Requirement: Slug CLI for filenames**
- [x] Verify Decision 2: skill text states folders are created on first need with no fixed bucket list
- [x] Verify Decision 3: skill text distinguishes default light ingest from `--integrate` / explicit integrate language
- [x] Verify Decision 7: `/kb` skill documents slug CLI invocation; `scripts/kb slug` produces kebab-case output
- [x] Tests: N/A — TypeScript CLIs verified via manual invocation in apply
- [x] Apply review: parallel Task — `tacos-apply-review` + `tacos-additional-apply-review` when applicable (parent merge; **MUST NOT** core-only); write `artifacts/openspec-reviews/init-llm-wiki/apply-review-2.md`
- [x] Human review: Pause for human sign-off before stage 3

## 3. Search, query, gates, and lint wiring

**Testable outcome:** AGENTS.md documents Prettier + wiki-lint gates; `kb` skill documents qmd bootstrap/search/reindex, `/kb query` cited synthesis, and `/kb lint` via CLIs.

- [x] Stage grill: per [task-stage-contract.md](task-stage-contract.md) ## Stage grill line — **this stage only** (`## 3` + unchecked below); **User inputs** under `## apply` ### Stage 3 in `grill-summaries.md`
- [x] 3.1 Add AGENTS.md implementation gates for Prettier check and wiki-lint CLI per **Requirement: Documented gate commands**
- [x] 3.2 Wire `/kb lint` in skill to run wiki-lint CLI + Prettier check per **Requirement: Lint via CLI not LLM checklist**
- [x] 3.3 Add qmd bootstrap, search, post-add reindex, and `/kb query` workflow to `kb` skill per **Requirement: Wiki query workflow** and **Decision 9**
- [x] 3.4 Document Obsidian Android + Obsidian Git mobile workflow in `wiki/SCHEMA.md` per grill mobile decision
- [x] Verify Decision 5: skill or SCHEMA documents qmd collection scoped to `wiki/` and `.qmd/` gitignored
- [x] Verify Decision 6: semantic lint runs via `scripts/kb` CLI; Prettier runs via root `npm run format:check` — not LLM checklist
- [x] Verify Decision 9: skill documents qmd top-N retrieval, cited synthesis, no commit by default, optional file-back
- [x] Tests: N/A — documentation and gate wiring only
- [x] Apply review: parallel Task — `tacos-apply-review` + `tacos-additional-apply-review` when applicable (parent merge; **MUST NOT** core-only); write `artifacts/openspec-reviews/init-llm-wiki/apply-review-3.md`
- [x] Human review: Pause for human sign-off before apply handoff
