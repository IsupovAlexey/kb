# PR and commit title conventions (slice-pr)

Host repo rules **win** — tacos MUST NOT invent a global title format.

## Where to look (before titles)

1. User message this session.
2. **Host docs** — `README.md`, `AGENTS.md`, `CONTRIBUTING.md`, PR templates (at least one when present).
3. **Existing PRs** — `gh pr list --limit 20` when docs are silent.
4. **Ask once** if docs and history disagree.

Record source in PR-create preview (one line).

## Slice commit messages (presentation branch)

Per slice in `slice-plan.md`:

1. Apply **host** conventional-commit or team rules first.
2. Optionally `[<slice_index>/<slice_count>]` **after** the host prefix when both apply.

**Host example (illustrative)** — apply only what host docs require (e.g. release-please when README says so):

```text
chore: [1/4] add OpenSpec planning artifacts
feat: [2/4] add webhook retry handler
```

Derive from **that slice's files**, not `tasks.md` stage titles when they disagree. Same string for `git commit -m` and **Review passes** summaries.

## Merge PR title (`gh pr create`)

One title; **MUST** follow host rules. `[n/N]` in merge title optional unless user asks. Jira key when `jira.md` binds (per host placement). **MUST NOT** use branch name alone.

Frontmatter `title:` on merge description = exact `--title` string.

## PR-create preview (mandatory)

Show exact `--title` verbatim. If host rules not read, stop and read `README.md` / `CONTRIBUTING.md` first.

## Jira

When bound, include issue key per host docs (e.g. `PROJ-123` or `<KEY>-<n>`).
