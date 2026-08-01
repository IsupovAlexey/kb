# Slice-pr procedure

Steps for `/tacos-slice-pr` after gates in [SKILL.md](../SKILL.md) ## Approval. Config from `openspec/tacos.yaml` (`slice_pr.*`, `pr.descriptions_root`).

1. Read `slice_pr.*` and `pr.descriptions_root` → `{descriptions_root}`.
2. Resolve `openspec/changes/<name>/` (message, branch, or `openspec list`; ask if many).
3. [base-branch-resolution.md](base-branch-resolution.md) — trunk; confirm `HEAD` is feature branch.
4. [preflight.md](preflight.md).
5. [artifact-grouping.md](artifact-grouping.md) + split-diff `analyze` on `git diff <trunk>...HEAD`; [pr-title-conventions.md](pr-title-conventions.md) for slice commit titles.
6. [plan-and-approve.md](plan-and-approve.md) — present plan → **slice-plan approval** → write `slice-plan.md` → **branch-execution approval**.
7. [execute-slice.md](execute-slice.md).
8. [verify-gates.md](verify-gates.md) — pass `{descriptions_root}` from step 1.
9. [post-verify-confirm.md](post-verify-confirm.md).
10. Push (optional until PR gate) → [gh-pr-create-single](../tacos-pr/references/gh-pr-create-single.md) preview/approve + [post-pr-create.md](post-pr-create.md) + [descriptions.md](descriptions.md) + [pr-title-conventions.md](pr-title-conventions.md) when pushing.
11. Completion: branch, `slice-plan.md`, PR URL (if any), `pr-descriptions/` paths. **Reviewer notes:** squash-merge the single PR; **Review passes** links on the PR; feature branch may be deleted after merge. Re-sync with `/tacos-pr sync` if artifacts change. Single-PR-from-feature (no slices) → `/tacos-pr`, not this skill.

## Branch and staging model

- Keep original feature branch read-only; presentation branch `review/<change-slug>`; one merge PR into trunk.
- Stage only explicit paths from `slice-plan.md`; partial files via [tacos-split-diff reconstruct](../tacos-split-diff/references/usage.md).
- `slice-plan.md` immutable after slice-plan approval — void and re-approve before editing.
- Stop on non-zero verify exit; [post-verify-confirm.md](post-verify-confirm.md) before push or PR create.
- Post-slice [descriptions.md](descriptions.md) when `regenerate_descriptions` true unless user opts out.

## Flags

|User says|Effect|
|-|-|
|`--no-draft` / ready for review|`gh pr create` without `--draft`|
|plan only|Stop after slice-plan approval|
|no PR / branch only|Skip PR create|
|skip descriptions|Skip post-slice descriptions|
|presentation `review/custom`|Set branch name at slice-plan approval|
|into `develop` / base `main`|Trunk per base-branch-resolution|

Default: `default_draft_prs` true; `regenerate_descriptions` true.
