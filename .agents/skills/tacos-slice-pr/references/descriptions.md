# Post-slice descriptions

When `slice_pr.regenerate_descriptions` is true (default). Skip when false or user says “skip descriptions”.

## When

|Path|Timing|
|-|-|
|PR created this run|After `gh pr create`, same PR-create approval — before completion|
|Branch only|When workflow ends with `slice-plan.md` and no PR|
|Later|User runs `/tacos-pr sync` after artifact edits or force-push|

## Inputs

- OpenSpec: `proposal.md`, `design.md`, `specs/**/*.md`, `tasks.md`, `grill-summaries.md`, `e2e-scenarios.md` when present
- `{descriptions_root}/<change>/slice-plan.md`
- **Exclude:** prior description bodies; live GitHub bodies (except `pr_number` via `gh pr view`)

## Outputs

Under `{descriptions_root}/<change>/pr-descriptions/`:

### Merge `01-<short-name>.md`

**Frontmatter:**

```yaml
---
head_branch: review/<change-slug>
base_branch: <trunk>
pr_number: null
pr_url: null
regenerated_at: null
title: null
preserve_why: false
---
```

**Body (order):** Summary, Why, How to review, Test plan, **Review passes** (bottom → top).

Per slice from `slice-plan.md`: title, commit summary, link `https://github.com/<owner>/<repo>/pull/<n>/commits/<full-sha>`. Resolve SHAs from `git rev-list --reverse <trunk>..<presentation-branch>` after verify and post-verify confirm. Frontmatter `title:` — exact merge PR title (host rules).

### Optional `02+` review notes

```yaml
---
slice_index: 2
presentation_commit: <sha>
pr_number: <same as merge>
pr_url: <same as merge>
---
```

Short Summary / How to review for that pass only.

## GitHub sync

PR opened this run: **MUST** `gh pr edit <number> --body-file` (strip frontmatter) in the same PR-create approval ([post-pr-create](post-pr-create.md)). No placeholder body; no separate `/tacos-pr sync` for the first body.

No PR: local files only; user MAY `/tacos-pr open` or `/tacos-pr sync` later.

After force-push on presentation branch: re-run descriptions and `gh pr edit` (or `/tacos-pr sync` with approval).

No agent footers or command hints in GitHub-bound markdown.

## Config

```yaml
slice_pr:
  regenerate_descriptions: true
```
