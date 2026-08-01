# Push from current `jira.md` (no regeneration)

Sync the **existing** bound ticket using content already in `jira.md` — do not recompose from other artifacts.

## Pre

- `jira.md` exists with `issue_key` and `url` in frontmatter.
- `jira.enabled: true` and transport available for writes.

## Steps

1. Read `title` (or `#` heading) and markdown body from `jira.md`.
2. Present key + summary + full description.
3. Ask approval via [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#jira-push) and [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): when tools available, **next tool call MUST** be `AskQuestion` / `AskUserQuestion` after preview — on **`approve`**, `push`. Plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable → **end turn** → next message yes → `push`.
4. `push` via MCP/CLI (`summary`, `description` only).

Approval gate: non-bypassable — see `SKILL.md` **Approval** and [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#jira-push). Re-preview after `jira.md` edits.

## Contrast

|Intent|Workflow|
|-|-|
|Update Jira from **artifacts**|`regenerate-jira-md` → approve → `push-from-jira-md`|
|Update Jira from **current jira.md**|`push-from-jira-md` only|
|Pull remote ticket into file|`fetch`|

`compose-description` is preview-only (no `jira.md` write); see `SKILL.md` **compose-description**.
