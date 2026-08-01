# tacos-work phase 3 — Implement

## MUST read

- Session `artifacts/tacos-work/<slug>/tasks.md` **## Work**
- [../tasks-template.md](../tasks-template.md) ## Spec touch decision — MUST NOT edit `openspec/specs/**` unless **Spec touch** lists paths
- [../../../tacos-orchestration/references/proactive-explore-delegation.md](../../../tacos-orchestration/references/proactive-explore-delegation.md) — discovery delegation before parent Grep/Read
- [../../../tacos-orchestration/references/runtime-delegation.md](../../../tacos-orchestration/references/runtime-delegation.md) — **`agent-tacos-apply-implement`** for implementation rows
- When **Project overview:** line present: [../../../tacos-project-overview/SKILL.md](../../../tacos-project-overview/SKILL.md) Entry — preview + approval before write
- When TDD in **Intent**: [../../../tacos-tdd/references/red-green-refactor.md](../../../tacos-tdd/references/red-green-refactor.md)
- When signals match: [../../../tacos-grill/references/triggered-grill.md](../../../tacos-grill/references/triggered-grill.md)

## During phase

|Step|Action|
|-|-|
|3.1|`tacos-work [<slug>]: implement`|
|3.2|When implementation paths are not named in **## Work** rows and Task is supported: launch read-only **`explore`** (or host equivalent) per [proactive-explore-delegation.md](../../../tacos-orchestration/references/proactive-explore-delegation.md) **before** parent Grep/Read — merge discovery bullets; inline discovery **forbidden** when Task is supported|
|3.3|When Task is supported: delegate implementation checkboxes to **`agent-tacos-apply-implement`** per [runtime-delegation.md](../../../tacos-orchestration/references/runtime-delegation.md) **Apply implement** example; parent merges worktree after implement summary when child used isolated worktree; inline implementation only when Task unavailable (note in turn summary)|
|3.4|Check off **## Work** in order: Plan → Spec touch → implementation → **Verify Decision N** → **Project overview** (when line present) → Tests|
|3.5|Triggered grill when signals match|

## Gate

Do not check off **Tests** or open Apply review until every **Verify Decision N** is `[x]` or waived and **Project overview:** is `[x]` or omitted.

## Done when

- **## Work** implementation checkboxes complete (through **Tests**)
- Parent **MUST** continue in the **same turn** — load [phase-4-review.md](phase-4-review.md) and delegate apply-review without waiting for user input
