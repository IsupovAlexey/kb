## ADDED Requirements

### Requirement: Vault root and navigation files

The wiki vault SHALL live at `wiki/` relative to the repository root. The vault SHALL include `wiki/index.md` (content catalog) and `wiki/log.md` (chronological append-only activity log).

#### Scenario: Fresh vault opened in Obsidian

- **WHEN** a user opens the `wiki/` folder as an Obsidian vault
- **THEN** `index.md` and `log.md` exist at the vault root and are valid markdown

### Requirement: Organic folder creation

The system SHALL NOT require a fixed taxonomy of content folders at init. The `/kb` skill SHALL create subfolders under `wiki/` on first need (e.g. `wiki/bookmarks/`, `wiki/notes/`, `wiki/topics/`).

#### Scenario: First bookmark without folder hint

- **WHEN** `/kb` ingests a URL and no target folder is specified
- **THEN** the skill creates `wiki/bookmarks/` if absent and writes the page there

#### Scenario: Bookmark subfolder emerges later

- **WHEN** the user or skill places content under a nested path such as `wiki/bookmarks/llm/`
- **THEN** the path is valid without prior planning-artifact updates

### Requirement: Deterministic slug filenames

Wiki and capture filenames SHALL be generated via the TypeScript slug CLI in `scripts/kb/` (kebab-case, `[a-z0-9-]`, collision suffix when needed). The `/kb` skill SHALL invoke this CLI rather than inventing filenames in prose.

#### Scenario: Bookmark title produces slug

- **WHEN** `/kb` ingests a bookmark titled "Karpathy LLM Wiki"
- **THEN** the slug CLI returns a kebab-case filename such as `karpathy-llm-wiki.md` (with hash suffix on collision)

#### Scenario: Slug collision

- **WHEN** the slug CLI detects an existing file with the same base name
- **THEN** it appends a short deterministic suffix so the new path is unique

### Requirement: Assets directory

Images and file attachments referenced from wiki pages SHALL be stored under `wiki/assets/`.

#### Scenario: Image ingest

- **WHEN** `/kb` processes an image attachment
- **THEN** the binary is saved under `wiki/assets/` and the wiki page links to it with a relative path

### Requirement: Wiki schema document

Wiki conventions SHALL be documented in `wiki/SCHEMA.md` including: page frontmatter fields, capture vs derived rules, folder policy, slug CLI usage, commit message pattern, and light vs deep ingest behavior.

#### Scenario: Agent reads conventions before ingest

- **WHEN** the `/kb` skill runs
- **THEN** it reads `wiki/SCHEMA.md` for formatting and policy rules

### Requirement: Sources immutability

Long-form and URL captures SHALL be written under `sources/` at the repository root. After initial commit, the `/kb` skill SHALL NOT modify the body of an existing capture file except on explicit user request to fix a capture error.

#### Scenario: URL bookmark ingest

- **WHEN** `/kb` ingests a URL or long paste warranting a capture
- **THEN** an immutable capture file is written under `sources/` and a derived summary page is written under `wiki/`

#### Scenario: Short note ingest

- **WHEN** `/kb` ingests a short note that does not warrant a capture
- **THEN** content is written only under `wiki/` with no `sources/` file

#### Scenario: User-requested capture correction

- **WHEN** the user explicitly asks to fix an error in an existing capture file
- **THEN** the skill MAY edit that capture's body and notes the correction in `wiki/log.md`

### Requirement: Index and log maintenance on add

Each successful `/kb` add SHALL append one line to `wiki/log.md` and add or update the corresponding entry in `wiki/index.md`.

#### Scenario: Successful bookmark add

- **WHEN** `/kb` completes a bookmark ingest
- **THEN** `wiki/log.md` gains a dated entry and `wiki/index.md` lists the new page with a one-line summary
