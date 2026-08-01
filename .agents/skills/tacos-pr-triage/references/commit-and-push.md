# Commit and push (before GitHub resolve)

When local **fix-in-code** changes exist, **commit and push to the PR branch before** any `gh` reply or thread resolve. Reviewers expect links to pushed commits, not only local edits.

Runs as **Publish gate** — the **only** structured prompt for fix review + commit + push. There is **no** separate review gate `AskQuestion`. See [gate-budget](gate-budget.md).

Applies after fix picklist implementations and before [Resolve picklist](approval-gates.md#resolve-picklist). **Does not apply** when `bypass_mode: true` — [bypass-mode](bypass-mode.md) § Autopublish runs without prompts.

## When required

|Situation|Action|
|-|-|
|`fix_state: applied` with `action: fix-in-code` (or local file edits this pass) and not on `origin`|Publish gate — **1** `AskQuestion`|
|`reply-only` / `resolve-without-code` only|Skip publish; go to resolve picklist|
|`git status` clean and `git log origin/<branch>..HEAD` empty|Skip — set `last_pushed_commit` from `git rev-parse HEAD`|

```bash
git branch --show-current
git status --porcelain
git log origin/<branch>..HEAD --oneline
```

## Publish gate (one prompt — includes fix review)

After fix picklist work completes, when publish is required:

1. **Preview in chat only** (same turn, before `AskQuestion` — not a gate):
   - Scoped `git diff` / `--stat` for fix-in-code changes
   - Proposed commit message when a new commit is needed
   - `git log origin/<branch>..HEAD --oneline` when commits exist but are unpushed
   - One line: what `approve` does (“commit N files with message …, then push” or “push M commit(s)”)
2. **One** `AskQuestion` / `AskUserQuestion` — **end turn**. Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): **forbidden:** prose-only publish menu when structured tools are present.

**Prompt:** `Publish these fixes to origin/<branch>?`

|id|label|
|-|-|
|`approve`|Commit (if needed) and push — then continue to resolve picklist|
|`edit`|Revise commit message (re-preview; **same** publish prompt once more — still one gate type)|
|`already-on-remote`|Already on remote — skip to resolve picklist|
|`defer`|Stop this pass|

On `approve` only: `git add` (scoped paths) → `git commit` (if needed) → `git push` → set `_session.md` `last_pushed_commit` / `last_pushed_at` → resolve picklist.

**Forbidden:** a prior “are fixes ready to commit?” `AskQuestion`; separate commit and push prompts; `git` writes before structured `approve`.

## Reply commit link

When `last_pushed_commit` is set, replies **MUST** include:

```text
Fixed in <short-sha>: https://github.com/<owner>/<repo>/commit/<sha>

<applied_summary>
```

When publish was `defer`, use path/line fallback per [resolve-threads](resolve-threads.md).

## Errors

- Push rejected → report; do not resolve until push succeeds or user chooses `defer`.
