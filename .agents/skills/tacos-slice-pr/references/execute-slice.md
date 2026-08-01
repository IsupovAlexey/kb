# Execute presentation branch

After branch-execution approval and `{descriptions_root}/<change>/slice-plan.md`. **Do not** commit on the feature branch.

## Model

One branch `review/<change-slug>` from `origin/<trunk>`: slice 1 from trunk; slice k parent = slice k-1 tip. **No** per-slice branch refs. Tip tree MUST equal feature branch tip after all slices.

## Setup

```bash
git fetch origin
git checkout -b review/<change-slug> origin/<trunk>
```

Use trunk ref from `slice-plan.md` (`base_branch`).

## Per slice (bottom → top)

1. Parent = trunk (slice 1) or `HEAD` after prior slice.
2. For each path in this slice’s **Files:**

**Whole file:**

```bash
git checkout <feature-branch> -- <path>
git add <path>
```

**Partial — split-diff CLI:** cumulative hunks 1..k for this file; base from **merge base**:

```bash
git show <merge-base>:path > /tmp/base
git diff <merge-base>...<feature-branch> -- path > /tmp/file.diff
dotnet run ../../tacos-split-diff/scripts/split-diff.cs reconstruct \
  --base-file /tmp/base --diff-file /tmp/file.diff --hunks 0,1,2 --output path
git add path
```

3. `git commit -m "<message from plan>"`
4. Do **not** push until [post-verify-confirm](post-verify-confirm.md).

**Forbidden:** `git add .`, `git add -A`, `git add -u`, `git commit -a`.

## Verification

After last slice: parent procedure step 8 (`verify-gates` ref) — exit `0` only.

Leave feature branch at original tip. On failure: stop; do not edit plan without new approval; preflight backup ref remains (`preflight` ref).
