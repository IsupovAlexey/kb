# Base / target branch resolution

**Base** = trunk for `review/<change-slug>` fork and `gh pr create --base`. **Head** = presentation branch.

## Resolution order

1. User in this session
2. `slice-plan.md` frontmatter `base_branch:` (do not override without new slice-plan approval)
3. Planning artifacts: `proposal.md`, `design.md`, `grill-summaries.md`
4. `openspec/tacos.yaml` — `slice_pr.default_base_branch` (overrides `pr.default_base_branch` when both set)
5. Repository detect (below)
6. Ask

Record trunk in session and `slice-plan.md` `base_branch`.

## Detection

```bash
gh repo view --json defaultBranchRef -q .defaultBranchRef.name
git symbolic-ref --short refs/remotes/origin/HEAD 2>/dev/null
git merge-base HEAD main && echo main
git merge-base HEAD master && echo master
git merge-base HEAD develop && echo develop
```

## Trunk sync

Before diff or presentation branch: `git fetch origin`; local `<trunk>` MUST equal `origin/<trunk>`. If not → stop; rebuild from `origin/<trunk>`. Full verify-time check: parent procedure step 8 (`verify-gates` ref).

Record `base_branch` and optional `base_ref: origin/<trunk>` in `slice-plan.md`.

## Resolve step

Confirm: `Feature branch: <HEAD> → base: <trunk> @ <SHA>`. Same trunk for `git diff <trunk>...HEAD`, slice 1 parent, and `verify-slices --trunk-ref`.

```yaml
slice_pr:
  default_base_branch: null
```
