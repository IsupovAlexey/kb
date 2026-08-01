# tacos orchestration binding (full contract)

Binding sources (in order): `openspec/config.yaml` (`<!-- tacos-config-begin -->`), host `AGENTS.md` (`<!-- tacos-agents-begin -->` … `<!-- tacos-agents-end -->`, then `<!-- tacos-implementation-gates-begin -->` … `<!-- tacos-implementation-gates-end -->` when markers exist), this file, `SKILL.md`, `grill-gates.md`.

Command-scoped load map: [orchestration-binding-index.md](orchestration-binding-index.md). Narrative sections: [binding-sections/](binding-sections/).

## Apply / apply-review read order

See [binding-sections/apply-read-order.md](binding-sections/apply-read-order.md).

## Command id normalization

See [binding-sections/command-id-normalization.md](binding-sections/command-id-normalization.md).

## Content ownership (planning artifacts)

See [binding-sections/content-ownership.md](binding-sections/content-ownership.md).

## Delegation (grill)

See [binding-sections/delegation-grill.md](binding-sections/delegation-grill.md).

## Subagent return contracts

See [subagent-return-contracts.md](subagent-return-contracts.md). Discovery router: [explore-return-contract.md](explore-return-contract.md).

## Orchestrator context budget

See [orchestrator-context-budget.md](orchestrator-context-budget.md).

## Structured gates (all skills)

See [binding-sections/structured-gates.md](binding-sections/structured-gates.md).

## Stock overrides

Full stock-vs-tacos matrix: [binding-sections/stock-overrides-matrix.md](binding-sections/stock-overrides-matrix.md). `SKILL.md` carries a short highest-violation mini-table. Load the matrix file on demand — not this router.

## POST-ARTIFACT (propose, ff, continue, update)

Toggle table: [binding-sections/post-artifact-hub.md](binding-sections/post-artifact-hub.md). Step router: [post-artifact-index.md](post-artifact-index.md).

## OpenSpec validate (sync, archive, verify)

See [binding-sections/validate-and-dedrift.md](binding-sections/validate-and-dedrift.md) § OpenSpec validate.

## Main-spec dedrift pass

See [binding-sections/validate-and-dedrift.md](binding-sections/validate-and-dedrift.md) § Main-spec dedrift pass.

## Named subagent launch (non-normative examples)

See [binding-sections/subagent-launch.md](binding-sections/subagent-launch.md).

## Lightweight-host bypass

See [binding-sections/lightweight-host-bypass.md](binding-sections/lightweight-host-bypass.md).

## When orchestration is off

See [binding-sections/orchestration-off.md](binding-sections/orchestration-off.md).

## Direct output (ambient)

Chat session voice when `orchestration.enabled` is true: kernel in `openspec/config.yaml` tacos-config; skill [../../tacos-direct-output/SKILL.md](../../tacos-direct-output/SKILL.md); carve-outs [../../tacos-direct-output/references/direct-output.md](../../tacos-direct-output/references/direct-output.md). Hub and gate templates win on conflict. Planning artifacts use [artifact-prose.md](artifact-prose.md).
