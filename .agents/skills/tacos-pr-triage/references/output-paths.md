# PR triage output paths

Persisted state under repository-root `{descriptions_root}/` (from `pr.descriptions_root` when `openspec/tacos.yaml` exists; default `artifacts/openspec-artifacts`), gitignored.

```text
{descriptions_root}/<branch-slug>/pr-triage/
├── _session.md
├── _dismissal-catalog.md
├── checks/
│   └── <check-slug>.md
└── comments/
    └── <id>.md
```

`_dismissal-catalog.md` — session dismissal catalog for cross-thread automation reuse ([dismissal-catalog](dismissal-catalog.md)); optional until first refused automation upsert.

## Slug

**Branch folder slug** — always the sanitized current branch name (`git branch --show-current`):

- Lowercase
- Replace `/`, `\`, spaces, and non-alphanumeric runs with `-`
- Trim leading/trailing `-`
- Collapse repeated `-`

**Check file slug** — sanitized check **name** for `checks/<check-slug>.md` ([check-sync](check-sync.md)); distinct from branch folder slug.

Store `pr_number`, `url`, `owner`, `repo`, `last_synced_at`, `last_checks_synced_at`, and `failing_check_count` in `_session.md` — do not rename the folder when the PR number is discovered.

## Config

```yaml
pr:
  descriptions_root: artifacts/openspec-artifacts
```

Do not commit unless the host project opts in. Overwrite only under this **branch-slug** folder's `pr-triage/` tree for the active session.
