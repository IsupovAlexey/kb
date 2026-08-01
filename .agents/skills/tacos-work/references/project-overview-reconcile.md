# Project overview reconciliation (tacos-work)

Host-generic procedure when `project_overview.enabled` is true. Surface criteria: [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Project overview line (optional) — link only; do not duplicate slash-command or host-config lists in tacos-work prose.

## Phase 1 grill — surface decision

When scope is unclear after reading **## Intent**, parent MUST ask once (structured prompt when available):

- Does shipped work belong in sections of `{project_overview.path}`?
- Which sections (if yes)?

**User inputs** MUST record exactly one of:

- `internal-only` — no **Project overview:** line in Phase 2
- Target sections at `{project_overview.path}` — e.g. `README.md — Workflow entry, Skills table`

Forbidden: vague third state ("maybe README later").

## Phase 2 — scope-vs-overview

Before finalizing **## Work** implementation rows:

|Step|Action|
|-|-|
|R.1|Read `{project_overview.path}` on disk|
|R.2|Compare **Intent**, **Planning**, and draft implementation scope to existing overview sections|
|R.3|Apply task-stage-contract user-visible surface criteria (by reference)|
|R.4|When reconciliation says yes → emit `- [ ] Project overview: update {project_overview.path} — <sections>` before **Tests**|
|R.5|When reconciliation says no → omit **Project overview:** line|
|R.6|Reconciliation **overrides** **Planning** `internal-only` when comparison says yes|

## Forbidden implementation rows

Do not emit standalone overview edit rows such as:

- `Update README.md`
- `Update {project_overview.path}`

Overview edits MUST use only the **Project overview:** checkbox and Phase 3 `tacos-project-overview` workflow (preview + approval before write).

## Phase 3

When **Project overview:** line is present, [phase-3-implement.md](read-graphs/phase-3-implement.md) MUST load `tacos-project-overview` Entry before checkoff.

## Phase 5 backstop

When **Project overview:** was required (line present) and remains unchecked at done, [phase-5-done.md](read-graphs/phase-5-done.md) warn-nudge applies.
