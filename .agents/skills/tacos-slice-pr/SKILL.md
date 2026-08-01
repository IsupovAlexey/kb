---
name: tacos-slice-pr
description: >-
  Post-hoc review slices: ordered commits on one presentation branch and one draft merge PR
  from a finished feature branch. Invoke via /tacos-slice-pr only (not ambient).
disable-model-invocation: true
user-invocable: true
---

# tacos slice-pr

Split one feature branch into ordered review commits on a presentation branch, then open one draft merge PR. Config: `openspec/tacos.yaml` (`slice_pr.*`, `pr.descriptions_root`). Generated `slice-plan.md` under `{descriptions_root}/<change>/`.

Invoke only via `/tacos-slice-pr` or explicit POST-ARTIFACT recommendation — not at apply completion.

Tooling: [tacos-split-diff](../tacos-split-diff/SKILL.md) (`analyze` / `reconstruct` / `verify-tip` / `verify-slices`).

## Entry

1. Read [procedure.md](references/procedure.md) when execution starts — not on invoke-only plan preview.
2. Five approval gates below are non-bypassable — load gate refs from table before each gate.

## Quick start

|User says|Effect|
|-|-|
|`/tacos-slice-pr` on `feat/<change>`|Plan → approvals → `review/<change>` → draft PR|
|`plan only`|Stop after `slice-plan.md`|
|`no PR` / `branch only`|Commits + verify; skip `gh pr create`|

## Approval (five gates, non-bypassable)

Intent ≠ approval. "Slice my PR" is not slice-plan, branch-execution, post-verify, or PR-create approval.

Each gate: preview in chat → structured prompt per [structured-gate-convention.md](../tacos-orchestration/references/structured-gate-convention.md) (**next tool call MUST** be `AskQuestion` / `AskUserQuestion` when available) → else plain text + **end turn**.

|Gate|When|After approval|
|-|-|-|
|Slice plan|Before `slice-plan.md` write|Write plan; offer branch-execution gate|
|Branch execution|After `slice-plan.md`|[procedure.md](references/procedure.md) step 7|
|Verify|After commits|step 8 — `verify-tip` + `verify-slices` exit `0`|
|Post-verify confirm|After verify pass|[post-verify-confirm.md](references/post-verify-confirm.md)|
|PR create|After post-verify confirm (if pushing)|step 10 — [approval-prompt#github-pr-create-and-sync](../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync) before `gh pr create`|

## Done when

- **Plan only:** `slice-plan.md` after slice-plan approval; no presentation branch
- **Branch only / no PR:** presentation branch with verified slices; verify exit `0`; deferred push/PR at post-verify confirm
- **Full run:** verify exit `0`; post-verify confirm approved; optional push; PR with full body per [post-pr-create.md](references/post-pr-create.md)

## References

[procedure.md](references/procedure.md) · [preflight](references/preflight.md) · [plan-and-approve](references/plan-and-approve.md) · [execute-slice](references/execute-slice.md) · [verify-gates](references/verify-gates.md) · [post-verify-confirm](references/post-verify-confirm.md) · [gh-pr-create-single](../tacos-pr/references/gh-pr-create-single.md) · [approval-prompt](../tacos-orchestration/references/approval-prompt.md)
