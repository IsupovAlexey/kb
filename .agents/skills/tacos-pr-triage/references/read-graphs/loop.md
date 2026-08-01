# Pr-triage — loop mode

When invoke includes `loop` token or `_session.md` has `loop_mode: true`. On invoke with `loop` (not `AGENT_LOOP_WAKE_PR_TRIAGE`), reset per [loop-mode.md](../loop-mode.md) **Fresh loop invoke**. When `loop_stopped_reason` is non-null and the message omits `loop`, re-verify exit per **Resume after premature stop** before stopping.

## MUST read

- [loop-mode.md](../loop-mode.md) — merge-ready exit, settle, iteration, open automation baseline
- [single.md](single.md) — base sync + triage steps
- [fetch-and-sync.md](../fetch-and-sync.md) § Bot settle poll — mandatory on every loop poll
- [suppressed-review-comments.md](../suppressed-review-comments.md) — § Agent checklist (mandatory) on every wake and poll; thread-only sync is incomplete
- [gate-budget.md](../gate-budget.md) ## Loop — stuck CI boundary only

## During loop

- **MUST run** loop continuation after completing the main sync/triage pass per [single.md](single.md) even when single-pass would stop
- **MUST arm** `AGENT_LOOP_WAKE_PR_TRIAGE` (2m sleeper or `gh pr checks --watch`) before end turn when `awaiting_settle` is non-null — see [loop-mode.md](../loop-mode.md) § Bot settle poll arming
- **On `AGENT_LOOP_WAKE_PR_TRIAGE` and every bot-settle poll:** run [fetch-and-sync.md](../fetch-and-sync.md) § Bot settle poll — paginated `reviewThreads`, `reviews` (including each review's `comments`), issue `comments`, and [suppressed-review-comments.md](../suppressed-review-comments.md) § Agent checklist (mandatory). Emit the checklist step 7 summary line in chat before updating `_session.md` baseline fields.
- **Forbidden:** bot-settle or wake polls that query only `reviewThreads` (or check-only sync) and then set `automation_comment_count: 0` — suppressed low-confidence items have no thread; all threads resolved does not mean zero open automation.
- **Forbidden:** ending turn with "re-run `/tacos-pr-triage`" or "come back in N minutes" without an armed watcher
- On fresh `loop` invoke: reset loop counters; re-run bot settle when `last_pushed_at` matches PR head — do not inherit prior-chat settle
- **MUST NOT** set `loop_stopped_reason: merge-ready` while `awaiting_settle` is non-null or before full comment sync + bot settle complete with wall-clock verification
- **MUST NOT** set `loop_stopped_reason: merge-ready` when full sync finds open automation on GitHub or `automation_baseline_ids` grew vs `_session.md` — apply [loop-mode.md](../loop-mode.md) § Baseline change; reset `bot_settle_poll_count` to `0` and `bot_wait_started_at`; prior resolved threads or stale `automation_comment_count: 0` do not satisfy exit
- When automation baseline id set grows during bot settle with `bypass_mode: true`: bypass sweep same turn; reset timers; re-enter settle after push
- Evaluate merge-ready exit only after bot settle completes; settle (CI watch, automation quiet poll); increment `loop_iteration`; return to sync
- Arm `AGENT_LOOP_WAKE_PR_TRIAGE` before end turn during settle
- Stuck CI: **0–1** structured prompt at loop edge only

## Done when

- `loop_stopped_reason: merge-ready` after full sync and completed bot settle, or user `stop loop` recorded in `_session.md`
