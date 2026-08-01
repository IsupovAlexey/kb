# Fetch and sync

**gh-first** discovery and comment listing. No automatic re-sync on HEAD movement.

## Preflight

When [check-sync.md](check-sync.md) step 1a already ran `gh auth` this invoke, skip — proceed to Discover PR. Otherwise run `gh auth status` per check-sync § Preflight.

## Discover PR

Run in parallel when useful:

```bash
git branch --show-current
gh api user --jq '.login'
gh repo view --json nameWithOwner --jq '.nameWithOwner'
```

Resolve PR (current branch unless user passed URL/number):

```bash
gh pr view [--json number,title,url,state,author,headRefName,baseRefName]
```

- No PR → report and stop.
- Author login ≠ current user → stop unless user explicitly targets a PR they own.
- `state != OPEN` → warn; read-only (no fix/resolve writes).

Write or update `_session.md` ([output-paths](output-paths.md), [persistence-schema](persistence-schema.md)).

## Sync checks (before comments)

Check sync is **step 1a** in [SKILL.md](../SKILL.md) — run [check-sync](check-sync.md) once per invoke **before** this document's comment fetch (step 1b). When step 1a already completed `gh auth status` preflight this invoke, this document's preflight is a no-op — proceed to Discover PR. Do **not** call check sync again from this reference when step 1a already completed in the same invoke. Update `_session.md` `last_checks_synced_at` and `failing_check_count` during step 1a only. When all checks are green and no failing records exist, skip log fetches but still refresh check status summary on re-sync.

## Fetch comments

Prefer `gh` and `gh api` for listing. For review-thread `isResolved` / `isOutdated`, use GraphQL when REST lacks fields ([resolve-threads](resolve-threads.md) § Fetch query).

**Pagination (mandatory):** follow [resolve-threads](resolve-threads.md) § Pagination loop for `reviewThreads`, `reviews` (including each review's `comments` connection), and issue `comments`. A single `first:100` response is a partial page — keep fetching while `pageInfo.hasNextPage` is true. Set `_session.md` `last_synced_at` only when every connection is complete and accumulated `nodes.length` matches `totalCount` when available.

**Tracked top-level items:**

1. Issue comments on the PR
2. Non-empty review summary bodies
3. First comment of each review thread (not replies)
4. Minimized inline comments from automation reviews — paginate each `PullRequestReview.comments` connection per [resolve-threads](resolve-threads.md) § Review comment pagination; upsert when `isMinimized: true` and `author_kind: automation` per [automation-detection](automation-detection.md); dedupe by comment `databaseId` when the same comment already appears on a review thread
5. **Suppressed low-confidence comments** in automation review bodies — parse and upsert per [suppressed-review-comments](suppressed-review-comments.md); **mandatory** when the body matches a suppressed block marker (`Suppressed comments` or legacy `Comments suppressed due to low confidence`); dedupe against thread and inline records by path, line, and body prefix

Filter to inbound feedback relevant to the author (exclude the author's own top-level comments). Set `author_kind` on each record per [automation-detection](automation-detection.md).

## Upsert

For each item, upsert `comments/<id>.md` per [persistence-schema](persistence-schema.md). Update `_session.md` `last_synced_at`.

## Sync completion (mandatory before triage)

After upsert and [dismissal-catalog](#dismissal-catalog-consult-mandatory-after-upsert) consult:

1. Complete [suppressed-review-comments](suppressed-review-comments.md) § Agent checklist (mandatory) — scan **every** paginated automation `reviews[].body`; thread-only or inline-only sync is incomplete without this step.
2. Emit one sync summary line in chat before triage or bypass sweep (checklist step 7 output):

```text
Suppressed low-confidence: <parsed> parsed from <automation-review-count> automation review(s)
```

When the body reports `(N)` and parsed count differs, append `(parsed M of N — see <review url>)`.

3. When `parsed` is 0 but any automation review body contains a [suppressed block marker](suppressed-review-comments.md#suppressed-block-markers), STOP and re-parse — do not proceed to triage.

## Dismissal catalog consult (mandatory after upsert)

After upsert completes and before triage picklists or bypass sweep:

1. Read `_dismissal-catalog.md` when present ([dismissal-catalog](dismissal-catalog.md)).
2. Match open automation records with unset triage or re-opened records with cleared triage against catalog keys (path + line + body prefix).
3. Persist matched dispositions on `comments/<id>.md`; include `(catalog match)` in the triage report.

Applies on every invoke, loop wake, and bot-settle poll — not only first sync.

## Bot settle poll (loop mode — mandatory)

When [loop-mode](loop-mode.md) bot settle runs a poll or evaluates merge-ready exit, run this document's **full** comment fetch (paginated GraphQL per [resolve-threads](resolve-threads.md) § Pagination loop) **before** comparing automation baselines or declaring merge-ready.

- Ad-hoc unresolved-thread-only queries, single-page `first:100` fetches, or REST-only listing **do not** satisfy sync — they MUST NOT update `last_synced_at`, `automation_comment_count`, or `automation_baseline_ids`. **All review threads resolved** is not a substitute — Copilot low-confidence items often exist only inside `reviews[].body` (`source: review_suppressed`) with no `reviewThread`. After each push, Copilot may add a **new** automation review with empty `comments` and a `Suppressed comments (N)` block in `body` — MUST scan every automation review per [suppressed-review-comments](suppressed-review-comments.md) § Agent checklist (mandatory).
- Upsert every tracked item; refresh `author_kind` per [automation-detection](automation-detection.md) on each poll.
- Run [Dismissal catalog consult](#dismissal-catalog-consult-mandatory-after-upsert); then compute open automation per [loop-mode](loop-mode.md) § Open automation baseline.

## Re-sync (optional)

After local fixes, offer: "Re-sync from GitHub before resolve?" User may skip. Re-sync refreshes GitHub-owned fields only.

## Errors

|Failure|Action|
|-|-|
|Network / rate limit|Report error; keep prior local state|
|Partial GraphQL page|Report `fetched N of totalCount`; keep prior local state; do not set `last_synced_at`|
|Permission denied|Actionable message; no GitHub writes|
