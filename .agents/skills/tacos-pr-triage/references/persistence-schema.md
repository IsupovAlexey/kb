# Persistence schema

One markdown file per tracked comment or thread group under `{descriptions_root}/<branch-slug>/pr-triage/comments/<id>.md`. One markdown file per failing check run under `{descriptions_root}/<branch-slug>/pr-triage/checks/<check-slug>.md`. Comment filename `<id>` = GitHub comment `databaseId` as a decimal string when present; otherwise synthetic `suppressed-<reviewDatabaseId>-<index>` per [suppressed-review-comments](suppressed-review-comments.md). Check filename `<check-slug>` = sanitized check name ([check-sync](check-sync.md)).

## `_session.md`

YAML frontmatter only (no body required):

```yaml
---
owner: my-org
repo: my-repo
pr_number: 42
url: https://github.com/org/repo/pull/42
title: Add feature
branch: feat/add-feature
state: OPEN
author: login
last_synced_at: 2026-06-09T12:00:00Z
last_checks_synced_at: null # ISO timestamp after check sync
failing_check_count: null # integer count after last check sync
change_id: add-<feature> # optional when bound
bypass_mode: false
loop_mode: false # true when user invoked loop
loop_started_at: null # ISO timestamp on first loop invoke
loop_iteration: 0 # reset to 0 on fresh loop invoke; incremented at start of each post-settle sync pass during continuation
awaiting_settle: null # allowed: checks, reviews, both; null when not waiting
settle_started_at: null # ISO when settle phase began
last_loop_wake_at: null # ISO after watch/poll wake
loop_stopped_reason: null # allowed: merge-ready, user-stop, stuck-ci, error
automation_comment_count: null # open automation record count at baseline; recompute after every full comment sync
automation_baseline_ids: null # comma-separated open automation record ids at baseline; reset quiet-period clock when id set changes
bot_wait_started_at: null # ISO when current quiet-period window began; reset when automation baseline changes
bot_settle_poll_count: 0 # polls since last baseline change or push; reset when baseline changes or last_pushed_at updates
last_pushed_commit: null # short or full sha after commit-and-push
last_pushed_at: null
---
```

## Comment record (`comments/<id>.md`)

```yaml
---
id: "1234567890" # or suppressed-4730456549-0 for body-only low-confidence items
thread_id: PRRT_kwDO... # null for issue/review-summary or when no linked thread; review_suppressed may set this when dedupe links an existing open thread per suppressed-review-comments.md
type: inline # allowed: inline, issue, review_summary
author: reviewer
author_kind: human # allowed: human, automation
is_minimized: false # true for GitHub minimized or review_suppressed low-confidence items
minimized_reason: null # GitHub minimizedReason when is_minimized; use low-confidence for review_suppressed
source: thread # allowed: thread, review_comment, review_suppressed, issue, review_summary
review_id: null # parent PullRequestReview databaseId when source is review_suppressed
created_at: 2026-06-09T10:00:00Z
updated_at: 2026-06-09T10:00:00Z
url: https://github.com/...
path: src/foo.cs # inline only
line: 42 # inline only
is_outdated: false
is_resolved_on_github: false
status: open # allowed: open, triaged, fixed, resolved, deferred, wontfix
validity: null # set at triage; allowed: valid, needs-context, invalid-or-nit
severity: null # set at triage; allowed: P0, P1, P2
action: null # set at triage; allowed: fix-in-code, reply-only, resolve-without-code, defer
fix_state: pending # allowed: pending, applied, skipped
applied_summary: null # one line after local fix
resolved_at: null
notes: null
---
```

Body below the frontmatter fence: raw GitHub comment `body` (refresh on sync).

## Check record (`checks/<check-slug>.md`)

```yaml
---
slug: build # sanitized check name; matches filename
name: Build # display name from GitHub
check_run_id: "12345678" # latest run id when available
workflow: CI # optional workflow name
conclusion: failure # normalized pass/fail outcome from primary-listing state/bucket (Checks API conclusion on fallback); refresh on sync
status: open # allowed: open, triaged, fixed, resolved, deferred
failure_kind: null # set at triage; allowed: code, infra, unknown
matrix_role: standalone # allowed: root, dependent, standalone
root_check_slug: null # when matrix_role is dependent
validity: null # set at triage; allowed: valid, needs-context, invalid-or-nit
severity: null # set at triage; allowed: P0, P1, P2
action: null # set at triage; allowed: fix-in-code, wait-retry, needs-human, defer
fix_state: pending # allowed: pending, applied, skipped
log_truncated: false # true when log excerpt incomplete (size cap, unavailable link, or fetch failure)
applied_summary: null
resolved_at: null
notes: null
url: https://github.com/.../runs/...
started_at: 2026-06-09T10:00:00Z
completed_at: 2026-06-09T10:05:00Z
# Sonar enrichment (optional; Sonar-sourced checks only — see sonarqube-enrichment.md)
sonar_enriched: false # true after successful API fetch
sonar_fetched_at: null # ISO timestamp of last successful API fetch
sonar_gate_status: null # OK | ERROR | null when not fetched
sonar_enrichment_skipped: null # reason string when API gate failed (e.g. SONAR_TOKEN not set)
sonar_enrichment_failed: null # reason string on 401/403/5xx/timeout after attempted fetch
---
```

