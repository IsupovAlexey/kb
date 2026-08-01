# Capability-batch delegation

Multi-capability detect uses read-only subagents; the **parent** owns preview, confirm, and all writes.

## When to delegate

When Task spawn is supported, parent **MUST** delegate detect to **`agent-tacos-dedrift-detect`** — **no** inline parent detect on orchestration surfaces or interactive `/tacos-dedrift` reconcile (including scoped count **≤ 6**). Inline detect in the parent is **forbidden** when Task is available.

When Task is unavailable, parent runs detect inline and notes inline execution in the turn summary.

## Batching

|Constant|Value|
|-|-|
|Batch threshold|6 capabilities|
|Batch size|4 capabilities per subagent (design target 3–5)|

When scoped capability count **≤ 6**, launch one **`agent-tacos-dedrift-detect`** child for the full scope.

When count **> 6**, split capabilities into batches of 4 and launch parallel detect children when the runtime supports concurrent Task spawn; otherwise sequential detect with merged report.

## Subagent contract

|Rule|Detail|
|-|-|
|Agent name|`agent-tacos-dedrift-detect`|
|Model|`orchestration.dedrift_detect_models` per [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md)|
|Role|Detect and report only — **no file writes**|
|Input|Mode, capability list for batch, paths to main specs|
|Output|Structured drift table per [subagent-return-contracts.md](../../tacos-orchestration/references/subagent-return-contracts.md) ## Detect only — ≤15 rows|

Launch batches in parallel when the runtime supports concurrent Task/subagent spawn; otherwise sequential detect with merged report.

## Deep detect loop

When [deep-mode.md](deep-mode.md) parent loop runs (interactive `reconcile … deep` or scheduled `scheduled_depth: deep`):

|Rule|Detail|
|-|-|
|Parent|**Never** inline detect in the deep loop — mirror apply-review deep ([../../tacos-apply-review/references/deep-mode.md](../../tacos-apply-review/references/deep-mode.md) ## Parent loop)|
|Each pass|Fresh **`agent-tacos-dedrift-detect`** child(ren); when scope **> 6**, parallel batches per [Batching](#batching); parent merges into one pass report|
|Pass 1|Full scoped detect per [modes.md](modes.md) against on-disk specs + codebase|
|Pass N>1|Inputs: in-memory merged reconcile baseline + all prior `dedrift-deep-*.md`; **must** re-detect — not a cache of pass 1 output|
|Child contract|Same as [Subagent contract](#subagent-contract) — detect and report only; compare behavioral obligations per modes.md|
|Parent artifact|One `dedrift-deep-N.md` per pass from merged child reports only|
|Task unavailable|Interactive: stop with message. Scheduled: review artifact failure + exit without writes|

After each pass, parent SHOULD verify child classifications against `openspec/specs/<capability>/spec.md` on disk before merging into the write set — do not trust "already aligned" without spot-check when findings are surprising.

## Parent merge

1. Collect batch reports.
2. Merge into one combined drift table (capability → status → proposed edits).
3. Single preview for all proposed writes.
4. After user **Proceed**, apply writes **serially** on parent — no concurrent spec file edits across batches.

## Scoping `all`

`/tacos-dedrift reconcile all` enumerates every directory under `openspec/specs/` with a `spec.md`. Skip archive-only or empty capability folders per repo layout.

## Failure handling

When a batch subagent fails or times out:

- Report which capabilities were not evaluated.
- Offer retry for failed batch, narrower scope, or Cancel.
- Do not partial-write specs for capabilities the user did not see in the combined preview.
