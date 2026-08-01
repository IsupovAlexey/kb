---
name: tacos-ask
description: >-
  Change-bound Q&A and small bounded edits from planning artifacts or main specs;
  confirm before repo writes. Invoke via /tacos-ask only (not ambient orchestration).
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Optional change id and question (e.g. add-<feature> fix the typo in design Decision 2)
---

# tacos ask

Change-bound Q&A and small edits in the current chat.

## Quick start

|User says|Action|
|-|-|
|`/tacos-ask <question>`|Resolve change → load context → answer with citations|
|`/tacos-ask <change-id> <question>`|Bind named change → load → answer|
|`/tacos-ask fix X in design.md`|Load → confirm → small edit per [small-edits](references/small-edits.md)|
|`/tacos-ask` (no text)|Resolve change → ask once for question if needed|
|"What does the design say about X for this change?"|Same when ask intent is clear|

Route large implementation or orchestration workflows to sibling skills per [escape-hatches](references/escape-hatches.md) (planning review, apply-review, grill, dedrift, jira, PR tools).

## Procedure

1. Resolve change — [change-resolution](references/change-resolution.md). When slug is `_no-change`, follow [no-change-mode](references/no-change-mode.md).
2. Load context — [artifact-load](references/artifact-load.md) for bound changes; [no-change-mode](references/no-change-mode.md) sources when slug is `_no-change`.
3. Answer (when the user asked a question) — cite loaded files with path + short quote. State missing artifacts when the question implicates them. Skip when the invoke is edit-only (see Quick start).
4. Small edit (when requested) — assess scope per [small-edits](references/small-edits.md); confirm before write; apply or route to [escape-hatches](references/escape-hatches.md). After confirmed writes that touched main specs or implementation, optional main-spec dedrift pass when `orchestration.enabled` — [orchestration-dedrift-pass](../tacos-dedrift/references/orchestration-dedrift-pass.md) § tacos-ask.
5. Escalate — when scope exceeds small edits or needs a different workflow, point to [escape-hatches](references/escape-hatches.md).

## Gates

Preview → structured prompt per [structured-gate-convention](../tacos-orchestration/references/structured-gate-convention.md) (next tool call MUST be `AskQuestion` / `AskUserQuestion` when available) → else plain text + end turn per [interview-prompt](../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable. Detail: [small-edits](references/small-edits.md) § Confirm before write.

|Step|Before repo write|Option ids|
|-|-|-|
|Small edit|First file change|`proceed` / `edit` / `cancel`|

No planning grill, staged apply, or task checkoffs from this skill.

## Done when

- Q&A: Change resolved per [change-resolution](references/change-resolution.md); bound changes load context per [artifact-load](references/artifact-load.md); answer cites path + short quote; missing artifacts named when the question implicates them.
- Small edit: Scope fits [small-edits](references/small-edits.md); user confirmed Proceed before the first repo write; report what changed with path citations; no task checkoffs on change `tasks.md`.
- Route out: When scope exceeds small edits or needs orchestration gates, user pointed to the sibling in [escape-hatches](references/escape-hatches.md) with reason before ending.

## When to use

|Situation|Path|
|-|-|
|Questions about what planning artifacts say|`tacos-ask`|
|Small fix in the active change thread (few files, confirm)|`tacos-ask`|
|Bounded feature with planning grill and apply-review|`tacos-work`|
|Multi-stage change-folder implementation|full `apply`|
|Open option tradeoffs or spikes|`explore`|
|Compact for next session|`tacos-handoff`|
|Walk through a PR diff|`tacos-assisted-review`|

## References

[change-resolution](references/change-resolution.md) · [artifact-load](references/artifact-load.md) · [no-change-mode](references/no-change-mode.md) · [small-edits](references/small-edits.md) · [escape-hatches](references/escape-hatches.md)
