# Grill update read graph

Phase: **update** — revise existing planning artifacts in place (**update** command only; not greenfield `grill.planning`).

Load from `tacos-grill/SKILL.md` Entry when orchestration starts **update** with `orchestration.grill_enabled` and `grill.update` is `pending`.

## MUST read

- [grill-prompts/update.md](../grill-prompts/update.md)
- [interview-prompt.md](../interview-prompt.md) — grill mode offer, ## Never substitute for interview
- [grill-gates.md](../../../tacos-orchestration/references/grill-gates.md) § Update grill — STOP before reconcile writes
- [implementation-quality-lenses.md](../implementation-quality-lenses.md) — Applicability first

## Before command

- STOP if `grill.update` pending — no reconcile writes until update grill completes or skip
- Gather Task `agent-tacos-grill-gather` when supported — [runtime-delegation.md](../../../tacos-orchestration/references/runtime-delegation.md)
- Parent interview only — no summarize Task before grill mode chosen

## During command

- One topic per structured prompt — [interview-prompt.md](../interview-prompt.md)
- Follow [grill-prompts/update.md](../grill-prompts/update.md) stance: revision mode, anchor edit, bidirectional ripple, minimal delta, redirect, apply state — not greenfield planning topics
- Lenses per [implementation-quality-lenses.md](../implementation-quality-lenses.md) only when the **revision** introduces new behavioral obligations; record N/A otherwise

## After command

- Summarize Task when supported; append **User inputs** under `## update` in `grill-summaries.md` (or change-session frontmatter when no grill-summaries file)
- Set `grill.update` to `complete` or `skipped` in change frontmatter
- Orchestration may proceed to stock reconcile per [update.md](../../../tacos-orchestration/references/read-graphs/update.md)

## Done when

- `grill.update` complete or skipped; non-empty **User inputs** for `complete` (unless skip)

## On demand

- Full `tacos-grill/SKILL.md` — manual `/tacos-grill` with phase **update** only
