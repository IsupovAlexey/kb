# GitHub PR create (single PR)

One PR from feature branch + `pr-descriptions/<change>.md`. Slicing/`review/*` → **`/tacos-slice-pr`**.

```yaml
pr:
  default_draft_prs: true
```

**Pre:** [preflight.md](preflight.md); `gh`; `pr.enabled`; current `<change>.md`; [base-branch-resolution.md](base-branch-resolution.md); branch pushed.

## Before preview

1. Host PR title rules ([pr-title-conventions.md](pr-title-conventions.md)) + host `README.md` / `CONTRIBUTING.md` / `AGENTS.md` when present.
2. Draft exact `--title` (literal string per host rules); set or confirm frontmatter `title:` on the description file.
3. Draft body sections from OpenSpec artifacts (or use regenerated file body).

## Preview (required)

|Field|Value|
|-|-|
|Head|feature branch|
|Base|trunk per base-branch-resolution|
|Title|Exact host-formatted string (verbatim)|
|Title source|One line (e.g. README, `gh pr list`)|
|Draft?|per config|
|Body|Full markdown (no frontmatter)|

**MUST NOT** preview a title that omits prefixes the host README requires (examples only — follow the host repo, not tacos defaults).

**Gates:** per SKILL.md Approval — [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync). After preview, when structured tools are available the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` with `approve` / `edit` / `cancel` ids ([interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt preferred) — **forbidden:** prose-only approval or `gh pr create` before structured selection. Preview fields: table above. On `approve`:

Strip YAML frontmatter from the description file (content below the closing `---`) before writing `--body-file`; **MUST NOT** pass the full `pr-descriptions/<change>.md` including frontmatter to `gh`.

```bash
gh pr create --base <base> --head <head> --title "<exact-host-title>" --body-file <body.md> [--draft]
```

Update `pr_number`, `pr_url`, `regenerated_at`, `title` in frontmatter. If a PR already exists for head → stop create; run the **Sync** workflow row in SKILL.md instead.
