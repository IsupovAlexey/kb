---
name: tacos-work
description: Bounded feature session under artifacts/tacos-work/ — planning grill when orchestration.grill_enabled, implement, apply-review; optional /tacos-work archive for thin session.md under openspec/changes/archive/. Invoke via /tacos-work; not multi-stage OpenSpec apply.
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Freeform change description (e.g. add CSV export), or `archive` / `archive <slug>` to persist a completed session to openspec/changes/archive/
---

# tacos work

Bounded feature session: one file `artifacts/tacos-work/<slug>/tasks.md`. When `orchestration.grill_enabled`, Phase 0 writes **Intent** only — **Planning** stays empty until Phase 1 grill (never `grill_mode: skip` without user **skip** in structured grill menu).

**Pipeline:** session file → planning grill → work checklist → execute confirm → implement → apply-review → (minor polish when Optional MINOR open) → human sign-off

## Entry

1. Resolve active phase from session state (new invoke → phase 0; resume from `tasks.md` frontmatter / checklist progress; `/tacos-work archive` → [read-graphs/archive-mode.md](references/read-graphs/archive-mode.md)).
2. Load **one** phase read graph (one hop):
   - 0 — [read-graphs/phase-0-session.md](references/read-graphs/phase-0-session.md)
   - 1 — [read-graphs/phase-1-grill.md](references/read-graphs/phase-1-grill.md) when `orchestration.grill_enabled`
   - 2 — [read-graphs/phase-2-work.md](references/read-graphs/phase-2-work.md)
   - 2.5 — [read-graphs/phase-2.5-confirm.md](references/read-graphs/phase-2.5-confirm.md)
   - 3 — [read-graphs/phase-3-implement.md](references/read-graphs/phase-3-implement.md)
   - 4 — [read-graphs/phase-4-review.md](references/read-graphs/phase-4-review.md)
   - 5 — [read-graphs/phase-5-done.md](references/read-graphs/phase-5-done.md)
   - Archive — [read-graphs/archive-mode.md](references/read-graphs/archive-mode.md)
3. Follow graph MUST read + During / Done when present. Index: [session-runbook.md](references/session-runbook.md).

## Gates

Preview → structured prompt per [structured-gate-convention](../tacos-orchestration/references/structured-gate-convention.md) (**next tool call MUST** be `AskQuestion` / `AskUserQuestion` when available) → else plain text + **end turn** per [interview-prompt](../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable.

|Phase|Stop before…|Option ids / binding|
|-|-|-|
|1 Grill|**Planning** prose|`full` / `short` / `defaults` / `assumptions` / `skip` — [planning-grill](references/planning-grill.md)|
|2.5 Confirm|Phase 3 implementation edits|`proceed` / `edit` / `cancel` — [phase-2.5-confirm](references/read-graphs/phase-2.5-confirm.md)|
|4 Review|Polish gate / human gate|Task `agent-tacos-apply-review` (+ parallel `agent-tacos-additional-apply-review` per **applicable** path when yaml non-empty; applicability before spawn; parent merge); then [minor-polish-gate](references/minor-polish-gate.md) when MINOR rows remain|
|5 Done|Session complete|Pause or waive|

**Implementation edit** — any change outside `artifacts/tacos-work/<slug>/` except `tasks.md` bootstrap (Intent, Planning, **## Work** structure before implementation).

## Quick start

|User says|Action|
|-|-|
|`/tacos-work <description>`|Entry phase 0 → advance per phase graphs|
|`/tacos-work` (no text)|Ask once, then phase 0|
|`/tacos-work archive`|Archive mode — resolve slug from context|
|`/tacos-work archive <slug>`|Archive mode — named session|

Scope too large → [escape-hatches](references/escape-hatches.md).

## Done when

- **Session file:** `tasks.md` with **Intent** (**Ask**, **In scope**, **Out of scope**, **Complexity note**), **Planning** (when grill ran), **## Work** + `**Testable outcome:**` per [tasks-template](references/tasks-template.md)
- **Execute confirm:** **Proceed** before Phase 3 implementation edits
- **Implement:** **## Work** checkboxes complete; **Project overview:** when present
- **Spec touch:** listed `openspec/specs/**` paths updated in git when behavioral obligations changed, or **Spec touch** marked N/A with reason in `tasks.md` when procedure-only per [tasks-template](references/tasks-template.md) ## Spec touch decision
- **Apply review:** latest `artifacts/openspec-reviews/<slug>/apply-review.md` passes [review-gate-pass](../tacos-orchestration/references/review-gate-pass.md); [minor-polish-gate](references/minor-polish-gate.md) complete when **Optional (MINOR)** had open rows; drift resolved per [orchestration-dedrift-pass](../tacos-dedrift/references/orchestration-dedrift-pass.md) § tacos-work
- **Human gate:** sign-off or waive; `tacos-work [<slug>]: done`

## When to use

|Situation|Path|
|-|-|
|Bounded feature, one review pass|tacos-work|
|Multi-stage apply, change-folder deltas|Full tacos (`/opsx-propose`)|
|Quick tweak, no session file|Edit directly — not `/tacos-work`|

## References

[session-runbook](references/session-runbook.md) · [tasks-template](references/tasks-template.md) · [minor-polish-gate](references/minor-polish-gate.md) · [planning-grill](references/planning-grill.md) · [project-overview-reconcile](references/project-overview-reconcile.md) · [output-paths](references/output-paths.md) · [slug-resolution](references/slug-resolution.md) · [archive-session-template](references/archive-session-template.md) · [archive-script](references/archive-script.md) · [escape-hatches](references/escape-hatches.md)
