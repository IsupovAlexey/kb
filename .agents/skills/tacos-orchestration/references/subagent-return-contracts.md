# Subagent return contracts

Canonical return shapes and numeric caps for orchestration child agents when the parent merges child output into chat or review context. Decision 5 (orchestrator-context-delegation).

## Family overview

|Role|Child examples|Return shape|Cap|
|-|-|-|-|
|Discovery|host `explore`, apply discovery scouts|Bullet summary|≤12 bullets|
|Implement|`agent-tacos-apply-implement`|Implement summary bullets only — no full diffs|≤12 bullets|
|Fix|`agent-tacos-orchestrator-fixes`|Fix summary bullets only|≤12 bullets|
|Gate|`agent-tacos-gate-runner`|Pass/fail + failing command + log path + excerpt bullets|≤5 bullets + mandatory log path|
|Detect|`agent-tacos-dedrift-detect`|Structured table rows — no prose dump|≤15 rows|

Parent merges child output into the turn summary or artifact path cited in the dispatch prompt — not full tool output, file contents, or unbounded dumps.

## Discovery

Child reports MUST be:

- ≤12 bullets total
- Paths + one-line role per path when path citation is needed
- Symbols / line ranges when locating code — not file contents
- Risks / unknowns when discovery is incomplete

Forbidden: file contents, diffs, full tool output, or unbounded dumps.

Parent merges discovery bullets into chat — max ≤12 bullets per discovery pass in the turn summary. When the parent reads a known path for edit, Grep before full Read with a line limit.

Exempt: Parent MAY Read/Grep paths explicitly named in the active stage implementation checkbox without prior explore when the task row names the file and the read is for edit, not discovery.

Router: [explore-return-contract.md](explore-return-contract.md) points here for discovery caps.

## Implement

When **`agent-tacos-apply-implement`** (or inline fallback) completes a worktree slice:

- Return implement summary bullets only — ≤12 bullets
- Cite changed paths and one-line intent per path when helpful
- Note worktree location or merge readiness when relevant
- Forbidden: full diffs, pasted file bodies, or serial tool dumps in the child return

Parent MUST merge the worktree into the session branch before apply review evaluates the merged tree.

## Fix

When **`agent-tacos-orchestrator-fixes`** remediates from a failing review artifact:

- Return fix summary bullets only — ≤12 bullets
- Cite review artifact path and remediated scope
- Forbidden: full parent session replay, review artifact body paste, or unbounded fix narration

Parent MUST spawn a fresh re-review Task after fixes per anti-short-circuit rules.

## Gate

When **`agent-tacos-gate-runner`** runs host or scoped gate profiles:

- Return pass/fail, failing command name when fail, and mandatory log path under `artifacts/outputs/` (or host-documented artifacts path)
- ≤5 summary excerpt bullets — not full log contents in the parent thread
- Forbidden: piping full build/test output into chat

## Detect

When **`agent-tacos-dedrift-detect`** reports drift classification:

- Return structured table — ≤15 rows
- Columns per tacos-dedrift detect contract (capability, classification, evidence pointer)
- Forbidden: prose dump of spec or code bodies in the parent thread

## Parent merge discipline

- Cap merged finding bullets per [review-format.md](../../tacos-apply-review/references/review-format.md) when parent merges parallel review children
- Record inline fallback in turn summary when Task is unavailable
- Skill paths in child prompts by reference — not inlined full `SKILL.md` bodies

Cross-links: [orchestrator-context-budget.md](orchestrator-context-budget.md); [proactive-explore-delegation.md](proactive-explore-delegation.md); [runtime-delegation.md](runtime-delegation.md); [task-stage-contract.md](task-stage-contract.md) ## Parent discovery discipline.
