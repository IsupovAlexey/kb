# Sync / archive prompts (orchestration)

After stock sync or archive, run these gates in order before draft or write. Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): when structured tools are in the session tool list, each gate's next tool call MUST be `AskQuestion` / `AskUserQuestion` — forbidden: prose-only menus. Plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools are absent.

Intent ≠ approval at every gate. STOP on skip/cancel — no draft, no write.

## Gate 1 — Opt in

Ask whether to update the overview from this change.

Prompt: `Update {project_overview.path} from change {name}?`

|id|Action|
|-|-|
|`yes`|Continue to Gate 2|
|`no`|STOP — report skipped|

Do not draft or gather scope until the user selects `yes`.

## Gate 2 — Scope plan

From that change’s planning artifacts (`proposal`, `design`, specs intent), propose a scope plan the user can verify or change before any draft:

- Sections or topics to add, update, or remove (or explicit no overview change).
- One-line rationale per item when not obvious.
- Do not paste spec SHALL blocks or `openspec/changes/**` paths into the plan.

Prompt: `Use this overview update plan for {path}?` (show the plan in the message above the tool.)

|id|Action|
|-|-|
|`approve_plan`|Draft and preview using this plan only|
|`refine`|STOP — ask the user to reply with edits to the plan (add/remove/reword items); re-show Gate 2 after they answer|
|`cancel`|STOP — no draft, no write|

On `refine`, incorporate the user’s reply into a revised plan and run Gate 2 again.

Manual `/tacos-project-overview` skips Gate 1. Use Gate 2 when scope is vague; otherwise use the user’s stated scope as the plan.

## Gate 3 — Preview approval

After draft → preview diff (or “no change needed”), run [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#project-overview).
