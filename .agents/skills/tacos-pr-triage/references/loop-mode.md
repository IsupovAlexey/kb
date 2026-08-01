# Loop mode

Repeat the author-owned triage workflow until the PR is **merge-ready** or the user stops the loop. Uses host **loop** dynamic wake for long CI and slow automation re-reviews (Copilot, Bugbot, Claude). Combines with [bypass-mode](bypass-mode.md) — **`loop` + `bypass`** is the primary merge-ready babysit path (autofix automation and code checks each iteration; human reviewers keep picklists).

## Invoke

|Form|Example|
|-|-|
|Flag / token|`/tacos-pr-triage loop`|
|With bypass|`/tacos-pr-triage loop bypass`|
|With PR|`/tacos-pr-triage 42 loop`|
|Full babysit|`/tacos-pr-triage 42 loop bypass`|
|With change|`/tacos-pr-triage add-<feature> loop bypass`|
|Natural language|"triage PR in a loop until merge-ready", "babysit CI and bots until green"|

Set `_session.md` `loop_mode: true` on first loop invoke. On that first invoke, also set `loop_started_at` (ISO) and `loop_iteration: 0` per [persistence-schema](persistence-schema.md). **`loop` invoke is approval** to run multiple triage iterations until exit criteria or user stop — no separate "start loop?" gate.

**Session persistence:** On every invoke, read `_session.md` when it exists.

- **Continuation** (do not reset loop counters): `AGENT_LOOP_WAKE_PR_TRIAGE`; gate end-turn while `loop_mode: true` and `loop_stopped_reason` is null when the message omits `loop`; when `loop_mode: true` and `loop_stopped_reason` is null, **keep** `loop_mode: true` even if this turn's message omits `loop`.
- **Fresh loop invoke** (reset babysit): when the invoke includes the `loop` token and the turn is **not** `AGENT_LOOP_WAKE_PR_TRIAGE`, treat as a new babysit run — set `loop_mode: true` (and `bypass_mode: true` when `bypass` is present); reset `loop_iteration: 0`, `loop_started_at` (now), `loop_stopped_reason: null`, `awaiting_settle: null`, `settle_started_at: null`, `bot_settle_poll_count: 0`, `bot_wait_started_at: null`, `last_loop_wake_at: null`. Preserve `last_pushed_at` / `last_pushed_commit` when they match the current PR head — bot settle MUST run again for that push until exit criterion 4 is satisfied **this run** with wall-clock checks; do not inherit "prior push settled" from a prior chat.

