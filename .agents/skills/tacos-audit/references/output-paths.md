# Audit output paths

Gitignored session artifacts under `artifacts/tacos-audit/<slug>/`. Apply-review under `artifacts/openspec-reviews/<slug>/`.

## Layout

```text
artifacts/tacos-audit/<slug>/
  findings.md           # vetted findings + direction table
  index.md              # status, rejections, execution order (optional)
  audit-plan.md         # default single plan
  plans/                # when 3+ unrelated findings selected
    001-<slug>.md

artifacts/openspec-reviews/<slug>/
  apply-review.md       # after execute
```

## Write order

Recon → explore → vet → `findings.md` → user picks ids → plan preview → `audit-plan.md` → execute confirm → worktree → apply-review.

## Resume

Same slug only when user confirms. Re-run `/tacos-audit reconcile <slug>` to refresh drifted plan SHAs or index status.
