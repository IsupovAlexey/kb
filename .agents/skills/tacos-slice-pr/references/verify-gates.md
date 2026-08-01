# Verify gates

Run **after** all slice commits on the presentation branch and **before** [post-verify-confirm](post-verify-confirm.md), push, PR create, or post-slice descriptions.

Resolve `{descriptions_root}` from `pr.descriptions_root` in `openspec/tacos.yaml` (same value as parent procedure step 1). Plan path: `{descriptions_root}/<change>/slice-plan.md`.

Binary: exit `0` or stop. “One file off” is **fail**.

## 1. verify-tip

```bash
dotnet run ../../tacos-split-diff/scripts/split-diff.cs verify-tip \
  --feature <feature_branch> \
  --presentation review/<change-slug> \
  --merge-base origin/<trunk> \
  --slice-plan {descriptions_root}/<change>/slice-plan.md
```

|Check|Failure means|
|-|-|
|Tree identity|Non-empty `git diff --no-ext-diff <feature> <presentation>`|
|Plan coverage|Path in feature diff missing from plan, or extra plan path|

On failure: print output; abort; do not edit `slice-plan.md` without new slice-plan approval.

## 2. Trunk sync (required)

```bash
git fetch origin
git rev-parse <trunk>
git rev-parse origin/<trunk>
```

**MUST** be equal before `verify-slices`. If not, abort — recreate presentation branch from `origin/<trunk>` (`execute-slice` ref).

## 3. verify-slices

```bash
dotnet run ../../tacos-split-diff/scripts/split-diff.cs verify-slices \
  --slice-plan {descriptions_root}/<change>/slice-plan.md \
  --presentation review/<change-slug> \
  --trunk-ref origin/<trunk>
```

|Check|Failure means|
|-|-|
|Commit count|Commits on presentation after trunk ≠ slice count|
|Linear history|Each commit’s parent = prior slice tip (or trunk for slice 1)|
|Incremental files|`git diff --name-only <parent>..<commit>` = slice **Files:**|
|Cumulative files|`git diff --name-only <trunk>..<commit>` = union of files slices 1..k|

On failure: print output; abort; fix branch or void plan and re-approve.

## 4. Title checklist (before PR-create preview)

Read host docs (`pr-title-conventions` ref).

Per slice commit: host prefix applied; optional `[n/N]`; Jira key when `jira.md` binds; message ≠ branch name; matches plan.

Merge PR title: exact `--title` per host rules (verbatim in preview); not branch name alone.
