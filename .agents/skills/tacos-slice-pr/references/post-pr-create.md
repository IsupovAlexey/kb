# Post-PR create (slice merge PR)

After [post-verify confirm](post-verify-confirm.md) and user approval at the PR-create gate. One PR: presentation branch → trunk.

**Preview and approve** — follow [gh-pr-create-single.md](../../tacos-pr/references/gh-pr-create-single.md) with slice field overrides:

|Field|Slice value|
|-|-|
|Head|`review/<change-slug>`|
|Base|trunk from `slice-plan.md`|
|Draft?|`slice_pr.default_draft_prs` (user `--no-draft` overrides)|

Draft body sections from OpenSpec artifacts per [descriptions.md](descriptions.md): Summary, Why, How to review, Test plan. **Review passes** need PR number — add after create. Read [pr-title-conventions.md](pr-title-conventions.md) for exact merge `--title`.

Gate: [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync) before `gh pr create`.

## On approve (atomic)

One gate: create and full GitHub body on GitHub.

1. `gh pr create --base <trunk> --head review/<change-slug> --title "<exact-host-title>" --body "Description follows." --draft` (or without `--draft` per config)
2. Post-slice descriptions ([descriptions.md](descriptions.md)) — write `01-*.md` with **Review passes** (`pr_number` from `gh pr view`)
3. `gh pr edit <number> --body-file <body-only.md>` (strip frontmatter)
4. Report PR URL with full description on GitHub

Complete steps 1–3 in the same PR-create approval — do not leave a placeholder body or defer initial sync to `/tacos-pr sync`.

Skip 1–3 for **no PR / branch only**. When `regenerate_descriptions: false`, create with previewed title and user/minimal body only.

Single PR from feature branch without review slices → **`/tacos-pr`**, not this skill.
