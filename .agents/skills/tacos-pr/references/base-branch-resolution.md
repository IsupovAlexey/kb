# Base / target branch resolution (single PR)

**Base** = trunk (`git diff`, `gh pr create --base`). **Head** = feature branch. Show both in create preview.

**Order:** session → frontmatter `base_branch` → proposal/design/grill-summaries (tasks if merge target stated) → `pr.default_base_branch` → detect → ask. Record in frontmatter.

```bash
gh repo view --json defaultBranchRef -q .defaultBranchRef.name
git symbolic-ref --short refs/remotes/origin/HEAD 2>/dev/null
git merge-base HEAD main && echo main
git merge-base HEAD master && echo master
git merge-base HEAD develop && echo develop
```

Before open: `git fetch origin`; if local trunk ≠ `origin/<trunk>`, stop or warn (use `origin/<trunk>` for `--base`).

```yaml
pr:
  default_base_branch: null # null = detect; no silent default main
```
