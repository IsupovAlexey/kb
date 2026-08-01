# tacos-work read graph

Entry: `/tacos-work` ([session-runbook.md](../../../tacos-work/references/session-runbook.md)).

Does not load full orchestration Entry or POST-ARTIFACT unless the session explicitly touches OpenSpec workflow commands.

## MUST read (session start)

Per active phase in [tacos-work/SKILL.md](../../../tacos-work/SKILL.md) Entry — one phase graph only:

|Phase|Graph|
|-|-|
|0|[phase-0-session.md](../../../tacos-work/references/read-graphs/phase-0-session.md)|
|1 (grill)|[phase-1-grill.md](../../../tacos-work/references/read-graphs/phase-1-grill.md)|
|2–5|Matching `phase-*.md` under `tacos-work/references/read-graphs/`|
|Archive|[archive-mode.md](../../../tacos-work/references/read-graphs/archive-mode.md)|

Phase 0 subset:

|Order|Reference|When|
|-|-|-|
|1|`openspec/tacos.yaml`|`orchestration.grill_enabled`, `grill.*`, `review.*`, `project_overview.*`|
|2|[runtime-delegation.md](../runtime-delegation.md)|Task names — rows below|

When grill will run (phase 1): [planning-grill.md](../../../tacos-work/references/planning-grill.md) + [read-graphs/planning.md](../../../tacos-grill/references/read-graphs/planning.md) — not full `tacos-grill/SKILL.md`.

## Delegation matrix rows (work path)

|Step|Agent `name:`|When|
|-|-|-|
|Planning gather|`tacos-grill-gather`|Phase 1 when `orchestration.grill_enabled`|
|Planning summarize|`agent-tacos-grill-summarize`|After parent interview|
|Apply review|`tacos-apply-review`|Phase 4 — **same turn** as Phase 3 completion; parent MUST NOT end turn with apply-review unchecked|

When `review.apply_review_additional_skills` is non-empty: evaluate applicability per [post-artifact-signoff.md](../post-artifact-signoff.md) **Apply review — parallel launch** step 1, then parallel **`tacos-additional-apply-review`** per **applicable** path (record skips under **`## Skipped additional skills`** in parent merge).

## Optional (Intent / ## Work)

- [project-overview-reconcile.md](../../../tacos-work/references/project-overview-reconcile.md) — when `project_overview.enabled`
- [task-stage-contract.md](../task-stage-contract.md) ## Project overview line — when `project_overview.enabled`
- [tacos-tdd/SKILL.md](../../../tacos-tdd/SKILL.md) — when TDD in invoke or **Intent**

## Hub pointer

Normative overrides: [orchestration-binding.md](../orchestration-binding.md). Command-scoped diet indexes: [explore.md](explore.md), [propose.md](propose.md).
