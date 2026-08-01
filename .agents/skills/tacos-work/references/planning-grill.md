# Planning grill (tacos-work)

When `openspec/tacos.yaml` has `orchestration.grill_enabled: true`. Gates: [grill-gates.md](../../tacos-orchestration/references/grill-gates.md) §1–3.

**Invocation** matches [planning-artifact-loop.md](../../tacos-orchestration/references/planning-artifact-loop.md) and [read-graphs/planning.md](../../tacos-grill/references/read-graphs/planning.md). **Persist:** update **## Planning** and frontmatter in `artifacts/tacos-work/<slug>/tasks.md` — not separate grill files or `openspec/changes/`.

## Prerequisites

- `artifacts/tacos-work/<slug>/tasks.md` exists with **## Intent** filled.
- Parent read [read-graphs/planning.md](../../tacos-grill/references/read-graphs/planning.md) and [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md) — not full `tacos-grill/SKILL.md`.
- When `project_overview.enabled` is true: parent read [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Project overview line (optional) and [project-overview-reconcile.md](project-overview-reconcile.md) before Phase 2 **## Work** fill.

## Run

**Order is mandatory** — same as full tacos planning ([grill-gates.md](../../tacos-orchestration/references/grill-gates.md) §1).

1. **Gather** — Task **`agent-tacos-grill-gather`**, phase **`planning`**.
2. **Interview** — Parent grill mode **before** any **Planning** subsection has session prose (structured prompt per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md); plain-text only when tools absent). One topic per turn for `full` / `short` / `defaults` / `assumptions`. Label **at most one** "(Recommended)" per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## When to recommend grill mode (default: neutral offer). On **`assumptions`**, follow [grill-prompts/planning.md](../../tacos-grill/references/grill-prompts/planning.md) ## Assumptions grill mode before summarize.
3. **Summarize** — Task **`agent-tacos-grill-summarize`** only after step 2 or user chose **skip** in step 2's menu.
4. **Persist** — Replace empty **Planning** stubs with summarize output; set frontmatter (`grill_mode`, `grill.planning: complete` or `skipped`).

**Forbidden:** `grill_mode: skip` or `planning: complete` without step 2 menu; filling **Planning** from **Intent** in Phase 0; paraphrasing Intent into **User inputs** without interview; complete/skip without user input when structured tool unavailable ([read-graphs/planning.md](../../tacos-grill/references/read-graphs/planning.md) STOP/forbidden; fallback per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable).

When `orchestration.grill_enabled` is false: no grill steps — Phase 0 already fills **Planning** and **## Work** from **Intent** with `grill.planning: skipped` ([tasks-template.md](tasks-template.md) **Create**).

## Before implementation

- `grill.planning` is `complete` or `skipped` when grill was required.
- **## Work** has `**Testable outcome:**`, Apply review and Human review lines; when `project_overview.enabled`, **Project overview:** line per scope-vs-overview reconciliation per [project-overview-reconcile.md](project-overview-reconcile.md) and [read-graphs/phase-2-work.md](read-graphs/phase-2-work.md) step 2.4.
- Phase 2.5 execute confirm passed per [session-runbook.md](session-runbook.md).

## Gather prompt

```text
tacos-work planning gather.

Phase: planning. Session slug: <slug>
Session file: artifacts/tacos-work/<slug>/tasks.md (read ## Intent only)

Return question script from `grill-prompts/planning.md`. Do not interview or write files.
```

## Summarize prompt

```text
tacos-work planning summarize.

Phase: planning. Session slug: <slug>. Grill mode: <mode>
Interview notes: <bullets>

Output for ## Planning only (Summary, Decisions, Open questions, User inputs).
Parent persists into tasks.md ## Planning + frontmatter. Template: tasks-template.md
Do not write openspec/changes/ or separate grill-summaries.md.

Decisions that affect pipeline ordering, gates before I/O, negative/error paths, or scope boundaries MUST include verifiable done-when phrasing (named test, trace step, or command) — not narrative-only. Parent fills matching **Verify Decision N** rows in ## Work per tasks-template.md.

When `project_overview.enabled` is true, record in **User inputs** either (a) `internal-only`, or (b) target sections at `project_overview.path` for Phase 2 — no vague third state. When unsure whether work belongs in overview sections, parent MUST ask in step 2 interview before summarize. Phase 2 scope-vs-overview may override `internal-only` per [project-overview-reconcile.md](project-overview-reconcile.md).
```
