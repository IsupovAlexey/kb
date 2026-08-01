# PR title conventions (host repo)

Host repo rules **win** — tacos MUST NOT invent a global title format.

## Where to look (before titles)

1. User message this session.
2. **Host docs** — `README.md`, `AGENTS.md`, `CONTRIBUTING.md`, PR templates (at least one when present).
3. **Existing PRs** — `gh pr list --limit 20` when docs are silent.
4. **Ask once** if docs and history disagree.

Record source in PR-create preview (one line).

## Single merge PR title (`gh pr create`)

One title; **MUST** follow host rules. Jira key when `jira.md` binds (per host placement). **MUST NOT** use branch name or change folder slug alone.

Frontmatter `title:` on `{descriptions_root}/<change>/pr-descriptions/<change>.md` = exact `--title` string.

## Regenerate

Before writing `title:` in frontmatter, complete discovery above. **MUST NOT** set `title:` from proposal headline, change slug, or branch name when host docs require a different format (e.g. Conventional Commits prefix). Leave `title: null` only when the user will supply title at open and discovery failed — prefer discovery over null.

## PR-create preview (mandatory)

Show exact `--title` verbatim. If host rules not read, stop and read `README.md` / `CONTRIBUTING.md` first.

**MUST NOT** preview a title that omits prefixes the host README requires (examples only — follow the host repo, not tacos defaults).

## Jira

When bound, include issue key per host docs (e.g. `PROJ-123` or `<KEY>-<n>`). See [tacos-jira-sync](../../tacos-jira-sync/SKILL.md).
