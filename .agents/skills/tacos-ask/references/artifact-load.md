# Artifact load (bound change)

When a change id is bound, read existing files under `openspec/changes/<id>/` **before** answering questions or editing files for that change.

## Assisted set (default)

Load every file that exists:

|Path|Use|
|-|-|
|`proposal.md`|Motivation, capabilities, impact|
|`specs/**`|Delta requirements and scenarios|
|`design.md`|Decisions, boundaries, risks|
|`tasks.md`|Staged work and acceptance|
|`grill-summaries.md`|Resolved grill decisions when present|

## Exclusions (default)

Do **not** load under `artifacts/openspec-reviews/` unless the user explicitly asks about apply-review or planning-review output.

## Partial bundle

When expected files are missing and the question implicates them, state which artifacts are absent and answer from what is available.

## Citations

Answers about bound changes MUST cite loaded files with **path + short quote** (e.g. `design.md` Decision 2: "…").
