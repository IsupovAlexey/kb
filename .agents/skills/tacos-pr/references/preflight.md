# Preflight (checkout)

Start of every `/tacos-pr` workflow before artifacts or description writes.

```yaml
pr:
  warn_dirty_checkout: true
  require_clean_checkout: false
```

```bash
git status --porcelain
```

Ignore `artifacts/` paths. Else dirty → warn (list paths; commit/stash suggestion) or stop if `require_clean_checkout: true`.

**open/sync:** treat dirty like require confirm even when yaml false, unless user confirmed after warning.

Regenerate-only may continue after warn when `require_clean_checkout` is false.