Body below the frontmatter fence:

1. Bounded failure log tail as a fenced code block ([check-sync](check-sync.md)). Omit or leave empty when log fetch failed or check is green on re-sync.
2. Optional `## Sonar enrichment` appendix when API enrichment ran or was attempted with partial data — **after** the log block. Do not store `SONAR_TOKEN` or raw full JSON payloads.

### Sonar enrichment appendix (`## Sonar enrichment`)

When `sonar_enriched: true` or enrichment was attempted, append:

```markdown
## Sonar enrichment

Fetched: 2026-06-09T12:00:00Z
Gate status: ERROR

### Gate conditions

|Metric|Status|Threshold|Actual|
|-|-|-|-|
|new_coverage|ERROR|80|72.1|

### Issues (capped)

|Severity|Rule|Component|Line|Message|
|-|-|-|-|-|
|MAJOR|csharp:S1234|src/Foo.cs|42|Remove unused parameter|
```

Omit the appendix when enrichment was skipped (`sonar_enrichment_skipped` set) and no API data exists. When auth/transport failed (`sonar_enrichment_failed`), the appendix MAY be omitted; the skip/fail reason lives in frontmatter and the **Check failures** report.

On green re-sync: clear Sonar enrichment frontmatter fields and remove the `## Sonar enrichment` appendix with the log body (same pass as other advisory field reset).

## Upsert rules

|Field group|On re-sync|
|-|-|
|GitHub-owned (`body`, `updated_at`, `author`, `author_kind`, `url`, `path`, `line`, `is_outdated`, `is_resolved_on_github`)|Refresh from fetch|
|Local triage (`validity`, `severity`, `action`, `status`, `fix_state`, `applied_summary`, `resolved_at`, `notes`)|Preserve when same `id`|

- New comment id from GitHub → create file with `status: open`; omit `validity`, `severity`, and `action` (or set them to `null`) until triage runs.
- Never duplicate one comment in two files.

**Check records** — same split:

|Field group|On re-sync|
|-|-|
|GitHub-owned (`name`, `check_run_id`, `workflow`, `conclusion`, `url`, `started_at`, `completed_at`, `log_truncated`, log body when re-fetched)|Refresh from fetch|
|Sonar enrichment (`sonar_enriched`, `sonar_fetched_at`, `sonar_gate_status`, `sonar_enrichment_skipped`, `sonar_enrichment_failed`, `## Sonar enrichment` appendix)|Preserve when same `slug` unless green re-sync clears them; refresh when triage re-fetches API for a still-failing Sonar check|
|Local triage (`failure_kind`, `matrix_role`, `root_check_slug`, `validity`, `severity`, `action`, `fix_state`, `applied_summary`, `notes`)|Preserve when same `slug`|
|Status transition (`status`, `resolved_at`)|Preserve when same `slug` **unless** green re-sync: when the latest run reports an explicit pass — primary-listing `bucket: pass` or `state: SUCCESS`, or normalized `conclusion: success` on fallback — set `status: resolved`, set `resolved_at` if unset, clear the failure log body excerpt, set `log_truncated: false`, reset `matrix_role` to `standalone` and `root_check_slug` to `null`, clear Sonar enrichment frontmatter (`sonar_enriched`, `sonar_fetched_at`, `sonar_gate_status`, `sonar_enrichment_skipped`, `sonar_enrichment_failed`) and remove the `## Sonar enrichment` appendix, preserve `fix_state: applied` and `applied_summary` when they record a local fix; otherwise set `fix_state: skipped` and clear `applied_summary`, and null advisory triage fields (`failure_kind`, `action`, `validity`, `severity`). When the latest run is `pending`, `in_progress`, or queued, preserve `status` and `resolved_at` unchanged. **Re-open on failure:** when a record was `status: resolved` and the check fails again → set `status: open`, clear `resolved_at`, null stale advisory triage fields (`failure_kind`, `action`, `validity`, `severity`, `matrix_role`, `root_check_slug`), reset `fix_state` to `pending`, and refresh the log body on fetch|

- New failing check from GitHub → create file with `status: open`; omit triage fields until assessment runs.
- Same check name on re-run → upsert same `slug` file (latest run wins).
- Check turns green on re-sync → keep file; refresh GitHub-owned fields; apply **Status transition** row above (resolved, clear log body, clear advisory triage fields).
- Check was resolved and fails again on re-sync → apply **Re-open on failure** row above (open, clear resolved_at, reset stale triage/fix fields, refresh log body); null Sonar enrichment frontmatter fields and remove any `## Sonar enrichment` appendix until the next assess pass re-fetches or skips.
- Never duplicate one check in two slug files.
- `gh` failure before successful fetch → do not write new records as if sync succeeded.

## `_dismissal-catalog.md`

Session dismissal catalog for cross-thread automation reuse. Schema and consult order: [dismissal-catalog](dismissal-catalog.md). Optional until first refused automation upsert. Persists across loop iterations and `AGENT_LOOP_WAKE_PR_TRIAGE` within the same branch `pr-triage/` folder.

## Cross-session

Read `_session.md`, `_dismissal-catalog.md` (when present), and existing `comments/*.md` and `checks/*.md` before fetch. Local triage and fix state is authoritative until the user changes it. Catalog entries supplement per-id records for cross-id match only.
