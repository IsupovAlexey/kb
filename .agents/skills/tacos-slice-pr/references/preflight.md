# Preflight

Before analysis or branch creation. Abort with clear fix instructions on failure.

## Checks

|Check|Pass|Fail action|
|-|-|-|
|Checkout|No unstaged/staged diff; no stray untracked that would enter a commit|Abort: commit, stash, or remove; list paths|
|Branch|Named branch (not detached)|Abort: checkout feature branch|
|Not on base|`HEAD` ≠ merge base for trunk|Abort: checkout feature branch with full change|
|Base detectable|Trunk per parent procedure step 3 (`base-branch-resolution` ref)|Abort: user supplies base|
|Trunk sync|After `git fetch`, local trunk = `origin/<trunk>`|Abort: explain skew; offer pull or rebuild from `origin/<trunk>`|
|Feature diff|`git diff --stat origin/<trunk>...HEAD` non-empty (or user confirms empty)|Abort or clarify|

## Recoverable snapshot

```bash
SHA=$(git stash create "tacos-slice-pr-pre")
if [ -n "$SHA" ]; then
  git update-ref "refs/backup/tacos-slice-pr-$(date +%s)" "$SHA"
fi
```

Report backup ref. Does not auto-stash/commit or fix skewed trunk without user action.
