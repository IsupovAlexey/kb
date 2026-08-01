# tacos-work phase 0 — Session file

Load from `tacos-work/SKILL.md` Entry on new invoke or when `tasks.md` does not exist.

## MUST read

- [tasks-template.md](../tasks-template.md)
- [slug-resolution.md](../slug-resolution.md)
- `openspec/tacos.yaml` — `orchestration.grill_enabled`, `grill.*`, `review.*`, `project_overview.*`

## During phase

|Step|Action|
|-|-|
|0.1|Change description from invoke or ask once|
|0.1b|When description mentions archive intent (not `/tacos-work archive`): non-binding expectation only — no git write until archive mode after done|
|0.2|`tacos-work [<slug>]: session — writing tasks.md`|
|0.3|Write `tasks.md` — first repo write. When `orchestration.grill_enabled` is true: **Intent** only + empty **Planning** + **## Work** skeleton; `grill.planning: pending`. When false: fill **Intent**, **Planning**, and **## Work** from invoke; `grill.planning: skipped` — then skip Phase 1|

## Done when

- `artifacts/tacos-work/<slug>/tasks.md` exists per step 0.3
- Next: Phase 1 when `orchestration.grill_enabled`; else Phase 2 — [phase-2-work.md](phase-2-work.md)
