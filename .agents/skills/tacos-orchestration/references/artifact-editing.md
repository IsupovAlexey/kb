# Artifact editing (user-directed removal)

## When this applies

|Trigger|Load|
|-|-|
|**`propose` / `ff` / `continue` / `apply`**|Full orchestration `SKILL.md`; this file from **Artifact edits**|
|**Any other turn**|Host `AGENTS.md` **Artifact removal** — **read this file** before editing `openspec/changes/**`|

Orchestration is **not** invoked for ad-hoc edits — do not run grill, POST-ARTIFACT, or status loops. Only follow the removal rules below.

## Removal rule

When the user asks to **remove** something from OpenSpec change artifacts under `openspec/changes/**` (`proposal.md`, `design.md`, `tasks.md`, `specs/**/*.md`, `grill-summaries.md`, `e2e-scenarios.md`, `jira.md`, etc.).

## Do

- Delete the content — section, requirement, task, bullet, table row, or file — so the artifact reads as if it were never there.
- Renumber or fix headings/lists only when needed for a coherent document after deletion.

## Do not

- Leave tombstones, audit trails, or meta-commentary about the edit.
- Replace removed content with placeholders such as “removed”, “N/A”, “not applicable”, “deferred”, “won’t do”, or “(deleted)”.
- Add notes like “user asked to drop X” or “no longer in scope” where X used to be.
- Use strikethrough, HTML comments, or empty sections that only explain absence.
- Keep checklist items marked cancelled with prose explaining why — **delete** the item.

## Chat vs artifacts

In chat you may briefly confirm what you removed. **Artifacts** stay clean; do not mirror removal narration into the files.
