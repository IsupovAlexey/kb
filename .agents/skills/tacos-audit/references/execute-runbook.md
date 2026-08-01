# Audit execute runbook

`/tacos-audit execute <slug>` — worktree executor + parent apply-review verdict.

## Preconditions

- `artifacts/tacos-audit/<slug>/audit-plan.md` (or selected `plans/*.md`) exists
- Frontmatter `handoff: execute`
- User selected plan at preview gate
- Dependencies (if any) marked DONE in `index.md`

## Drift check

Before execute confirm:

```bash
git diff --stat <planned_at_sha>..HEAD -- <in-scope paths from plan>
```

If drift exceeds plan assumptions → STOP; refresh plan or `/tacos-audit reconcile <slug>`.

## Execute confirm gate

After drift check passes, collect explicit user confirmation before dispatch. Plain-text or structured prompt — required; silence is not approval.

## Dispatch

1. Launch `agent-tacos-audit-executor` in **isolated worktree**.
2. Inline **full audit-plan text** in child prompt (worktree may lack uncommitted `artifacts/`).
3. Child preamble: follow plan **## Work**; run verify commands; STOP on plan STOP conditions; commit in worktree only.

Parent MUST NOT pass yaml `model` on Task — host reads installed agent `model:`.

## Parent review

After executor completes:

1. Re-run every done criterion from plan in worktree.
2. Scope check: `git diff --stat` vs plan in/out lists.
3. Dispatch parallel `agent-tacos-apply-review` (+ additional when applicable); write `artifacts/openspec-reviews/<slug>/apply-review.md`.
4. Verdict per review gate — parent never patches user working tree.

Merging and push stay user-owned.

## Decline direct implement

If user asks audit parent to implement during execute review → decline; send back to executor or revise plan.
