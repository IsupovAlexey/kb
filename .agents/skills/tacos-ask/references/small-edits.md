# Small edits

When the user requests a repo change during `tacos-ask`, assess scope before writing.

## In scope (stay in ask)

|Signal|Examples|
|-|-|
|Few files|One planning artifact, a small code/doc patch, a main spec clarification|
|Single outcome|Fix typo, clarify a decision, adjust wording aligned with loaded context|
|User-requested|User explicitly asks for the edit (not agent-initiated scope creep)|
|Bound change or no-change sources|Files under `openspec/changes/<id>/`, touched `openspec/specs/**`, or paths implicated by the question|

## Confirm before write

Before the first repo edit in the session (and again when scope expands materially):

1. Summarize intended files and change.
2. Structured gate per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): when tools are available, next tool call MUST be `AskQuestion` / `AskUserQuestion` with `proceed` / `edit` / `cancel` — forbidden: prose-only menu. Plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools are absent.
3. Write only after `proceed` or explicit execute-after-corrections in chat.

No session file — lighter than `tacos-work`, but same confirm shape.

## Out of scope (route out)

|Signal|Route|
|-|-|
|Multi-file feature or multiple independent outcomes|[escape-hatches](escape-hatches.md) → `tacos-work` or `apply`|
|Needs planning grill, apply-review gate, or Work session file|`tacos-work`|
|Staged apply, task checkoffs, or orchestration gates|full `apply`|
|Output under `artifacts/` (handoff, work session, review)|sibling skill per escape hatches|
|New capability or multi-day scope|`propose` or `tacos-work`|

When signals conflict, prefer routing out over stretching "small."

## After edit

Report what changed with path citations. Do not check off change `tasks.md` checkboxes.

When `orchestration.enabled` and the confirmed edit touched `openspec/specs/**` or implementation paths, run optional main-spec dedrift pass per [orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md) § tacos-ask before ending the turn.
