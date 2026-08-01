# Review-plan runbook

`/tacos-audit review-plan <file>` — critique and tighten an existing audit-plan for executor readiness. Read-only on application source; may rewrite the plan artifact after preview Proceed.

## Preconditions

- Path is under `artifacts/tacos-audit/<slug>/` — `audit-plan.md` or `plans/NNN-*.md`
- Plan already exists (from `/tacos-audit plan` or manual draft the user points at)
- User did **not** request a greenfield plan without findings — redirect to `/tacos-audit plan` or `/tacos-work` / propose

## Review checklist

Re-read every cited `file:line` in the plan (do not trust plan excerpts alone). Score each dimension:

|Dimension|Pass when|
|-|-|
|Self-contained context|Paths, excerpts, and conventions inlined — no “as discussed”|
|Verification gates|Each **## Work** row ends with runnable command + expected outcome|
|STOP conditions|Explicit stop-and-report cases when reality diverges|
|`planned_at_sha`|Present; drift check command matches scope paths|
|Scope in/out|Bounded; no scope creep into unrelated surfaces|
|Handoff fit|`handoff` matches scope — execute vs work vs propose vs dedrift|

## Procedure

1. Load plan file; extract slug from path or frontmatter `audit_slug`.
2. Run drift check command from plan if `planned_at_sha` is stale — note in preview.
3. Draft tightened plan (same file path); do **not** invent new findings or change `finding_ids` without user ask.
4. Preview full revised plan — [preview-gate](preview-gate.md) (Proceed / Edit / Cancel).
5. On Proceed: overwrite plan file only; record path in chat.

## Out of scope

- New audit categories or findings table
- Source code edits
- tacos-grill planning interviews
- Skipping preview gate
