# Explore read graph

Command id: explore (normalize host `opsx-explore` per [openspec-commands.md](../openspec-commands.md)).

Load after `SKILL.md` Entry steps 1–2 (`openspec/tacos.yaml`, command normalization).

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)
- [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md) when `orchestration.grill_enabled`
- [proactive-explore-delegation.md](../proactive-explore-delegation.md) when Task supported

## Before command

- No planning grill loop — [grill-gates.md](../grill-gates.md) § Explore is not grilling
- Delegate codebase search per [proactive-explore-delegation.md](../proactive-explore-delegation.md) when Task supported

## During command

- Chat voice: tacos-config direct-output kernel; load [direct-output.md](../../../tacos-direct-output/references/direct-output.md) on conflict only — not in MUST read by default
- Triggered grill when `grill.triggers.explore_on_decision` + decision signals — [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md)

## Done when

- Triggered grill satisfied when signals fired; record conclusions in `grill-summaries.md` ## explore when decisions crystallize
- **End turn** — **propose** / **ff** is a separate command; do not batch **propose** / **ff** or planning-section writes (`## proposal` … `## tasks`) in the same turn as explore conclusion (`## explore` update is allowed)

## After explore / before propose

When the user will invoke **propose** or **ff** next (same session or later):

- `## explore` records conclusions only — it does **not** satisfy `grill.planning` or planning-phase **User inputs**
- **Forbidden:** copying explore conclusions into `grill-summaries.md` planning sections (`## proposal` … `## tasks`) without planning grill (gather → grill mode offer → parent interview → summarize)
- **Forbidden:** same-turn **propose** / **ff** after explore conclusion while `grill.planning` is `pending` — end the explore turn first; next turn runs planning grill per [planning-artifact-loop.md](../planning-artifact-loop.md)
- Full matrix: [binding-sections/stock-overrides-matrix.md](../binding-sections/stock-overrides-matrix.md)

## Six override invariants (compact)

Hub wins on conflict. Full matrix: [binding-sections/stock-overrides-matrix.md](../binding-sections/stock-overrides-matrix.md).

|Invariant|Explore obligation|
|-|-|
|Stock opsx MUST NOT win|MUST NOT follow stock opsx or use "prefer reasonable decisions" instead of tacos-grill — [stock-override-binding.md](../stock-override-binding.md)|
|Planning grill sequence|explore does not substitute for `grill.planning` on propose / ff|
|Explore ≠ planning grill|Triggered grill only — not gather → interview → summarize|
|POST-ARTIFACT|Not required on explore-only turns unless crystallizing for future propose|
|Delegation matrix|Prefer host explore / `tacos-spec-grounding` Task — [runtime-delegation.md](../runtime-delegation.md)|
|OpenSpec validate stops|sync / archive / verify hard stops — not explore entry|
|Stock-override reachability|Mini-table here + link — full stock-override matrix on demand for propose / apply|

## MUST NOT load (explore-only, unless triggered)

- [post-artifact-index.md](../post-artifact-index.md) POST-ARTIFACT step bundles
- [pipeline-and-verification.md](../../../tacos-apply-review/references/pipeline-and-verification.md)
- Full stock-override matrix inlined in the same turn
