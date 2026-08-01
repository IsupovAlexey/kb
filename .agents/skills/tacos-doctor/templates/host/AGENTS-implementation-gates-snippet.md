<!-- tacos-implementation-gates-begin -->

## Implementation gates (local dev)

When **Commands** lists local dev checks, **apply** and **apply-review** treat them as **hard stops** on failure. Repos with no code (docs-only, specs-only) may leave **Commands** empty — tacos orchestration still applies; gates are optional.

- Redirect **build** and **test** output to `artifacts/outputs/` (or the host's documented artifacts output path). Read and search saved logs for failures; do **not** re-run a command solely to re-display output.
- Prefer **scoped** checks (changed paths, filtered test subsets) over whole-repo runs when the repo supports them.
- Run checks in order: **formatters and linters** → **build** → **tests**; when one build suffices, reuse its output for tests.

## Commands

<!-- Repo-specific format, lint, build, and test commands are generated at install or maintained here. -->

<!-- tacos-implementation-gates-end -->
