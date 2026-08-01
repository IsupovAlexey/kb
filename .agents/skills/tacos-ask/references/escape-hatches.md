# Escape hatches

Host OpenSpec extension command aliases (`/opsx-*`) map to tacos workflow commands — see [openspec-commands.md](../../tacos-orchestration/references/openspec-commands.md).

Route to a sibling when the user needs more than Q&A and small edits (see [small-edits](small-edits.md)).

|User intent|Slash / path|When|
|-|-|-|
|Bounded feature with planning grill + apply-review|`/tacos-work`|Single outcome but needs work session pipeline|
|Multi-stage or change-folder implementation|`/opsx-propose` or `/opsx-apply`|Delta specs, staged apply, orchestration gates|
|Continue planning artifacts|`/opsx-continue`|Next artifact in an active change|
|Planning grill or pre-artifact interview|`/opsx-propose` or `/opsx-continue`|Orchestration grill gates on an active change|
|Planning spec review on a change|`/tacos-spec-review`|POST-ARTIFACT-style review outside ask|
|Apply / implementation review on a change|`/tacos-apply-review` or `/opsx-apply`|Staged apply review or manual apply-review pass|
|Main-spec drift reconcile or conform|`/tacos-dedrift`|Behavioral drift between main specs and code|
|Jira regenerate or push for a change|`/tacos-jira-sync`|When `jira.enabled` and user wants Jira sync|
|Open or refresh a pull request description|`/tacos-pr`|PR create/sync outside implementation|
|Triage checks and review comments on author's PR|`/tacos-pr-triage`|Author-owned PR loop|
|Compact session for next chat|`/tacos-handoff`|Continuity file under `artifacts/session-handoff/`|
|PR or diff walkthrough|`/tacos-assisted-review`|Review-oriented diff narrative|
|Repo health scan or structured audit|`/tacos-audit`|Codebase audit or health review with vetted findings — not `/opsx-explore`|
|Open options or tradeoffs|`explore` or `/opsx-explore`|Not artifact-bound Q&A|

When orchestration would delegate via Task (grill gather/summarize, spec review, apply review, e2e scenarios, spec grounding), `tacos-ask` does not spawn those subagents — route to the slash path above and say ask cannot run POST-ARTIFACT or apply gates.

Tell the user why `tacos-ask` stops and which sibling fits. Small edits that pass [small-edits](small-edits.md) stay in ask — do not route those to `tacos-work` unless the user prefers a full work session.
