---
name: tacos-grill
description: >-
  Runs tacos-grill interviews: planning and per-phase grilling before OpenSpec
  artifacts, mandatory stage apply, and triggered grilling during apply, sync, or
  explore. Use when orchestration loads this skill, grill.triggers.* signals fire,
  the user invokes /tacos-grill, or they refine a draft. Writes grill-summaries.md.
user-invocable: true
---

# tacos grill

Human-in-the-loop grilling: one topic at a time, recommended defaults, repo-aware challenges before asking the human. Implementation-quality lenses: [implementation-quality-lenses.md](references/implementation-quality-lenses.md) (**Applicability first**; record N/A when they do not apply).

**Delegation:** Interview on parent; gather and summarize via Task when supported — [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md).

Config: `openspec/tacos.yaml` (`orchestration.grill_enabled`, `grill.*`). Stage grill gate: [task-stage-contract.md](../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate; notation: [config-notation.md](../tacos-orchestration/references/config-notation.md).

## Entry

1. Resolve phase from caller (orchestration command, `tasks.md` stage, manual `/tacos-grill`, or triggered signal).
2. Load the active phase read graph (one hop):
   - planning (propose, ff) — [read-graphs/planning.md](references/read-graphs/planning.md)
   - update (revise existing artifacts) — [read-graphs/update.md](references/read-graphs/update.md)
   - proposal, specs, design, tasks (continue) — [read-graphs/per-phase.md](references/read-graphs/per-phase.md)
   - apply stage start — [read-graphs/stage-apply.md](references/read-graphs/stage-apply.md)
   - apply / sync / explore triggered — [read-graphs/triggered.md](references/read-graphs/triggered.md)
   - manual `/tacos-grill` — [read-graphs/manual.md](references/read-graphs/manual.md)
3. Follow graph MUST read + Before / During / After / Done when present.

STOP / forbidden contract: [interview-prompt.md](references/interview-prompt.md) ## Never substitute for interview; gates: [grill-gates.md](../tacos-orchestration/references/grill-gates.md); structured gates: [structured-gate-convention.md](../tacos-orchestration/references/structured-gate-convention.md).

## Done when

- Topic interview satisfied per [interview-prompt.md](references/interview-prompt.md) ## Interview minimum (full / short / defaults) or ## Interview minimum (assumptions) when `grill_mode: assumptions` — mode offer alone is insufficient except explicit **`skip`** / waive.
- `grill-summaries.md` has `grill_mode`, matching `grill.*` frontmatter, and **User inputs** for the grilled phase tracing to user replies from topic prompts (or documented skip/waiver in chat).
- Manual invoke: phase interview complete or user chose skip; planning artifacts unchanged unless the user explicitly requested writes.
