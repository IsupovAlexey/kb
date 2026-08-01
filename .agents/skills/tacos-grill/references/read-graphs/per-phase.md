# Grill per-phase read graph

Phases: **proposal**, **specs**, **design**, **tasks** — continue path when `grill.<phase>` is pending.

## MUST read

- [grill-prompts/per-phase.md](../grill-prompts/per-phase.md) — section matching active phase (`## proposal`, `## specs`, …)
- [interview-prompt.md](../interview-prompt.md) — grill mode, one topic per call
- [artifact-prose.md](../../../tacos-orchestration/references/artifact-prose.md) before artifact write (orchestration loads from propose graph)
- [grill-summaries template](../../../tacos-doctor/schemas/tacos/templates/grill-summaries.md)

## Before command

- Gather uses `openspec instructions <artifact-id> --json` plus matching section in [grill-prompts/per-phase.md](../grill-prompts/per-phase.md)
- Gather uncertainty sizing baseline — [grill-prompts/planning.md](../grill-prompts/planning.md) ## Gather uncertainty sizing (raise/shrink vs `grill.default_max_questions`; no pad-to-N)
- STOP if `grill.<phase>` pending — write only the target artifact after grill

## During command

- Parent interview per [interview-prompt.md](../interview-prompt.md); do not set `grill.planning` complete from per-phase alone

## After command

- Summarize updates `## <phase>` only in `grill-summaries.md`
- Orchestration writes the single ready artifact

## Done when

- `grill.<phase>` complete or skipped; **User inputs** under `## <phase>`
