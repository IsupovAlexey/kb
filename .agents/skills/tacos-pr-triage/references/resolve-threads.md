# Resolve threads

GitHub writes for reply and resolve **only after** [approval-gates](approval-gates.md) § Resolve picklist selection on the gated path. **One gate** — selection is approval. **MUST NOT** call `AskQuestion` again before or after each thread ([gate-budget](gate-budget.md)). Review threads only — issue comments and review summaries have no resolve API.

During [bypass-mode](bypass-mode.md) sweep, bypass invoke is approval for automation `gh` reply/resolve — no resolve picklist. **MUST** post a reply before resolve for every automation thread.

## Reply (REST)

For a selected inline thread, post a short reply on the top-level comment:

```bash
gh api -X POST \
  "repos/<owner>/<repo>/pulls/<number>/comments/<id>/replies" \
  -f body="<reply_body>"
```

Build `reply_body` per [commit-and-push](commit-and-push.md) § Reply commit link when `_session.md` has `last_pushed_commit` and `fix_state: applied`. Otherwise one factual line from `applied_summary` plus path/line when inline.

### Bypass refusal reply

When `bypass_mode: true` and `fix_state: skipped` (refused or non-applicable automation item), post a short factual reply — no commit link. One to three sentences: why the suggestion is not applied (incorrect, nit, already addressed, out of scope). Cite triage `validity`/`action` in plain language; do not quote gate severities. Example shape:

```text
Not applying this suggestion: <one-line rationale from notes>. Marking resolved — no code change for this thread.
```

**Prerequisite:** for `action: fix-in-code` items on the gated path, [commit-and-push](commit-and-push.md) publish gate MUST have completed (or `already-on-remote`) before reply/resolve. Bypass path: publish gate waived; refusal replies MAY run before or after autopublish when no code fix was applied for that item.

**Batch:** loop selected ids and reply+resolve — **no** `AskQuestion` inside the loop.

If reply fails, record in `notes` and **do not** resolve the thread.

## Resolve (GraphQL)

REST does not resolve threads. Use:

```bash
gh api graphql -f query='
mutation($id:ID!){
  resolveReviewThread(input:{threadId:$id}){ thread{ id isResolved } }
}' -F id=<thread_id>
```

On success: set record `status: resolved`, `resolved_at` (ISO-8601), `is_resolved_on_github: true`.

## Fetch query (sync)

When listing threads for sync, use GraphQL with **cursor pagination** on every connection. GitHub caps each page at 100 nodes — a single `first:100` call is never sufficient when `totalCount` exceeds 100 or `pageInfo.hasNextPage` is true.

### Query shape

```bash
gh api graphql -f query='
query(
  $owner:String!,$repo:String!,$num:Int!
  $threadsCursor:String,$reviewsCursor:String,$issueCommentsCursor:String
){
  repository(owner:$owner,name:$repo){
    pullRequest(number:$num){
      reviewThreads(first:100, after:$threadsCursor){
        totalCount
        pageInfo { hasNextPage endCursor }
        nodes{
          id isResolved isOutdated
          comments(first:100){
            totalCount
            pageInfo { hasNextPage endCursor }
            nodes{ databaseId author{login __typename} body path line originalLine createdAt updatedAt url }
          }
        }
      }
      reviews(first:100, after:$reviewsCursor){
        totalCount
        pageInfo { hasNextPage endCursor }
        nodes{
          id
          databaseId
          author{login __typename}
          state
          body
          submittedAt
          url
          comments(first:100){
            totalCount
            pageInfo { hasNextPage endCursor }
            nodes{
              databaseId
              author{login __typename}
              body
              path
              line
              originalLine
              createdAt
              updatedAt
              url
              isMinimized
              minimizedReason
            }
          }
        }
      }
      comments(first:100, after:$issueCommentsCursor){
        totalCount
        pageInfo { hasNextPage endCursor }
        nodes{ databaseId author{login __typename} body createdAt updatedAt url }
      }
    }
  }
}' -F owner=<owner> -F repo=<repo> -F num=<number>
```

Omit `-F` cursor fields on the first page (or pass empty string when `gh` requires the variable). Subsequent pages pass `endCursor` from the prior response as the matching `$…Cursor` variable.

### Pagination loop (mandatory)

For each connection (`reviewThreads`, `reviews`, issue `comments`):

1. Fetch with `after: null` (omit cursor).
2. Append `nodes` to an accumulator for that connection.
3. When `pageInfo.hasNextPage` is true, re-run with `after: pageInfo.endCursor` and repeat step 2.
4. Stop when `pageInfo.hasNextPage` is false.
5. When `totalCount` is present, verify `accumulator.length === totalCount` before treating sync as complete. On mismatch, report partial fetch and **do not** set `_session.md` `last_synced_at`.

**Nested thread comments:** when a thread's `comments.pageInfo.hasNextPage` is true, paginate that thread's comments with the same loop (per-thread `after` cursor) before upserting the thread record. Thread reply bodies beyond the first page are out of scope for tracked items ([fetch-and-sync](fetch-and-sync.md) tracks the first comment only) but full pagination prevents dropping threads whose first comment sits on a later page.

**Review comment pagination (mandatory):** for each review node in the accumulated `reviews` list, paginate that review's `comments` connection until `hasNextPage` is false. **Per-review cursors** — issue a separate GraphQL request per review `id` (node global id) or `databaseId` (nested query scoped to one review); do not advance per-review `comments` cursors inside the top-level PR query — that query's cursor variables apply to `reviewThreads`, `reviews`, and issue `comments` only. Verify `accumulator.length === totalCount` per review when `totalCount` is present. From the full comment set:

- Upsert review-thread first comments per [fetch-and-sync](fetch-and-sync.md) item 3 (when not already captured).
- Upsert minimized automation inline comments (`isMinimized: true`, `author_kind: automation`) per [fetch-and-sync](fetch-and-sync.md) item 4 when the comment `databaseId` is not already tracked.
- Parse and upsert suppressed low-confidence entries from automation review bodies per [suppressed-review-comments](suppressed-review-comments.md) — **mandatory** when the body matches a suppressed block marker; do not rely on item 4 or count mismatch alone.

**Merge rule:** upsert only after all required connections (nested thread comment pages, per-review comment pages, suppressed-body parse, and top-level connections) are fully fetched.

**Partial page:** when pagination stops early (rate limit, error, or unhandled `hasNextPage`), report `fetched N of totalCount` in chat, keep prior local state, and **do not** mark sync complete ([fetch-and-sync](fetch-and-sync.md) § Errors).

## Commit link fallback

When `last_pushed_commit` is unset and the user explicitly waived commit/push (`defer` at commit gate is **not** a waiver — that stops resolve), reply may reference `path:line` only and note the fix is local-only. Prefer completing [commit-and-push](commit-and-push.md) instead.

## Issue / review-summary items

No resolve mutation. After fix or reply-only disposition, update local `status` only; optional issue comment via separate user-approved `gh issue comment` is out of default scope — prefer thread reply path when `thread_id` exists.
