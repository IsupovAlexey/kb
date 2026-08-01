# tacos-work phase 2 — Work checklist

## MUST read

- [tasks-template.md](../tasks-template.md) — **Filling ## Work** and ## Spec touch decision
- When `project_overview.enabled`: [project-overview-reconcile.md](../project-overview-reconcile.md) and [../../../tacos-orchestration/references/task-stage-contract.md](../../../tacos-orchestration/references/task-stage-contract.md) ## Project overview line (optional)

## During phase

- 2.1 — `tacos-work [<slug>]: work — filling ## Work`
- 2.2 — Run [project-overview-reconcile.md](../project-overview-reconcile.md) ## Phase 2 — scope-vs-overview before implementation checkboxes
- 2.3 — Complete implementation checkboxes; MUST include `**Testable outcome:**` under `## Work`; record spec-touch decision in **Plan** and **Spec touch** rows per [tasks-template.md](../tasks-template.md) ## Spec touch decision; MUST NOT add standalone overview-path edit rows
- 2.4 — When reconciliation requires overview update: emit `Project overview:` line with scoped sections before **Tests**; when internal-only after reconcile: omit line
- 2.5 — Report path; implementation not started

## Gate

No implementation edits until 2.2–2.4 complete.

## Done when

- **## Work** filled with terminal Apply review / Human review lines
- Scope-vs-overview reconciliation recorded (line emitted or omitted with reconcile outcome)
- Next: [phase-2.5-confirm.md](phase-2.5-confirm.md)
