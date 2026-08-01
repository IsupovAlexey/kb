# Audit handoff routing

Set `handoff` in audit-plan frontmatter. After plan preview **Proceed**, parent MAY invoke downstream skills when user confirms handoff (not emit-only).

## Routing matrix

|`handoff`|When|Parent action|
|-|-|-|
|`execute`|Bounded fix; audit-plan has **## Work**|`/tacos-audit execute <slug>` after execute confirm|
|`work`|Needs planning grill; single bounded session|Invoke `/tacos-work` — seed **Intent** from audit-plan|
|`propose`|Multi-capability; delta specs; staged apply|Invoke `/opsx-propose` — cite audit-plan path|
|`dedrift`|Main spec stale vs code|Invoke `/tacos-dedrift reconcile` or `conform` for named capabilities|

## Multiple findings

- Related findings → single `audit-plan.md`
- 3+ unrelated → `plans/NNN-<slug>.md` each with own `handoff`
- User may pick different handoffs per plan file

## Dedrift

Audit only **recommends** dedrift — finding cites capabilities. Parent invokes `/tacos-dedrift` only after user confirms at handoff gate. No auto-reconcile.

## Downstream grill

|Handoff|Grill owner|
|-|-|
|`execute`|None in audit; apply-review after executor|
|`work`|tacos-work planning grill when `orchestration.grill_enabled`|
|`propose`|tacos planning grill|
|`dedrift`|dedrift preview gate only|
