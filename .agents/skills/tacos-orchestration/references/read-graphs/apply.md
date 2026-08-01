# Apply read graph

Command id: apply (normalize host `opsx-apply` per [openspec-commands.md](../openspec-commands.md)).

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)
- [proactive-explore-delegation.md](../proactive-explore-delegation.md) + [explore-return-contract.md](../explore-return-contract.md) when Task supported
- [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md) when `orchestration.grill_enabled`
- [entry-conditionals.md](../entry-conditionals.md) rows for apply + staged apply + tdd
- [task-stage-contract.md](../task-stage-contract.md) § Per-stage checklist order
- [post-artifact-signoff.md](../post-artifact-signoff.md) when `orchestration.staged_apply_enabled` (Apply review step authority)

Schema `apply.instruction` is the apply **gate header** only (FIRST ACTION, FORBIDDEN, pointers). Canonical staged runtime is this graph MUST read + `task-stage-contract.md`.

## Before command

## During command

- Chat voice: tacos-config direct-output kernel; carve-outs [direct-output.md](../../../tacos-direct-output/references/direct-output.md) on demand
- Stage grill gate: STOP on unchecked `Stage grill:` before implementation — [grill-prompts/apply-mandatory.md](../../../tacos-grill/references/grill-prompts/apply-mandatory.md); topic interview per [interview-prompt.md](../../../tacos-grill/references/interview-prompt.md) ## Interview minimum (full / short / defaults) — mode offer alone does not complete the gate
- Triggered apply when `grill.triggers.apply_on_ambiguous_task` or `grill.triggers.apply_on_spec_drift` — [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md)
- **Within-stage parallel slices** — when independent task rows have disjoint file ownership, parent **SHOULD** pipeline implement → gate-runner → apply review per slice within one stage; default sequential when ownership overlaps or is unclear; cross-stage human review stays sequential — [task-stage-contract.md](../task-stage-contract.md) ## Within-stage parallel slices
- **Same-turn apply review** — when the active stage implementation checkboxes are complete through **Tests** (and **Project overview:** when present), parent MUST delegate **Apply review** in the same turn — forbidden: ending the turn with **Apply review:** still unchecked. Only **Human review:** is a required human pause per [post-artifact-signoff.md](../post-artifact-signoff.md).

## After command

- Staged apply: per-stage checklist, Apply review, Human review — [task-stage-contract.md](../task-stage-contract.md), [review-gate-pass.md](../review-gate-pass.md)
- Main-spec drift prompt when apply-review reports drift — [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Staged apply

## Done when

- Each stage `Human review:` checked or waived; latest apply-review Summary is APPROVE + Ready — [review-gate-pass.md](../review-gate-pass.md)

## On demand

- Active `tasks.md` `## N` stage section + `grill-summaries.md` `## apply` ### Stage N when stage grill runs
- [orchestration-binding-index.md](../orchestration-binding-index.md) § Apply — apply read order + stage contract pointers
- [binding-sections/stock-overrides-matrix.md](../binding-sections/stock-overrides-matrix.md) — full matrix on demand (not the hub router)
- [post-artifact-signoff.md](../post-artifact-signoff.md) — at each `Apply review:` line
