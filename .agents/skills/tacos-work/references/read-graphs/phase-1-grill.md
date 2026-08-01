# tacos-work phase 1 — Planning grill

When `openspec/tacos.yaml` has `orchestration.grill_enabled: true`. Skip → [phase-2-work.md](phase-2-work.md).

## MUST read

- [planning-grill.md](../planning-grill.md) — mandatory order gather → interview → summarize
- [../../../tacos-grill/references/read-graphs/planning.md](../../../tacos-grill/references/read-graphs/planning.md) — not full `tacos-grill/SKILL.md`
- [../../../tacos-grill/references/interview-prompt.md](../../../tacos-grill/references/interview-prompt.md) — grill mode offer, ## Never substitute for interview
- [../../../tacos-orchestration/references/grill-gates.md](../../../tacos-orchestration/references/grill-gates.md) — STOP before **Planning** prose
- [../../../tacos-orchestration/references/structured-gate-convention.md](../../../tacos-orchestration/references/structured-gate-convention.md)
- [../../../tacos-orchestration/references/runtime-delegation.md](../../../tacos-orchestration/references/runtime-delegation.md) — gather / summarize Task names

## STOP / forbidden (≤1 hop)

- No **Planning** prose until grill mode interview or user **skip** in structured menu
- No `grill_mode: skip` or `planning: complete` without parent interview — [planning-grill.md](../planning-grill.md) **Forbidden**
- Invalid Phase 0 artifact: delete Planning prose, reset `grill.planning: pending`, re-run Phase 1

## During phase

|Step|Action|
|-|-|
|1.1|`tacos-work [<slug>]: grill — starting planning`|
|1.2|Task `agent-tacos-grill-gather` — [planning-grill.md#gather-prompt](../planning-grill.md#gather-prompt)|
|1.3|Parent grill mode **first** — structured prompt before **Planning** prose|
|1.4|Parent interview or **skip**|
|1.5|Task `agent-tacos-grill-summarize` — [planning-grill.md#summarize-prompt](../planning-grill.md#summarize-prompt)|
|1.6|Update **Planning** + frontmatter in same `tasks.md`|

## Done when

- `grill.planning` is `complete` or `skipped` in session `tasks.md`
- Next: [phase-2-work.md](phase-2-work.md)
