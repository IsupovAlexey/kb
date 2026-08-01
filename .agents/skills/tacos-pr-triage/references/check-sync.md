# Check sync

**gh-first** fetch of PR check runs and bounded failure log tails. Runs **before** comment sync on every invoke ([fetch-and-sync](fetch-and-sync.md) preflight).

## Preflight

```bash
gh auth status
```

If `gh` is missing or not authenticated → stop with actionable message (`gh auth login`). Re-running `gh auth status` after a successful preflight this invoke is a no-op. Do not write check records as if sync succeeded.

## Fetch check runs

Primary listing:

```bash
gh pr checks [<number>] --json name,state,link,startedAt,completedAt,workflow,bucket
```

**Required fields from primary path:** `name`, failing/passing `state` or `bucket`, and `link` (or equivalent details URL) for log fetch on failures.

**Checks API fallback** — run when any required field is absent from `gh pr checks` output, or when numeric `check_run_id` is needed:

```bash
gh api repos/{owner}/{repo}/commits/{head_sha}/check-runs --paginate
```

Use the PR head SHA from `gh pr view --json headRefOid` when the API path requires a commit.

## Persist scope

On each sync, create or update `checks/<check-slug>.md` for **failing** check runs and for previously persisted records that transition on re-sync:

- Include runs whose primary-listing `bucket` is `fail` from `gh pr checks` (or normalized failure on Checks API fallback). Exclude `pending`, queued, and in-progress runs — do not persist running checks as failures.
- Skip all-green runs on initial sync — do not create files for passing checks.
- On re-sync, when a previously failing check is now green: **keep** the file; refresh GitHub-owned fields and set local `status: resolved` (see [persistence-schema](persistence-schema.md)).
- On re-sync, when a previously **resolved** check fails again: set `status: open`, clear `resolved_at`, null stale advisory triage fields, and refresh the failure log body so assessment and picklists see the recurring failure.

**Green detection:** After fetching the current check listing, scan local `checks/*.md` files whose `status` is not `resolved`. For each, map file check-slug → check `name` and look up the latest `state`/`bucket` (or normalized `conclusion` on Checks API fallback) in the full listing (including passing checks). Apply the **Status transition** row only when the latest run reports an explicit pass — primary-listing `bucket: pass` or `state: SUCCESS`, or fallback `conclusion: success`. When the run is `pending`, `in_progress`, or queued, leave local `status` unchanged; absence from the failing subset is not sufficient for resolution. For recurring failures, apply **Re-open on failure** per [persistence-schema](persistence-schema.md) § Check records.

## Check-slug

`<check-slug>` = sanitized check **name** (not numeric run id):

- Lowercase
- Replace `/`, `\`, spaces, and non-alphanumeric runs with `-`
- Trim leading/trailing `-`
- Collapse repeated `-`

**Latest run wins:** same check-slug overwrites the prior file (upsert by name).

## Log tail (failed runs only)

For each failing run with a log URL in `link` or Checks API `details_url` / `output`:

1. Resolve workflow run id from the URL when present (`/actions/runs/<id>`).
2. Fetch job logs:

```bash
gh run view <run_id> --log-failed
```

Or when the workflow run id is unavailable, fetch check-run annotations as the bounded excerpt:

```bash
gh api repos/{owner}/{repo}/check-runs/{check_run_id}/annotations --paginate
```

Use annotation messages (and titles when present) as the log tail — do not require `{job_id}` or job log endpoints in this fallback path.

3. Apply cap: last **200 lines** or **16 KiB**, whichever is smaller.
4. Store in the markdown **body** below frontmatter as a fenced code block.
5. Set frontmatter `log_truncated: true` when the log excerpt is incomplete — source exceeds the cap, `link` is HTML-only with no log API path, or log fetch fails.

When `log_truncated: true`, persist the check record without a body tail if needed. At triage, treat capped, unavailable, and fetch-failed the same: set `validity: needs-context`; do not assert fix-in-code with high confidence — **unless** SonarQube Web API enrichment supplies enumerated issues for that check (`sonar_enriched: true`; see [sonarqube-enrichment](sonarqube-enrichment.md)). Defer to enrichment assessment before forcing `needs-context`.

Do not fetch logs for green checks.

## Upsert

For each failing check (or previously failing check now green), upsert `checks/<check-slug>.md` per [persistence-schema](persistence-schema.md).

Update `_session.md`:

- `last_checks_synced_at` — ISO timestamp after successful check sync
- `failing_check_count` — count of checks with failing `state` or `bucket` after this sync (exclude `status: resolved` local-only greens if re-sync marked them resolved)

## Errors

|Failure|Action|
|-|-|
|Network / rate limit|Report error; keep prior local check state|
|Partial check list|Note truncation; treat as **read-only** — keep prior local check state; do **not** create, update, or resolve check records (green detection and re-open transitions require a complete listing); do **not** update `last_checks_synced_at` or `failing_check_count`|
|Log fetch failure|Persist check record without body tail; set `log_truncated: true` and `validity: needs-context` at triage|
|Permission denied|Actionable message; no GitHub writes|

## Re-sync after publish

After local check fixes and publish ([commit-and-push](commit-and-push.md)), re-fetch checks silently (0 AskQuestion). Refresh GitHub-owned fields; update `failing_check_count` and `last_checks_synced_at`.
