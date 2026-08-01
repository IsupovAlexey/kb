# Regenerate PR description from artifacts

Rebuild `{descriptions_root}/<change>/pr-descriptions/<change>.md` ([output-paths.md](output-paths.md)). No GitHub write unless user continues to sync with approval. [preflight.md](preflight.md) first; [change-resolution.md](change-resolution.md) for active or archived artifact dir.

## Include

`proposal.md`, `design.md`, `specs/**/*.md`, `tasks.md`, `grill-summaries.md`, optional `e2e-scenarios.md`; `pr.preserve_why` for new files; [pr-title-conventions.md](pr-title-conventions.md).

## Exclude

`slice-plan.md`; prior description bodies (except **Why**); live PR bodies (`gh pr view` only for `pr_number`/`pr_url` backfill); other `pr-descriptions/` files with `slice_index`.

## Preserve Why

If `preserve_why: true`: extract `## Why` → regenerate per [description-template.md](description-template.md) → reinsert verbatim.

## Title (frontmatter)

Before writing `title:`, run host discovery in [pr-title-conventions.md](pr-title-conventions.md). Set `title:` to the exact GitHub `--title` string (host prefix, scope, Jira key when bound). **MUST NOT** copy proposal headline, change slug, or branch name when host docs require a different format. Preserve existing `title:` only when `preserve_why`-style retention does not apply and the user did not ask to refresh the title — on explicit regenerate, re-derive from host rules unless the user scoped “body only”.

## Output

Frontmatter: `head_branch`, `base_branch`, `pr_number`, `pr_url`, `regenerated_at`, `title`, `preserve_why`. Body: Summary, Why, How to review, Test plan only. `regenerated_at` ISO-8601 UTC.

**Legacy:** single `01-*.md` without `slice_index` → refresh in place or normalize per user ([output-paths.md](output-paths.md)).

**After:** If the user wants GitHub updated in the same turn, run the **Sync** workflow row in SKILL.md (preview → approval → `gh pr edit`). Otherwise report the file path and suggest open or sync.

**Slice description file:** sync MAY push a scoped file that already has `pr_number`; regenerate does not rebuild slice **Review passes** sections — use `/tacos-slice-pr` for those.
