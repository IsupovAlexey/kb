# Source scanning

What `/tacos-test-plans` reads before synthesis. Record all scanned inputs in the test-cases metadata header; MAY duplicate the audit in `sources.md`.

## Always scan

|Source|Notes|
|-|-|
|User prompt|Scope, slug hints, supplementary paths, pasted URLs|
|`openspec/specs/**`|When present in the repo; primary requirement source|

## Optional (user-named)

|Source|When|
|-|-|
|`openspec/changes/<name>/`|User names a change — read `proposal.md`, `specs/**`, `design.md`|
|Supplementary spec directories|User provides paths outside `openspec/specs/`|
|Explicit filesystem paths|Any repo path the user lists|
|Pasted URLs|Jira, Figma, Confluence, etc.|

## External URLs

When URLs appear in the prompt or declared sources, read them with host tools when available (Atlassian MCP or CLI, Figma MCP, fetch). Record each URL and whether content was read in **Source** metadata and optional `sources.md`. Do not invent behavior from unread URLs.

## Host gather skills

Hosts MAY use domain-specific gather skills (grooming notes, CSV tooling, spreadsheets). Those skills are **inputs only** — not part of the tacos test-plans contract. List what was used in metadata.

## Scan procedure

1. Resolve slug and list user-declared sources.
2. Walk `openspec/specs/**` when not excluded by user.
3. When change folder named, load planning artifacts under `openspec/changes/<name>/`.
4. Read supplementary paths when present and readable.
5. When URLs are present, try host tools to read linked content; record read status.
6. Build requirement index: verbatim `### Requirement:` titles → spec path.
7. Note gaps, conflicts, and unread URLs for preview gate open questions.

## sources.md (optional companion)

When the scan is non-trivial, write `sources.md`:

```markdown
# Sources — {slug}

|Path or URL|Role|Read|
|-|-|-|
|openspec/specs/foo/spec.md|Primary spec|yes|
|https://…|Figma|yes|
|https://…|Jira|metadata only|
```
