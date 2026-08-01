---
name: tacos-project-overview
description: >-
  Updates openspec/tacos.yaml project_overview.path in-scope sections only;
  preview before write. Use after sync/archive hooks, /tacos-project-overview,
  planning Project overview checkbox, or overview refresh requests.
user-invocable: true
---

# tacos project overview

Updates `project_overview.path` from `openspec/tacos.yaml`. Procedure: [update-workflow.md](references/update-workflow.md).

Out of scope: `AGENTS.md`, rules, skill bodies, `openspec/specs/**`, `openspec/changes/**`.

## Manual scope (examples)

|User says|In scope|
|-|-|
|“Add a bullet under Features describing the new export command; leave everything else.”|Features section only|
|“Rewrite Configuration from current env vars; don’t touch Contributing.”|Configuration section|
|“Remove the outdated beta disclaimer in Installation.”|Installation — remove that disclaimer|

If scope is vague, ask once which parts of `{path}` to add, update, or remove — then [update-workflow.md](references/update-workflow.md) with trigger manual.

## Triggers

|Trigger|Detail|
|-|-|
|sync / archive|[project-overview-hooks.md](../tacos-orchestration/references/project-overview-hooks.md) + [sync-archive-prompts.md](references/sync-archive-prompts.md)|
|planning|Optional `Project overview:` checkbox — [task-stage-contract.md](../tacos-orchestration/references/task-stage-contract.md) ## Project overview line|
|manual|[manual-invocation.md](references/manual-invocation.md) → [update-workflow.md](references/update-workflow.md)|

`enabled: false` → sync/archive orchestration off; manual still OK.

## Approval

Intent ≠ approval. Preview and approval before write → [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#project-overview).

## Done when

- Manual: scope confirmed; preview shown; user approved per [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#project-overview); `{path}` updated in-scope or user cancelled after preview.
- Sync/archive: Gates 1–2 completed per [sync-archive-prompts.md](references/sync-archive-prompts.md) when opted in; write only after approval.
- Planning checkbox: full workflow runs when the line is checked off during implement — same preview + approve bar before write.

## Entry

[update-workflow.md](references/update-workflow.md) · [sync-archive-prompts.md](references/sync-archive-prompts.md) · [overview-guidance.md](references/overview-guidance.md)
