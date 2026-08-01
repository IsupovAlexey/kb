# Suppressed review comments (Copilot low confidence)

GitHub Copilot often emits low-confidence inline feedback **only** inside the automation review summary body — not as `reviewThreads` nodes and not as `PullRequestReviewComment` nodes with `isMinimized: true`.

Typical shapes in `reviews.nodes[].body` (Copilot has used both summary labels):

```html
<details>
<summary>Suppressed comments (2)</summary>

**<skill-root>/foo.md:42**
* Suggestion text here.

**src/bar.cs:10**
* Another suggestion.
</details>
```

Legacy label (still parse when present):

```html
<summary>Comments suppressed due to low confidence (2)</summary>
```

**MUST parse and upsert** these during every full comment sync ([fetch-and-sync](fetch-and-sync.md) item 5). Reporting `needs-context` on count mismatch alone is **not** sufficient when the `<details>` block is present.

## Agent checklist (mandatory)

Execute **in order** on every invoke, loop wake, and bot-settle poll **before** triage, bypass sweep, merge-ready evaluation, or emitting the suppressed-parse summary line. Emitting the summary line without completing this checklist is a **sync failure** — do not update `last_synced_at`, automation baseline fields, or `loop_stopped_reason`.

1. **Paginate `reviews`** — fetch every page until `hasNextPage` is false; accumulated count MUST match `totalCount` when present ([resolve-threads](resolve-threads.md) § Pagination loop).
2. **Filter automation reviews** — keep every review whose author matches [automation-detection](automation-detection.md); record `automation-review-count` = that list length (includes multiple reviews from the same bot on different commits).
3. **Fetch each automation `body` in full** — REST `gh api repos/{owner}/{repo}/pulls/{number}/reviews/{review_id}` when GraphQL truncates or the agent did not retain the body. **MUST** include post-push re-reviews even when `comments.nodes` is empty or the overview says `generated no new comments`.
4. **Scan every body** — case-insensitive match for [suppressed block markers](#suppressed-block-markers). Do **not** stop after the first Copilot review or the first page of results.
5. **Parse and upsert** — for each matching body, run [Parse procedure](#parse-procedure); upsert `comments/suppressed-<reviewDatabaseId>-<index>.md` per [Upsert fields](#upsert-fields) and [Dedupe](#dedupe-mandatory).
6. **Verify counts** — when the marker line reports `(N)`, confirm parsed entries equal `N` or report `parsed M of N` with review URL.
7. **Emit summary** — only after steps 1–6:

```text
Suppressed low-confidence: <parsed> parsed from <automation-review-count> automation review(s)
```

When step 4 finds a marker and step 5 parsed zero entries → **STOP** (sync failure); re-parse — do not triage or declare merge-ready.

### Forbidden shortcuts

|Shortcut|Why it fails|
|-|-|
|Query `reviewThreads` only (or checks-only) on bot-settle polls|Suppressed items have no `reviewThread`|
|Set `automation_comment_count: 0` because all threads are resolved|Resolved threads ≠ zero open automation; body-only suppressed items remain|
|Infer `parsed: 0` from `generated N comments` or non-minimized inline counts|Suppressed block is separate from visible inline comments|
|Scan only the earliest automation review|Copilot often posts a **new** review per push with `Suppressed comments (N)` and zero `comments` nodes|
|Print the summary line without upserting `comments/suppressed-*.md`|Ritual output; sync did not run|

## Suppressed block markers

Case-insensitive body match when **any** of these substrings appears:

- `Suppressed comments` — current Copilot label (`<summary>Suppressed comments (N)</summary>`)
- `Comments suppressed due to low confidence` — legacy Copilot label

When both appear in one body, parse a single block starting at the first match.

Extract expected count `N` from the parenthesized number on the same line as the matched marker (e.g. `Suppressed comments (5)` or `Comments suppressed due to low confidence (5)`).

## When to run

After paginating `reviewThreads`, `reviews` (including per-review `comments`), and issue `comments` per [resolve-threads](resolve-threads.md):

1. Filter `reviews` to automation authors per [automation-detection](automation-detection.md).
2. For each review whose body matches a [suppressed block marker](#suppressed-block-markers), extract suppressed entries (below).
3. Upsert one record per extracted entry.
4. Verify extracted count equals `N` from the marker line when `N` is present. On mismatch after parse, report `parsed M of N suppressed` with review URL — do **not** treat sync as failed if at least one entry parsed.

## Parse procedure

Within each matching review body:

1. Locate the suppressed block — from the first [suppressed block marker](#suppressed-block-markers) through the closing `</details>` when present; otherwise through the next `##` heading or end of body.
2. Split into entries — each entry starts at a path header line:
   - `**<path>:<line>**` (preferred), or
   - `**<path>**` (line unknown).
3. Body text — the bullet line(s) immediately after the path header, usually `* <text>`; strip leading `* `.
4. Optional code fence after the bullet — ignore fenced blocks for `body`; they are context snippets, not the suggestion.

Skip entries whose path header or body is empty after trim.

## Synthetic record id

When GitHub provides no `databaseId`:

```text
suppressed-<reviewDatabaseId>-<index>
```

`<index>` — zero-based order within that review's suppressed block (top to bottom).

Filename: `comments/suppressed-<reviewDatabaseId>-<index>.md`.

## Upsert fields

|Field|Value|
|-|-|
|`id`|synthetic id above|
|`type`|`inline`|
|`source`|`review_suppressed`|
|`author`|review author login|
|`author_kind`|`automation`|
|`is_minimized`|`true`|
|`minimized_reason`|`low-confidence` (literal when API omits `minimizedReason`)|
|`review_id`|parent review `databaseId` as string|
|`thread_id`|`null` unless dedupe links an existing thread (below)|
|`path` / `line`|from path header|
|`url`|parent review `url`|
|`is_resolved_on_github`|`false` (no thread until GitHub surfaces one)|
|`status`|`open` on first upsert|

Body below frontmatter: parsed suggestion text (bullet content only).

## Dedupe (mandatory)

Before creating a synthetic record, check existing tracked comments (same sync pass):

|Match|Action|
|-|-|
|Same `databaseId` already tracked|Skip synthetic duplicate|
|Same `path` + `line` + body prefix ≥ 40 chars on an open automation inline record|Skip synthetic; refresh GitHub-owned fields on the existing record|
|Same `path` + `line` on an open automation thread whose first-comment body shares the same prefix|Skip synthetic; set `notes: deduped to thread <thread_id>` on thread record if helpful|
|Synthetic record exists for same `review_id` + `index` but body changed|Refresh body; preserve local triage unless body materially changed (then null triage fields)|

**Never** drop a suppressed entry solely because a different open thread exists on the same file — only skip when body content clearly matches.

## Triage and bypass

Include `source: review_suppressed` records in automation triage and [loop-mode](loop-mode.md) § Open automation baseline the same as thread-backed automation comments.

## Sync report line (mandatory)

Every full sync MUST emit before triage (see [fetch-and-sync](fetch-and-sync.md) § Sync completion):

```text
Suppressed low-confidence: <parsed> parsed from <automation-review-count> automation review(s)
```

Zero parsed with zero automation reviews scanned is valid. Zero parsed when a review body contains a [suppressed block marker](#suppressed-block-markers) is a sync failure — re-parse before triage.

|Disposition|GitHub writes|
|-|-|
|`fix-in-code` applied|Autopublish per [bypass-mode](bypass-mode.md); if a matching open `thread_id` exists, reply + resolve that thread; else set local `status: resolved` and list in sweep report under **Suppressed (no thread)**|
|`resolve-without-code` / already fixed|Same — prefer thread reply when `thread_id` linked; else local resolve only|
|`invalid-or-nit`|Refusal reply on thread when `thread_id` set; else local `status: resolved` with `notes`|

Report suppressed items in triage output:

```text
- [suppressed-4730456549-0] author_kind=automation source=review_suppressed suppressed — <summary>
```

## Merge-ready

Open `source: review_suppressed` records with `status: open` and triage `action` other than `defer` / `resolve-without-code` **block** loop merge-ready the same as thread-backed automation items.
