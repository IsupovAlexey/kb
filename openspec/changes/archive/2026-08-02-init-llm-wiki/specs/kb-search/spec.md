## ADDED Requirements

### Requirement: qmd indexes wiki only

Local search via qmd SHALL index markdown under `wiki/` only. `sources/`, `openspec/`, and `.agents/` SHALL NOT be indexed.

#### Scenario: Search scope

- **WHEN** qmd collection is configured for this repository
- **THEN** indexed paths are limited to `wiki/**/*.md`

### Requirement: Reindex after kb add

After each successful `/kb` add (when qmd is installed), the skill SHALL run qmd reindex for the wiki collection or document the reindex command the user must run.

#### Scenario: qmd available after add

- **WHEN** `/kb` completes ingest and `qmd` is on PATH
- **THEN** the wiki search index is updated before the skill finishes

#### Scenario: qmd unavailable

- **WHEN** `/kb` completes ingest and `qmd` is not installed
- **THEN** ingest still succeeds and the skill notes that search reindex was skipped

### Requirement: Search invocation

The `kb` skill SHALL document how to search the wiki via qmd (CLI command or skill sub-workflow).

#### Scenario: User searches wiki

- **WHEN** the user asks to search the wiki for a term
- **THEN** the agent runs qmd search against the wiki collection and returns ranked results

### Requirement: qmd artifacts gitignored

The `.qmd/` index directory SHALL be listed in `.gitignore`.

#### Scenario: Fresh clone

- **WHEN** a user clones the repository
- **THEN** `.qmd/` is not tracked and bootstrap instructions explain how to create the index

### Requirement: Wiki query workflow

The `kb` skill SHALL support querying the wiki: use qmd search (top N, default 5) or `wiki/index.md` to find candidate pages, read only those pages, and synthesize an answer with citations to wiki paths. Query SHALL NOT commit or push by default.

#### Scenario: User asks a wiki question

- **WHEN** the user invokes `/kb query "what do I know about X?"` or asks a natural-language question via `/kb`
- **THEN** the agent runs qmd search (or reads index), opens top-ranked pages only, and returns a cited answer

#### Scenario: Low-confidence retrieval

- **WHEN** qmd returns no relevant results above a reasonable threshold
- **THEN** the agent states that the wiki has no confident answer rather than synthesizing from unrelated pages

#### Scenario: File answer back to wiki

- **WHEN** the user asks to save the query answer (e.g. "file this" or `/kb query --file`)
- **THEN** the answer is written as a new wiki page, index/log updated, and changes committed per ingest publish rules

#### Scenario: Query does not publish by default

- **WHEN** `/kb query` completes without a file-back request
- **THEN** no git commit or push occurs
