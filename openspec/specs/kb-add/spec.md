## Purpose

Single `/kb` skill for ingesting URLs, pastes, files, and short notes into the personal LLM wiki.

## Requirements

### Requirement: Single ingest entry point

The repository SHALL provide one user-invocable Cursor skill at `.agents/skills/kb/SKILL.md` invocable as `/kb`.

#### Scenario: User invokes kb skill

- **WHEN** the user runs `/kb` with a URL, paste, file path, or short note
- **THEN** the agent follows the skill workflow without requiring a separate CLI

### Requirement: Input classification

On each invoke, the skill SHALL classify content `type` and `tags` when the user does not supply them. Suggested `type` values include `bookmark`, `note`, `image`, `video`, `idea`, and `citation`; the set is open.

#### Scenario: Unlabeled URL paste

- **WHEN** the user pastes a URL without metadata
- **THEN** the skill sets `type: bookmark`, fetches or summarizes the target, and assigns relevant `tags`

### Requirement: Light ingest default

By default, `/kb` SHALL perform light synthesis: update index and log, add wikilinks to existing topic or people pages, and create a new topic or people page only when match confidence is high. It SHALL NOT run a full multi-page entity rebuild.

#### Scenario: Default bookmark add

- **WHEN** `/kb` runs without integrate flags
- **THEN** it writes the primary page(s), updates index and log, and links only to existing related pages where confidence is high

#### Scenario: Deep integrate on nudge

- **WHEN** the user includes `--integrate` or explicitly asks to integrate into topics
- **THEN** the skill MAY create or update multiple entity pages across the wiki

### Requirement: Git publish default

Unless the user passes `--no-commit` or `--no-push`, `/kb` SHALL commit and push changes after a successful ingest.

#### Scenario: Default publish

- **WHEN** `/kb` completes ingest without no-publish flags
- **THEN** a git commit is created with message prefix `kb:` and pushed to the remote

#### Scenario: Dry run

- **WHEN** the user passes `--dry-run` or `--no-commit`
- **THEN** files are written or previewed but not committed

### Requirement: Partial and fetch failure handling

If URL fetch or summarization fails, the skill SHALL still write a wiki stub with available metadata (URL, title if known) and report the fetch failure. If file writes fail, the skill SHALL not commit. If writes succeed but git commit or push fails, the skill SHALL report the failure without claiming full success.

#### Scenario: URL fetch fails

- **WHEN** `/kb` cannot fetch a bookmark URL
- **THEN** a wiki page is written with URL and a note that fetch failed; no success claim for summary content

#### Scenario: Push failure after commit

- **WHEN** commit succeeds and push fails
- **THEN** the user receives an explicit error and instructions to retry push

#### Scenario: Write failure before commit

- **WHEN** a file write fails during ingest
- **THEN** no git commit is attempted and the user receives an explicit error

### Requirement: Link and media summaries

For bookmarks, videos, and external links, the derived wiki page SHALL include a human-readable summary section.

#### Scenario: Video URL add

- **WHEN** `/kb` ingests a video URL
- **THEN** the wiki page includes title, URL, and a `## Summary` section

### Requirement: Slug CLI for filenames

The `/kb` skill SHALL call the TypeScript slug CLI in `scripts/kb/` to generate wiki filenames.

#### Scenario: Filename on ingest

- **WHEN** `/kb` writes a new wiki page
- **THEN** the slug CLI determines the filename before write

### Requirement: Token-light read discipline

On default ingest, the skill SHALL use qmd search (top 5 results) or `wiki/index.md` for cross-link candidates instead of reading all wiki page bodies. URL content used for summarization SHALL be truncated to a bounded length before LLM summarization. Index and log updates SHALL append lines rather than rewriting entire files.

#### Scenario: Cross-link discovery

- **WHEN** `/kb` seeks related topic pages on a default add
- **THEN** it uses qmd search or index catalog before opening full page bodies

### Requirement: Lint via CLI not LLM checklist

When the user requests wiki lint, the skill SHALL run the TypeScript wiki-lint CLI and Prettier format check commands documented in `AGENTS.md`, not an LLM-driven file-by-file checklist.

#### Scenario: Lint request

- **WHEN** the user invokes `/kb lint` or asks for a wiki health check
- **THEN** the agent runs `scripts/kb/` wiki-lint and optional Prettier check commands

### Requirement: Bulk Firefox bookmark import

The repository SHALL provide a TypeScript CLI to import Firefox Netscape bookmark HTML exports into the wiki with folder-aware paths, dead-link exclusion, and short summaries from fetched page content.

#### Scenario: Firefox HTML import

- **WHEN** the user runs the Firefox import CLI against a bookmarks HTML file
- **THEN** the tool creates wiki bookmark pages under `wiki/bookmarks/` mirroring folder structure, and appends batch summary lines to `wiki/index.md` and `wiki/log.md`

#### Scenario: Dead link exclusion

- **WHEN** a bookmark URL fails fetch or returns an HTTP error during import
- **THEN** no wiki page is created for that URL and the manifest records it as dead or skipped with reason

#### Scenario: Imported bookmark summary

- **WHEN** a bookmark URL is successfully fetched during bulk import
- **THEN** the wiki page includes a `## Summary` section with excerpt or meta description from the fetched content

#### Scenario: Idempotent re-import

- **WHEN** the Firefox import CLI is run again for the same export date and URLs were already imported
- **THEN** existing imported URLs are skipped without creating duplicate wiki pages

### Requirement: Bulk Google Keep import

The repository SHALL provide a TypeScript CLI to import Google Keep Takeout zip exports into the wiki with standard-split classification, label-to-theme mapping, dead-link exclusion, image attachments, and idempotent re-run.

#### Scenario: Keep Takeout import

- **WHEN** the user runs the Keep import CLI against a Takeout zip containing `Takeout/Google Keep/*.json`
- **THEN** the tool creates wiki pages under `wiki/bookmarks/` and `wiki/notes/` per classification rules, copies image attachments to `wiki/assets/`, and appends batch summary lines to `wiki/index.md` and `wiki/log.md`

#### Scenario: Keep classification standard-split

- **WHEN** a Keep note has WEBLINK annotations or URL-only text content
- **THEN** it is imported as a bookmark page with fetch and `## Summary`
- **WHEN** a Keep note has freeform text only
- **THEN** it is imported as a note page under `wiki/notes/<theme>/`
- **WHEN** a Keep note has both non-URL text and embedded URLs
- **THEN** it is imported as a single note page (not split)

#### Scenario: Keep label mapping

- **WHEN** a Keep note has labels such as IT, TOPDeck, or ServiceTitan
- **THEN** labels become frontmatter tags and map to wiki themes (`programming`, `games`, `work`); unmapped labels use `wiki/notes/personal` or `wiki/bookmarks/unsorted`

#### Scenario: Keep dead link exclusion

- **WHEN** a bookmark-bound Keep URL fails health check or fetch during import
- **THEN** no wiki page is created for that URL and the manifest records it as dead with reason

#### Scenario: Keep image attachments

- **WHEN** a Keep note has image attachments
- **THEN** images are copied to `wiki/assets/` and the wiki note links to the asset path

#### Scenario: Keep idempotent re-import

- **WHEN** the Keep import CLI is run again for the same export date
- **THEN** notes already recorded in the manifest and bookmark URLs already in the wiki are skipped without duplicate pages

#### Scenario: Keep dry-run

- **WHEN** the user passes `--dry-run` to the Keep import CLI
- **THEN** a manifest is written under `artifacts/kb-import/` and no wiki files are created
