## ADDED Requirements

### Requirement: Prettier markdown formatting

Markdown under `wiki/` and `sources/` SHALL be formattable via Prettier using an isolated package at `scripts/formatting/` (personalization-hub pattern: pinned `prettier` + `prettier-plugin-compact-markdown-table`, root `.prettierrc`, `.prettierignore`).

#### Scenario: Format check on wiki markdown

- **WHEN** a user or agent runs the documented Prettier check command on `wiki/**/*.md`
- **THEN** Prettier reports formatting violations without modifying files

#### Scenario: Format write on wiki markdown

- **WHEN** a user or agent runs the documented Prettier write command
- **THEN** markdown files are reformatted per `.prettierrc`

### Requirement: Semantic wiki lint CLI

The repository SHALL provide a TypeScript CLI under `scripts/kb/` that reports semantic wiki issues: orphan pages, broken `[[wikilinks]]`, and index gaps. Prettier SHALL NOT be used for these checks.

#### Scenario: User requests semantic lint

- **WHEN** the user asks to lint or health-check the wiki (e.g. `/kb lint`)
- **THEN** the agent runs the TypeScript wiki-lint CLI and returns its report

#### Scenario: Orphan page detected

- **WHEN** a wiki page has no inbound wikilinks from `index.md` or other pages (excluding `index.md`, `log.md`, `SCHEMA.md`)
- **THEN** the CLI lists the orphan path

#### Scenario: Broken wikilink detected

- **WHEN** a page contains `[[target]]` and no matching file exists under `wiki/`
- **THEN** the CLI reports the source file and unresolved target

#### Scenario: Index gap detected

- **WHEN** a content page under `wiki/` is absent from `wiki/index.md`
- **THEN** the CLI reports the missing index entry

#### Scenario: Default semantic lint does not mutate

- **WHEN** the wiki-lint CLI runs without an explicit fix flag
- **THEN** output is a report only

### Requirement: Documented gate commands

`AGENTS.md` implementation gates SHALL document Prettier check/write commands and the semantic wiki-lint CLI invocation for markdown changes under `wiki/` and `sources/`.

#### Scenario: Agent edits wiki markdown

- **WHEN** an agent completes wiki or sources markdown edits
- **THEN** `AGENTS.md` lists the commands to run for format check and optional semantic lint
