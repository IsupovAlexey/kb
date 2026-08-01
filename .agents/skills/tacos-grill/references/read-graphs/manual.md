# Grill manual read graph

Phase: user-invoked `/tacos-grill` or draft refinement.

## MUST read

- [interview-prompt.md](../interview-prompt.md) — grill mode, change/phase disambiguation
- [grill-prompts.md](../grill-prompts.md) router table — phase bundle for resolved phase
- [grill-gates.md](../../../tacos-orchestration/references/grill-gates.md) when planning path
- [grill-summaries template](../../../tacos-doctor/schemas/tacos/templates/grill-summaries.md)

## Before command

- Resolve change and phase; default phase from `openspec status` when one artifact is `ready`
- Re-grill → light grill when `## <phase>` exists — [interview-prompt.md](../interview-prompt.md) ## Light grill

## During command

- Gather Task when supported for pre-artifact phases; skip gather for apply/sync/explore triggered-style runs
- Do not write planning artifacts unless user explicitly requests

## After command

- Parent persists `grill-summaries.md`; summarize Task when supported

## Done when

- Phase interview complete or user chose skip; planning artifacts unchanged unless user requested writes
