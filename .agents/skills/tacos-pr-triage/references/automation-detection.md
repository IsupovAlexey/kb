# Automation detection

At sync, classify each comment author as **`human`** or **`automation`**. Store `author_kind` on the record ([persistence-schema](persistence-schema.md)). [bypass-mode](bypass-mode.md) sweeps `author_kind: automation` only; human reviewers keep [approval-gates](approval-gates.md).

## Fetch fields

From `gh` / GraphQL comment payloads, read at minimum:

- `author.login`
- `author.__typename` when present (`Bot` vs `User`)

## Assessment (first match wins)

### 1. GitHub Bot type

`author.__typename == Bot` → `automation`.

### 2. Login patterns (case-insensitive)

|Pattern|Examples|
|-|-|
|Contains `[bot]`|`dependabot[bot]`|
|Ends with `-bot`|`codecov-bot`|
|Exact login in [Known AI reviewer logins](#known-ai-reviewer-logins)|`Copilot`, `github-copilot` — applies even when `author.__typename == User`|
|Substring in built-in catalog|See [Built-in catalog](#built-in-catalog) — **only when** `author.__typename == Bot` **or** the login is listed in [repo-local overrides](#3-repo-local-overrides-optional) (`logins` or `substrings`)|

Substring match on **login** only (not comment body). When `author.__typename == User`, catalog substrings alone do **not** classify as automation (avoids misclassifying personal logins such as `claude-smith`) — except [Known AI reviewer logins](#known-ai-reviewer-logins).

### Known AI reviewer logins

Exact `author.login` match (case-insensitive) → `automation` even when `author.__typename == User`:

|Login|Typical source|
|-|-|
|`copilot`|GitHub Copilot code review|
|`github-copilot`|GitHub Copilot|
|`copilot-pull-request-reviewer[bot]`|GitHub Copilot PR reviewer app|

Extend via [repo-local overrides](#3-repo-local-overrides-optional) `logins` for org-specific AI reviewers that post as `User`.

### 3. Repo-local overrides (optional)

When present, merge with built-in rules (union — any match → `automation`):

```text
automation-authors.yaml
```

Search order (first file found):

1. `{descriptions_root}/automation-authors.yaml`
2. Repository root `automation-authors.yaml`

```yaml
logins:
  - exact-github-login
substrings:
  - inhouse-reviewer-prefix
```

Use for org-specific AI reviewers, internal automation accounts, and named tools not in the built-in catalog. Do not commit secrets; logins only.

### 4. Default

No match above → `human`.

Re-sync refreshes `author_kind` from GitHub metadata and override files. Preserve local triage/fix fields on upsert.

## Built-in catalog

Substring match on `author.login` (case-insensitive). Typical automation — extend via [repo-local overrides](#3-repo-local-overrides-optional):

|Substring|Typical source|
|-|-|
|`github-actions`|CI|
|`dependabot`|Dependency PRs|
|`renovate`|Dependency PRs|
|`codecov`|Coverage|
|`sonar`|SonarQube|
|`snyk`|Security scan|
|`bugbot`|Bugbot|
|`copilot`|GitHub Copilot review|
|`claude`|Claude review bots|
|`cursor`|Cursor review automation|
|`graphite`|Graphite|
|`mergify`|Merge automation|
|`danger`|Danger|
|`pullrequest`|PR automation apps|
|`reviewbot`|Generic review bots|
|`stale`|Stale bot|

Personal or ambiguous `User` logins that happen to contain a catalog token (e.g. `claude-smith`) remain `human` per step 2 unless `__typename == Bot` or `automation-authors.yaml` lists the login explicitly.

## Triage report

Show `author_kind` in the triage line when `automation`:

```text
- [<id>] author_kind=automation validity=... — <summary>
```

Group automation items separately from human reviewers in the report when both exist.

Minimized automation comments (`is_minimized: true` on sync) MUST appear in the triage report with a `minimized` tag when `author_kind: automation`.

`source: review_suppressed` records MUST appear with a `suppressed` tag — they are first-class automation items even without `thread_id`.
