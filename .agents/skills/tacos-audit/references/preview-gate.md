# Audit preview gate

Before writing or rewriting `audit-plan.md` or `plans/*.md` (including `/tacos-audit plan` and `/tacos-audit review-plan`), show the full plan summary in chat and collect approval.

## Preview (required)

Include per selected finding:

- Finding ids and titles
- `handoff` frontmatter value
- Scope in/out summary
- **## Work** checklist preview
- `planned_at_sha` to stamp

Multi-plan: show each file path when using `plans/NNN-*.md`.

## Gate

Per [structured-gate-convention](../../tacos-orchestration/references/structured-gate-convention.md): when structured tools are available, **next tool call MUST** be `AskQuestion` / `AskUserQuestion` after preview.

**Prompt:** `Write these audit plan files?`

|id|label|
|-|-|
|`proceed`|Proceed — write audit-plan file(s)|
|`edit`|Edit — revise plan; re-preview|
|`cancel`|Cancel — no writes|

On **`proceed`:** write only under `artifacts/tacos-audit/<slug>/`; record paths in chat.

On **`edit`:** incorporate corrections; re-preview; gate again.

On **`cancel`:** state cancelled; no writes.

**Never counts as approval:** silence; ambiguous “looks good”; invoking plan without gate when tools were available.

## Execute confirm (separate gate)

Before `/tacos-audit execute`, collect explicit confirm after drift check — [execute-runbook](execute-runbook.md). Not a substitute for this plan preview gate.

## No planning grill

This skill MUST NOT run tacos-grill planning interviews. Downstream `/tacos-work` and `/opsx-propose` own grill when `handoff` routes there.
