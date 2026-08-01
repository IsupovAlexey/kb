# Grill stage-apply read graph

Phase: **apply** — mandatory stage start at each `tasks.md` `## N` boundary when stage grill gate is true.

No gather step. Parent interview only.

## MUST read

- [grill-prompts/apply-mandatory.md](../grill-prompts/apply-mandatory.md)
- [interview-prompt.md](../interview-prompt.md) ## Mandatory stage apply
- [task-stage-contract.md](../../../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate

## Before command

- STOP on unchecked `Stage grill:` — no implementation checkboxes in that stage until complete or skip/waiver
- Grill mode offer once per stage — [interview-prompt.md](../interview-prompt.md)

## During command

- Scope: this stage title + unchecked implementation tasks + relevant planning context
- Topic interview per [interview-prompt.md](../interview-prompt.md) ## Interview minimum (full / short / defaults) — mode offer alone is insufficient
- Full-resync planning when stage grill changes requirements — [task-stage-contract.md](../../../tacos-orchestration/references/task-stage-contract.md)

## After command

- Append **User inputs** under `## apply` in `grill-summaries.md` (trace to user replies from topic prompts — not planning-artifact summaries)
- Check off `Stage grill:` only after topic interview or documented skip/waiver

## Done when

- Interview minimum satisfied (or explicit skip/waiver); stage grill checkbox may be checked; triggered apply still runs per [triggered.md](triggered.md) before each implementation checkbox
