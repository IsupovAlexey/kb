# Grill planning read graph

Phase: **planning** — propose and ff only (one session before any planning artifact).

Load from `tacos-grill/SKILL.md` Entry when orchestration starts propose or ff with `orchestration.grill_enabled`.

## MUST read

- [grill-prompts/planning.md](../grill-prompts/planning.md)
- [interview-prompt.md](../interview-prompt.md) — grill mode offer, ## When to recommend grill mode, ## Never substitute for interview
- [grill-gates.md](../../../tacos-orchestration/references/grill-gates.md) — STOP / forbidden before artifacts
- [implementation-quality-lenses.md](../implementation-quality-lenses.md) — Applicability first
- [grill-summaries template](../../../tacos-doctor/schemas/tacos/templates/grill-summaries.md)

## Before command

- STOP if `grill.planning` pending — no planning artifact writes until planning grill completes or skip
- Gather Task `agent-tacos-grill-gather` when supported — [runtime-delegation.md](../../../tacos-orchestration/references/runtime-delegation.md)
- Parent interview only — no summarize Task before grill mode chosen

## During command

- One topic per structured prompt — [interview-prompt.md](../interview-prompt.md)
- Lenses per [implementation-quality-lenses.md](../implementation-quality-lenses.md); record N/A when not applicable

## After command

- Summarize Task when supported; parent writes `grill-summaries.md` ## proposal (and related phases when re-run)
- Orchestration may proceed to planning artifacts per [planning-artifact-loop.md](../../../tacos-orchestration/references/planning-artifact-loop.md)

## Done when

- `grill.planning` complete or skipped in frontmatter; **User inputs** recorded under grilled phase(s)

## On demand

- Full `tacos-grill/SKILL.md` — manual `/tacos-grill` or phase disambiguation only
- [grill-prompts/per-phase.md](../grill-prompts/per-phase.md) when continue runs per-phase grill after planning
