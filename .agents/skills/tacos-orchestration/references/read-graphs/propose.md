# Propose read graph

Command ids: propose, ff, continue (planning paths).

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)
- [planning.md](../../../tacos-grill/references/read-graphs/planning.md) + [grill-gates.md](../grill-gates.md) when `orchestration.grill_enabled`
- [planning-artifact-loop.md](../planning-artifact-loop.md)
- [entry-conditionals.md](../entry-conditionals.md) matching rows

## Before command

- STOP if `grill.planning` pending — planning grill once, then artifacts per [planning-artifact-loop.md](../planning-artifact-loop.md)
- When **explore** concluded in the same session (or change folder has substantive `## explore`): **STOP** until planning grill completes — explore text MUST NOT substitute for planning-phase **User inputs** ([grill-gates.md](../grill-gates.md) §6)
- **Forbidden:** same-turn **propose** / **ff** after explore conclusion without completed planning grill — end explore turn first ([explore.md](explore.md) ## After explore / before propose)
- **Forbidden:** prefilled `grill-summaries.md` (`grill_mode` set, phases `complete`, explore-derived **User inputs**, cross-phase "Same as proposal") without gather → parent interview → summarize
- When `grill.planning` is `pending` and structured tools are available: **next parent tool call MUST** be `AskQuestion` / `AskUserQuestion` for grill mode per [structured-gate-convention.md](../structured-gate-convention.md) — not `grill-summaries.md` or planning artifact writes
- continue: per-phase grill when `grill.<phase>` pending; write only that artifact
- When `openspec/changes/<name>/jira.md` exists: read per [planning-artifact-loop.md](../planning-artifact-loop.md) ### `jira.md` as planning context before planning artifact writes (even when `jira.enabled` is false)

## During command

- Chat voice: tacos-config direct-output kernel; carve-outs [direct-output.md](../../../tacos-direct-output/references/direct-output.md) on demand
- Before writing **proposal** / **specs** / **design** / **tasks**: [artifact-prose.md](../artifact-prose.md)
- Before writing **proposal** / **specs** / **tasks**: [planning-artifact-loop.md](../planning-artifact-loop.md) ## Generation-time contract — Modified Capabilities research, specs STOP on unlisted contradiction, FORBIDDEN Verify Decision embed, Apply review **medium** line (not one-line pointer)
- Before writing **tasks**: also [task-stage-contract.md](../task-stage-contract.md) ## Per-stage checklist order (Verify Decision separate rows; Apply review FORBIDDEN shortened)
- Per [grill-gates.md](../grill-gates.md) and [planning-artifact-loop.md](../planning-artifact-loop.md)

## After command

- When apply-ready (full bundle including tasks): same-turn POST-ARTIFACT — load [post-artifact-index.md](../post-artifact-index.md) first; step bundles on demand
- E2E / spec review / validate / sign-off per toggles in post-artifact-index

## Done when

- `grill.planning` complete or skipped; planning artifacts written; POST-ARTIFACT gates pass when apply-ready

## On demand

- [binding-sections/stock-overrides-matrix.md](../binding-sections/stock-overrides-matrix.md) — full matrix before artifact writes after planning grill (not the hub router)
- [orchestration-binding-index.md](../orchestration-binding-index.md) — command-scoped section files when apply-ready
- [post-artifact-planning-review.md](../post-artifact-planning-review.md) — when planning spec review step runs
- [post-artifact-signoff.md](../post-artifact-signoff.md) — when human sign-off or apply review runs

## Hub authority

[orchestration-binding.md](../orchestration-binding.md) wins on conflict with this index.
