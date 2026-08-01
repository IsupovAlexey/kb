# Work session file (`tasks.md`)

One file per session: `artifacts/tacos-work/<slug>/tasks.md` — **Intent**, **Planning** (grill), and **## Work** (checkboxes). No separate `intent.md` or `grill-summaries.md`.

## Create (Phase 0)

Write the file with **Intent** filled from invoke text: **Ask** verbatim; Phase 0 MAY draft **In scope** from invoke; **Out of scope** and **Complexity note** MAY stay `(pending)` until Phase 1 grill or Phase 2.5 playback.

When `orchestration.grill_enabled` is true:

- Frontmatter: `grill_mode: null`, `grill.planning: pending` — **never** `skip` / `complete` in Phase 0.
- **Planning** subsections: leave empty or `(pending grill)` only — **no** Summary/Decisions/User inputs prose from Intent.
- **Forbidden:** one-shot file with filled Planning; `grill_mode: skip` because the ask is clear or README-only.

When `orchestration.grill_enabled` is false: fill **Planning** and **## Work** from Intent; set `grill.planning: skipped`.

## After grill (Phase 1)

Update **Planning** and frontmatter (`grill_mode`, `grill.planning`) in the **same** file. Fill **## Work** checkboxes from grill **User inputs** (see below).

## Gate

No repo edits outside `artifacts/tacos-work/<slug>/` until `tasks.md` has **## Work** with Apply review and Human review lines — [session-runbook.md](session-runbook.md) **Gates**. When `orchestration.grill_enabled`, complete [planning-grill.md](planning-grill.md) before implementation. After **## Work** is filled, run Phase 2.5 execute confirm before Phase 3.

## Template

Replace `<slug>`. One **## Work** section only (single stage — no stage counter).

