# Change binding

Optional OpenSpec change id for planning-context-aware **triage hints** only. Binding does not turn triage into spec-compliance review.

## Runtime (structured ask)

Same as tacos binding flows — [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md) (runtime table) and [interview-prompt.md](../../tacos-grill/references/interview-prompt.md):

|Runtime|Tool|
|-|-|
|Cursor|`AskQuestion`|
|Claude Code|`AskUserQuestion`|
|Other / unavailable|Plain-text fallback — **end turn**|

## Modes

|Mode|When|Confirm|
|-|-|-|
|**Explicit**|User passes change id in invoke|No — load immediately|
|**Strong evidence**|Unambiguous match (branch basename, PR marker)|No — bind; note evidence in chat|
|**Heuristic**|Weaker match (e.g. only active change)|Yes — one structured confirm|
|**Ad hoc**|User declines or no match|PR-only triage|
|**Skip binding**|User selects skip|Do not ask binding again this session|

Heuristic signals match [change-binding in tacos-assisted-review](../../tacos-assisted-review/references/change-binding.md): branch basename, PR title/body marker, single active change (weak). For **strong evidence auto-bind**, **multiple candidates**, and confirm prompt tables, load [§ Strong evidence auto-bind](../../tacos-assisted-review/references/change-binding.md#strong-evidence-auto-bind), [§ Multiple candidates](../../tacos-assisted-review/references/change-binding.md#multiple-candidates), and [§ Heuristic binding confirm](../../tacos-assisted-review/references/change-binding.md#heuristic-binding-confirm) only — not the full assisted-review doc.

## After bind

Load only under `openspec/changes/<id>/`:

- `proposal.md`, `specs/**`, `design.md`, `tasks.md`, `grill-summaries.md` (if present)

Record `change_id` in `_session.md`. Use planning context to inform validity/action hints — **never** emit BLOCKER/APPROVE or gate-review findings.

## Prompt template (heuristic confirm)

`Bind PR triage to OpenSpec change "{change-id}"?`

|id|label|
|-|-|
|`bind`|Bind to {change-id}|
|`adhoc`|PR-only triage|
|`skip`|Skip binding this session|
