# Push description to GitHub (sync)

`gh pr edit` for an existing PR. No `pr_number` yet → stop; run the **Open** workflow row in SKILL.md first.

**Pre:** [preflight.md](preflight.md); `gh`; `pr.enabled`; file per [output-paths.md](output-paths.md); regenerate if `pr.regenerate_on_sync` (default) unless push-only.

1. Scope `<change>.md` (skip `slice_index` unless user scoped note).
2. Optional regenerate (single-PR description artifacts only).
3. **Gates:** per SKILL.md Approval — [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync). After preview, when structured tools are available the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` with `approve` / `edit` / `cancel` ids ([interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt preferred) — **forbidden:** `gh pr edit` before structured selection. Preview MUST include full body; when frontmatter `title` is set and differs from `gh pr view --json title`, preview the exact new title (host rules per [pr-title-conventions.md](pr-title-conventions.md)).
4. Strip YAML frontmatter (and footers) before `--body-file`; **MUST NOT** pass the full `pr-descriptions/<change>.md` including frontmatter to `gh`. Then `gh pr edit <n> --body-file <body.md>`; when title changes in preview, add `--title "<exact-host-title>"`. Update `regenerated_at`, `pr_url`.

Errors: missing `gh` → stop; missing `pr_number` → offer **Open** workflow in SKILL.md; edit fail → report stderr.

**Push-only:** skip regenerate; use file as-is.