```markdown
---
grill_mode: null
grill:
  planning: pending
---

# tacos-work session

## Intent

### Ask

<verbatim change description from invoke>

### In scope

<!-- Phase 0 MAY draft from invoke; Phase 1 grill refines -->

### Out of scope

<!-- Phase 1 grill or Phase 2.5 playback -->

### Complexity note

<!-- Phase 1 grill or Phase 2.5 playback -->

## Planning

### Summary

### Decisions

### Open questions

### User inputs

## Work

**Testable outcome:** <!-- One sentence: how you will know this session succeeded -->

- [ ] Plan: goal, acceptance, target paths, spec-touch (or N/A)
- [ ] Spec touch: edit listed `openspec/specs/**` (or N/A)
- [ ] <implementation task 1>
- [ ] <implementation task 2>
- [ ] Verify Decision 1: <done-when — test name, trace step, or command>
- [ ] Verify Decision 2: <done-when — or N/A — reason when not testable>
- [ ] Project overview: update {project_overview.path} — <sections when scope-vs-overview reconciliation requires; omit when internal-only after reconcile — see project-overview-reconcile.md>
- [ ] Tests: <command or N/A — reason>
- [ ] Apply review: parallel Task — `agent-tacos-apply-review` + `agent-tacos-additional-apply-review` per applicable path (parent merge; core-only when zero applicable); write `artifacts/openspec-reviews/<slug>/apply-review.md`
- [ ] Human review: Pause for human sign-off before reporting Work workflow complete
```

### TDD rows (when TDD in invoke or **Intent**)

Replace implement-first implementation rows with Red → Green → Refactor per behavioral slice. One slice ≈ one acceptance unit from **Planning** **User inputs**. Docs-only work keeps a single implementation row plus `Tests: N/A — <reason>`.

```markdown
## Work

**Testable outcome:** <!-- One sentence: how you will know this session succeeded -->

- [ ] Plan: goal, acceptance, target paths, spec-touch (or N/A)
- [ ] Spec touch: edit listed `openspec/specs/**` (or N/A)
- [ ] Red: <behavior> — write/run test, paste failure in chat
- [ ] Green: minimal implementation until test passes
- [ ] Refactor: cleanup without behavior change
- [ ] Verify Decision 1: <done-when — test name, trace step, or command>
- [ ] Verify Decision 2: <done-when — or N/A — reason when not testable>
- [ ] Project overview: update {project_overview.path} — <sections when scope-vs-overview reconciliation requires; omit when internal-only after reconcile — see project-overview-reconcile.md>
- [ ] Tests: no additional suite beyond Red/Green (or host command for extra checks)
- [ ] Apply review: …
- [ ] Human review: Pause for human sign-off before reporting Work workflow complete
```

**Red done-when:** failing test output pasted in chat. **Green done-when:** test passes and Red satisfied. Load [tacos-tdd/SKILL.md](../../tacos-tdd/SKILL.md) during Phase 2 when TDD intent is set.

**Planning completion:** When `orchestration.grill_enabled` is **false**, set `grill.planning: skipped` in Phase 0 (**Create** above). When grill is **true** (Phase 1 only): set `complete` or `skipped` only after [planning-grill.md](planning-grill.md) (parent grill mode via structured prompt per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md); plain-text only when tools absent, then interview or user-chosen **skip** in chat). `grill_mode: skip` is valid only when the user selected **skip** in that menu — not because Intent was detailed. Non-empty **User inputs** when `complete` (unless skip). Copied Intent text is **not** grill **User inputs**.

## Filling ## Work

- **Testable outcome** — One plain sentence under `## Work`; verifiable done-ness for the session (same pattern as staged apply `tasks` schema)
- **Plan** — Goal, done-ness, paths, spec-touch from **Intent** + **Planning** **User inputs**
- **Spec touch** — N/A or `openspec/specs/**` paths; decision rubric at [## Spec touch decision](#spec-touch-decision)
- **Implementation** — 1–5 focused checkboxes from grill; when TDD in invoke/Intent → Red/Green/Refactor triple per behavioral slice
- **Verify Decision N** — One row per material **Planning → Decisions** entry when **Decisions** has 2+ material items or any decision affects control flow, failure paths, or scope boundaries; done-when MUST be testable (named test) or traceable (hot-path step). FORBIDDEN: embedding `Verify Decision N` / `— **Verify Decision N**` inside implementation checkbox text — cite Decision N for traceability only; verification is its own row. Use `Verify Decision N: N/A — <reason>` only when genuinely not verifiable in this session
- **Project overview** — When `project_overview.enabled`: run [project-overview-reconcile.md](project-overview-reconcile.md) during Phase 2 before implementation rows; emit checkbox with scoped sections when reconciliation requires; omit when internal-only after reconcile; MUST NOT add standalone `Update {project_overview.path}` implementation rows
- **Tests** — Host command or `Tests: N/A — <reason>`; TDD behavioral slices use `no additional suite beyond Red/Green`. Do not check off **Tests** or **Apply review** until all **Verify** rows are `[x]` or waived in chat

**Verification trigger:** pipeline ordering, gate-before-I/O, negative/error paths, and scope boundaries each need a **Verify** row with concrete done-when — not only narrative under **Decisions**.

## Spec touch decision

Record the decision in **Planning → User inputs** and the **Plan** row before checking **Spec touch**. Phase 3 MUST NOT edit `openspec/specs/**` unless **Spec touch** lists paths.

- Behavioral — durable SHALL/MUST or invariant scenario under `openspec/specs/**` changes → list paths in **Plan** and **Spec touch**; edit those main specs in Phase 3 before implementation rows that depend on them
- Procedure-only — skill runbook, checklist order, phase routing, or template wording with unchanged normative obligations in main specs → **Spec touch: N/A — \<reason\>** (one line); no `openspec/specs/**` edits in Phase 3

When uncertain, treat as behavioral and list implicated capability specs. Dedrift after apply-review is a safety net, not a substitute for upfront **Spec touch**.

## Apply-review inputs

- Session diff
- This `tasks.md` (Intent, Planning, Work)
- Touched `openspec/specs/**`

## Resume

Add checkboxes only under the same **## Work**. Multi-outcome scope → [escape-hatches.md](escape-hatches.md).
