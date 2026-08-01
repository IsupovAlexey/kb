# Change binding

Optional OpenSpec change id for planning-context-aware walkthrough.

## Runtime (structured ask)

Same as tacos binding and approval flows — [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md) (runtime table) and [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) (one topic per structured call):

|Runtime|Tool|
|-|-|
|Cursor|`AskQuestion`|
|Claude Code|`AskUserQuestion`|
|Other / unavailable|Plain-text fallback below — **end turn**; wait for next user message|

Structured selection is the answer. Silence or implied consent does not bind.

## Modes

|Mode|When|Confirm|
|-|-|-|
|**Explicit**|User passes change id in invoke (second token or named flag)|No — load immediately|
|**Strong evidence**|[Auto-bind](#strong-evidence-auto-bind) — unambiguous match|No — bind and note evidence in chat (one line)|
|**Heuristic**|Weaker match (e.g. only active change)|Yes — [Heuristic binding confirm](#heuristic-binding-confirm)|
|**Ad hoc**|User selects `adhoc`, no match, or ambiguous after one ask|Planning set not loaded|
|**Skip binding**|User selects `skip` on confirm|Ad hoc for this review; do not ask binding again this session|

## Heuristic signals

1. `git branch --show-current` equals change folder name, or ends with `/` + change id (e.g. `feat/add-<feature>` → `add-<feature>`).
2. PR title/body contains `openspec/changes/<id>` or bare change id in tacos convention.
3. Single active change under `openspec/changes/` (non-archive) — **weak**; confirm unless [strong evidence](#strong-evidence-auto-bind).

When two or more change ids score equally (margin ≤1, same as [change-resolution.md](../../tacos-pr/references/change-resolution.md) ambiguity), use [Multiple candidates](#multiple-candidates) instead of picking a default.

## Strong evidence auto-bind

Bind **without** `AskQuestion` when **one** change id is clear and any of:

|Signal|Example|
|-|-|
|**Branch basename**|`git branch --show-current` is `add-<feature>` or `feat/add-<feature>` and `openspec/changes/add-<feature>/` exists|
|**PR marker**|PR title or body contains `openspec/changes/<id>/` or the change id as a standalone token in the host project's PR|
|**Explicit + exists**|User passed change id and folder exists|

Require **at least one** strong row. Do **not** auto-bind on “only active change” alone, foreign forks, or tied scores — use [Heuristic binding confirm](#heuristic-binding-confirm) or [Multiple candidates](#multiple-candidates).

After auto-bind, state in chat one line: `Bound to <change-id> (<evidence>).` then continue (delegation, walkthrough).

## Heuristic binding confirm

When heuristic suggests `{change-id}`:

1. Show **evidence** in chat (branch name, PR title snippet, or “only active change” — one line each).
2. Structured gate per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): **next tool call MUST** be `AskQuestion` / `AskUserQuestion` when tools available (same turn as evidence) — **forbidden:** prose-only binding menu.

**Prompt:** `Bind assisted review to OpenSpec change "{change-id}"?`

|id|label|Action|
|-|-|-|
|`bind`|Bind to {change-id}|Load planning set — [After bind](#after-bind)|
|`adhoc`|Ad hoc review|[rubric-adhoc](rubric-adhoc.md) only|
|`skip`|Skip binding this session|Ad hoc; do not ask binding again this session|

Substitute `{change-id}` in the prompt and in the `bind` label.

### Plain-text fallback

```text
Bind assisted review to OpenSpec change "{change-id}"?
Evidence: <branch / PR marker / active-change note>

1. bind — load planning set from openspec/changes/{change-id}/
2. adhoc — walkthrough without planning context
3. skip — ad hoc; do not ask binding again this session
```

End turn until the user replies with `bind`, `adhoc`, `skip`, or an unambiguous paraphrase (`yes bind`, `1`, …). Ambiguous `ok` → re-ask once (structured if possible), then stop.

Record the chosen id in walkthrough meta. When uncertain after one ask, prefer `adhoc`.

## Multiple candidates

When heuristics tie or several actives match:

**Prompt:** `Which OpenSpec change should assisted review bind to?`

|id|label|
|-|-|
|`<change-id-a>`|Bind to {change-id-a}|
|`<change-id-b>`|Bind to {change-id-b}|
|`adhoc`|Ad hoc review|
|`skip`|Skip binding this session|

One option per candidate change id (kebab-case id equals option id). Add `adhoc` and `skip` as above. Plain-text: numbered list with the same ids.

## Resolution order

1. Explicit change argument from invoke.
2. Heuristic match → [Strong evidence auto-bind](#strong-evidence-auto-bind) if qualified; else [Heuristic binding confirm](#heuristic-binding-confirm) (or [Multiple candidates](#multiple-candidates)).
3. Else ad hoc.

Heuristic match loads planning context only after structured `bind` (or explicit arg). `openspec list` alone never auto-binds.

## After bind

Load only under `openspec/changes/<id>/`:

- `proposal.md`, `specs/**`, `design.md`, `tasks.md`, `grill-summaries.md` (if present)

Planning-set load under `openspec/changes/<id>/` only — see [rubric-bound](rubric-bound.md). Gate findings for look outs: [review-delegation](review-delegation.md) reuse (assisted or staged artifacts when same diff) or fresh delegation.
