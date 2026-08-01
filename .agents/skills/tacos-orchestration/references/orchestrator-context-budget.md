# Orchestrator context budget

Parent orchestrator heuristics when spawning and merging named subagents. Decision 6 (orchestrator-context-delegation). Return caps live in [subagent-return-contracts.md](subagent-return-contracts.md).

## Parallel wave size

- Launch 2–4 parallel children per wave when the host supports concurrent Task spawn
- When ≥2 independent questions target distinct surfaces, prefer parallel children in one parent turn — not serial parent Grep/Read first
- Sequential fallback when concurrent spawn is unavailable; note sequential fallback in the turn summary

## Cognitive locality merge

Before spawning, merge child scopes that share file ownership or cognitive locality into one child prompt when independence is weak — reduces spawn count without dropping coverage.

Within-stage parallel slices (independent task rows, disjoint file ownership) **SHOULD** pipeline implement → gate-runner → apply-review per slice when documented in [task-stage-contract.md](task-stage-contract.md). Default sequential when ownership overlaps or is unclear.

## Transcript pollution

Forbidden in the parent orchestrator thread:

- Full child tool output, file contents, diffs, or review artifact bodies
- Unbounded discovery or gate log dumps
- Pasting merged review artifacts into chat — cite path + Summary line only
- Polling background agents (Await, status checks, or transcript fetch) for full child transcripts or tool dumps — consume only the bounded return contract output

Parent merges per return contracts; discovery/implement/fix ≤12 bullets; gate ≤5 bullets + log path; detect table ≤15 rows.

## Prompt-by-reference

Child dispatch prompts MUST cite skill paths and return contract refs — not inlined full `SKILL.md` bodies or full planning artifact paste. Pass scope, artifact paths, and task row citations the child needs.

Installed agent templates use prompt-by-reference patterns per [runtime-delegation.md](runtime-delegation.md).

## Delegated roles

When Task spawn is supported, orchestration delegates disposable work to named host subagents:

|Agent `name:`|Role|
|-|-|
|`agent-tacos-apply-implement`|Isolated worktree implementation|
|`agent-tacos-orchestrator-fixes`|Review remediation from artifact + scope|
|`agent-tacos-gate-runner`|Host or scoped implementation gates|
|`agent-tacos-dedrift-detect`|Drift classification table|

Parent retains grill interview, human sign-off, worktree merge after implement summary, review child launch, and coordination.

Cross-links: [orchestration-binding.md](orchestration-binding.md); [proactive-explore-delegation.md](proactive-explore-delegation.md); [review-gate-pass.md](review-gate-pass.md) ## Turn-summary delegation record.