**Resume after premature stop:** When `loop_mode: true`, `loop_stopped_reason` is non-null, and the invoke does **not** include `loop`, run full check sync + full comment sync ([fetch-and-sync](fetch-and-sync.md) § Bot settle poll) before honoring the stop reason. When exit criteria no longer hold, clear `loop_stopped_reason`, set `awaiting_settle` per [Settle phase](#settle-phase) when a push occurred or automation is unstable, and **continue the loop** — do not report merge-ready. When the invoke **includes** `loop`, follow **Fresh loop invoke** instead.

When **`bypass`** is also present, set `bypass_mode: true`. **`loop` + `bypass` invoke is approval** for multi-iteration bypass sweeps (code check autofix, automation autofix, autopublish, autoresolve) each iteration, plus settle waits between iterations.

## Open automation baseline

After every full comment sync, compute from persisted `comments/*.md` records (after [dismissal-catalog](dismissal-catalog.md) consult applies matched dispositions):

- `author_kind: automation`
- `status` not in (`resolved`, `deferred`, `wontfix`)
- `is_resolved_on_github: false`

`automation_comment_count` = count of those records. `automation_baseline_ids` = comma-separated sorted `id` values.

**Baseline change** — when either `automation_comment_count` or the id set differs from `_session.md` baseline fields:

1. Update `automation_comment_count` and `automation_baseline_ids` to the new values.
2. Reset `bot_wait_started_at` to now (ISO).
3. Reset `bot_settle_poll_count` to `0`.
4. **MUST NOT** set `loop_stopped_reason: merge-ready` on this turn — open automation or a grown id set means unsettled; quiet-period and minimum post-push wait restart from these resets.
5. When `bypass_mode: true` and the id set **grew** (new open automation): exit bot settle and run [bypass-mode](bypass-mode.md) sweep in the **same turn** (see [Bot settle poll arming](#bot-settle-poll-arming-mandatory) step 2).

## Exit criteria (merge-ready)

Stop the loop successfully when **all** hold **after** full check sync and full comment sync ([fetch-and-sync](fetch-and-sync.md) § Bot settle poll):

1. **Checks green** — no failing `bucket` / `state` on `gh pr checks` for the PR head (pending runs are not failures).
2. **No open actionable triage items** — no persisted comment or check records with:
   - `fix_state: pending` and `action: fix-in-code`, or
   - `status: open` with human `author_kind` and triage `action` other than `defer` / `resolve-without-code` still awaiting author disposition on the picklist path.
3. **No open automation on GitHub** — zero records matching [Open automation baseline](#open-automation-baseline).
4. **Bot settle complete** — `awaiting_settle` is null; when `last_pushed_at` is set for the current PR head, the agent MUST verify **this turn** with wall-clock elapsed time: `bot_settle_poll_count` ≥ **3**, elapsed since `last_pushed_at` ≥ `pr.loop_bot_wait_min`, and elapsed since `bot_wait_started_at` ≥ `pr.loop_quiet_period` with automation baseline unchanged across those polls. Persisted session fields alone MUST NOT satisfy this criterion without wall-clock verification.

Deferred, wontfix, resolved, and advisory-only infra/unknown checks do **not** block exit.

Report PR URL and iteration count; set `loop_stopped_reason: merge-ready` and clear `awaiting_settle`.

## One iteration

One loop iteration = one full triage pass per SKILL.md procedure (sync → checks track → comment triage → applicable gates) until the agent would normally **end the invoke**:

- User completes or skips all gates for that pass, **or**
- Report-only stall (advisory-only checks, no pending fix items), **or**
- **End turn** at a gate (next turn continues the **same** iteration — do not increment `loop_iteration` mid-gate).

Increment `loop_iteration` only when starting a **new** sync pass after settle/wait (see [Settle phase](#settle-phase)).

[gate-budget.md](gate-budget.md) applies per iteration. When `bypass_mode: true`, each iteration uses the bypass path for code checks and automation (zero picklists for those tracks); human comments keep gated picklists. Loop alone uses the full gated path each iteration.

## Settle phase

After an iteration produces a **push** (`last_pushed_at` updated) or when sync shows **pending/in-progress** checks and there is nothing to fix locally without waiting:

1. Set `awaiting_settle` on `_session.md`:
   - `checks` — CI still pending; bot wait not started
   - `reviews` — checks settled green; waiting for automation comment stability
   - `both` — checks pending and bot wait will follow push
2. Set `settle_started_at` (ISO).

### CI wait (dynamic wake)

When checks are pending or in-progress after publish or at iteration end:

```bash
gh pr checks <number> --watch --fail-fast
```

Omit `<number>` when the current branch has an open PR.

- **Primary wake:** watch exits when a check fails or all complete.
- On watch exit: re-sync checks silently; if still pending beyond [stuck-ci timeout](#stuck-ci-timeout), surface advisory and pause loop.
- When all checks report pass or explicit fail: proceed to bot wait if a push occurred this iteration; else evaluate exit or next iteration.

**Windows / PowerShell:** equivalent watch via repeated `gh pr checks --json name,bucket,state` polling when `--watch` is unavailable — use the same settle semantics with a 60–90s poll interval.

### Bot / automation re-review wait

After relevant checks are **not** pending (green or failed and assessed), and `last_pushed_at` is set this iteration:

1. Run **full** comment sync per [fetch-and-sync](fetch-and-sync.md) § Bot settle poll; run dismissal catalog consult; compute [Open automation baseline](#open-automation-baseline). When bot settle **starts**, record baseline in `automation_comment_count` / `automation_baseline_ids` and set `bot_wait_started_at` (ISO). Set `bot_settle_poll_count: 0`.
2. Poll comment sync on **`pr.loop_comment_poll_interval`** when set in `openspec/tacos.yaml`; else default **2m** (no `AskQuestion`). **Each poll MUST** run full comment sync (same section) — not ad-hoc thread queries. **Each poll MUST re-arm the next poll** (see [Bot settle poll arming](#bot-settle-poll-arming-mandatory)) until settle completes, bypass interrupts settle, or the user stops the loop.
3. **Quiet period:** automation baseline unchanged for **`pr.loop_quiet_period`** when set; else default **12m** of **wall-clock** time since `bot_wait_started_at` — not one poll. Require **`bot_settle_poll_count` ≥ 3** before quiet period can complete (never exit bot settle after a single poll).
4. **Minimum wait after push:** do not complete bot settle until **`pr.loop_bot_wait_min`** when set; else default **10m** has elapsed since `last_pushed_at`, even if the baseline looks stable — slow LLM reviewers (Copilot, Bugbot, Claude) often need several minutes on large PRs.
5. **New automation during bot settle:**
   - When baseline id set **grows** (per [Open automation baseline](#open-automation-baseline)): apply [Baseline change](#open-automation-baseline) rules.
   - When `bypass_mode: true` and baseline id set grew: **exit settle immediately** — clear `awaiting_settle`, run [bypass-mode](bypass-mode.md) sweep for open automation comments in the **same turn**; do **not** re-arm poll or wait for quiet period. After autopublish, re-enter settle from CI watch per [Settle phase](#settle-phase).
   - When `bypass_mode: false`: reset quiet-period clock; triage on the next iteration (normal gates).

Long-running LLM reviews may take **10–20+ minutes** on large diffs; prefer raising `pr.loop_quiet_period` (e.g. `20m`) over ending settle early.

### Bot settle poll arming (mandatory)

During bot settle, **every turn that ends while waiting** MUST arm the host **loop** skill dynamic wake **before** the agent ends the turn:

1. Run one poll now: full check sync per [check-sync](check-sync.md); full comment sync per [fetch-and-sync](fetch-and-sync.md) § Bot settle poll — including paginated `reviews` and [suppressed-review-comments](suppressed-review-comments.md) § Agent checklist (mandatory) (not satisfied by `reviewThreads`-only queries); upsert records; emit the suppressed-parse summary line in chat **only after** checklist step 7; run dismissal catalog consult per [dismissal-catalog](dismissal-catalog.md); recompute [Open automation baseline](#open-automation-baseline). Update `_session.md` (`last_synced_at`, `last_checks_synced_at`, `last_loop_wake_at`) only after full sync completes. Increment `bot_settle_poll_count`. Apply [Baseline change](#open-automation-baseline) when count or id set differs from stored baseline.
2. When `bypass_mode: true` and the automation baseline id set **grew** since stored baseline: **stop bot settle** — set `awaiting_settle: null`, run [bypass-mode](bypass-mode.md) sweep in the **same turn**; then continue loop per SKILL.md step 5 (CI settle when pushed). **Do not** re-arm poll. **Do not** set `loop_stopped_reason: merge-ready`.
3. Else evaluate whether bot settle may complete: `awaiting_settle: reviews`, `bot_settle_poll_count` ≥ **3**, elapsed ≥ `pr.loop_bot_wait_min` since `last_pushed_at`, elapsed ≥ `pr.loop_quiet_period` since `bot_wait_started_at` with baseline unchanged across those polls, checks green.
4. If settle **incomplete**, arm a one-shot background sleep for **`pr.loop_comment_poll_interval`** (default **2m**) that emits a wake whose prompt preserves active invoke flags — always `loop`; include `bypass` when `_session.md` `bypass_mode: true`:

```text
AGENT_LOOP_WAKE_PR_TRIAGE {"prompt":"/tacos-pr-triage loop bypass — continue bot settle: full comment sync, evaluate quiet period, re-arm 2m wake if incomplete"}
```

Use `notify_on_output` on `^AGENT_LOOP_WAKE_PR_TRIAGE`. Adapt sleep syntax to the host shell (bash `sleep` / PowerShell `Start-Sleep`). 5. End turn with a **short** status line only — include next wake in ~2m and that the loop is armed. **Do not** ask the user to manually re-run `/tacos-pr-triage` or "come back in N minutes" without an armed watcher.

On `AGENT_LOOP_WAKE_PR_TRIAGE`, execute the payload prompt in the same chat, then repeat from step 1.

**After bot settle completes** (step 3 passes): set `awaiting_settle: null`, then evaluate [Exit criteria (merge-ready)](#exit-criteria-merge-ready) — only then may `loop_stopped_reason: merge-ready` be set.

### CI watch arming

When using `gh pr checks --watch` (or PowerShell polling equivalent), prefer **blocking watch in a background shell** with `notify_on_output` on watch exit. If the agent must end turn while CI is still pending, arm the same `AGENT_LOOP_WAKE_PR_TRIAGE` sentinel at **60–90s** (CI poll) before ending turn — same mandatory rule as bot settle.

### Fallback heartbeat

When neither inline watch nor the 2m poll sleeper can be armed (host limitation), use a one-shot background sleep per host **loop** skill (lean long — default **10m**) with `AGENT_LOOP_WAKE_PR_TRIAGE`. This is a **last resort** — bot settle normally uses the 2m poll interval, not 10m.

## Loop procedure (after normal triage steps)

When `loop_mode: true`, after SKILL.md Procedure steps 1–5 (or when a single-pass invoke would stop):

1. **Settle first when required** — when `awaiting_settle` is non-null, or `last_pushed_at` matches the current PR head and bot settle is incomplete per criterion 4 wall-clock checks, run [Bot settle poll arming](#bot-settle-poll-arming-mandatory); **do not** evaluate merge-ready yet.
2. **Evaluate exit** — only when bot settle is complete (or no push pending settle): if merge-ready per [Exit criteria (merge-ready)](#exit-criteria-merge-ready), stop.
3. **Settle** — if last publish this iteration or checks pending with no local fix path → [Settle phase](#settle-phase); **end turn only after** arming watch/poll per [Bot settle poll arming](#bot-settle-poll-arming-mandatory) (0 `AskQuestion` during watch/poll).
4. **Stuck CI** — if `settle_started_at` + stuck-ci timeout exceeded with checks still pending → [Stuck CI timeout](#stuck-ci-timeout) (one gate).
5. **Next iteration** — increment `loop_iteration`; run sync from step 1; repeat.

**User stop:** phrases `stop loop`, `stop triage loop`, or killing the host loop watcher → set `loop_stopped_reason: user-stop`; report status.

## Stuck CI timeout

Default **45 minutes** (`pr.loop_stuck_ci_timeout` when set; else `45m`) from `settle_started_at` while checks remain pending/in-progress.

|Outcome|Action|
|-|-|
|Timeout fires|Emit short **Stuck CI** advisory in chat; **one** gate per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): **next tool call MUST** be structured prompt when tools available (`continue-wait` / `stop-loop`); plain-text only when tools absent|
|`continue-wait`|Reset `settle_started_at`; re-arm watch|
|`stop-loop`|Set `loop_stopped_reason: stuck-ci`; stop|

## What loop does not change

- Author-ownership, preflight, and persistence rules unchanged
- Infra/unknown checks remain report-only (bypass does not autofix them)
- Human comments keep full [approval-gates](approval-gates.md) each iteration when open
- **Optional Task delegation** — when Task spawn is supported, parent **MAY** delegate read-only fetch-and-sync scouting or bounded automation fix-application iterations to host subagents; parent **MUST** retain human comment approval gates, loop arming, merge-ready exit, and bot settle authority ([fetch-and-sync](fetch-and-sync.md) § Bot settle poll). Human gate prompts **MUST NOT** be delegated to children.

### Optional loop delegation (Task supported)

|Surface|Parent MAY delegate|Parent MUST retain|
|-|-|-|
|Fetch/sync scout|Read-only child scoped to comment/check sync procedure before merge-ready evaluation|Merge-ready rules, bot settle arming, `automation_baseline_ids` updates|
|Bounded fix iteration|Child scoped to fix iteration diff + review artifact paths for automation suggestions|Human comment [approval-gates](approval-gates.md); loop stop / `loop_stopped_reason`; publish gate for human threads|

When Task is unavailable, parent runs triage inline per existing loop procedure.

## Forbidden during loop

- Skipping gates on **human** comments because a later iteration will run
- Treating pending checks as failures in sync ([check-sync](check-sync.md))
- Re-triage immediately after push without settle (bots and CI need time)
- Extra `AskQuestion` per iteration beyond normal gate budget (except stuck-ci row)
- **Ending bot settle or CI wait with only a chat status report** — telling the user to "re-run `/tacos-pr-triage` in N minutes" or "come back later" **without** arming `AGENT_LOOP_WAKE_PR_TRIAGE` (or an active `gh pr checks --watch` / 2m poll sleeper)
- **Completing bot settle after one poll** or one status check — need `bot_settle_poll_count` ≥ 3 plus `pr.loop_bot_wait_min` and `pr.loop_quiet_period` wall-clock
- **Re-arming bot settle poll when `bypass_mode: true` and automation baseline id set grew** — run [bypass-mode](bypass-mode.md) sweep in the same turn instead
- **Setting `loop_stopped_reason: merge-ready` while `awaiting_settle` is non-null** or before full comment sync and bot settle complete
- **Declaring merge-ready from ad-hoc thread queries** or check-only polls without [fetch-and-sync](fetch-and-sync.md) § Bot settle poll and [suppressed-review-comments](suppressed-review-comments.md) § Agent checklist (mandatory)
- **Emitting the suppressed-parse summary line without completing** [suppressed-review-comments](suppressed-review-comments.md) § Agent checklist (mandatory) or without upserting `comments/suppressed-*.md` when markers were present
- **Skipping bot settle on fresh loop invoke** because a prior chat declared merge-ready or session counters look complete
- **Declaring "prior push settled"** without wall-clock verification against `last_pushed_at` and `bot_wait_started_at` this turn
- **Dropping `loop_mode` or stopping the loop** when a full sync finds new open automation — reset baseline timers and continue (bypass sweep when `bypass_mode: true`)
