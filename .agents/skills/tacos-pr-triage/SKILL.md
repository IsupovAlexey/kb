---
name: tacos-pr-triage
description: Author-owned PR triage — sync checks/comments, fix with gates, publish/resolve. Invoke via /tacos-pr-triage (loop, bypass); not /tacos-apply-review or /tacos-pr.
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Optional PR URL/number; optional OpenSpec change id; optional loop to iterate until
  merge-ready; optional bypass to triage-gated automation autofix (combine loop bypass for babysit)
  (e.g. /tacos-pr-triage loop bypass, /tacos-pr-triage bypass).
---

# tacos pr triage

**You assist; the author decides.** Loop on the author's own open PR: sync → triage → gates → `gh`.

|Need|Skill|
|-|-|
|PR description sync|`/tacos-pr`|
|Reviewer diff walkthrough|`/tacos-assisted-review`|

## Entry

1. Parse invoke: `loop` / `bypass` tokens; read `_session.md` when present (`loop_mode`, `bypass_mode`, `loop_stopped_reason`, `awaiting_settle`). On `AGENT_LOOP_WAKE_PR_TRIAGE`, continue loop from session flags even when the wake message omits tokens. On invoke with `loop` (not a wake), reset loop counters per [loop-mode.md](references/loop-mode.md) **Fresh loop invoke**.
2. **Loop continuation (mandatory):** When `loop_mode: true` and `loop_stopped_reason` is null, **MUST NOT** end the turn after a single pass without [loop-mode.md](references/loop-mode.md) § Bot settle poll arming — arm `AGENT_LOOP_WAKE_PR_TRIAGE` background sleeper (2m) or active `gh pr checks --watch`. **Forbidden:** telling the user to manually re-run `/tacos-pr-triage` without an armed watcher.
3. **Merge-ready gate (loop):** Before `loop_stopped_reason: merge-ready`, run full comment sync per [fetch-and-sync.md](references/fetch-and-sync.md) § Bot settle poll — paginated `reviews` plus suppressed-body parse per [suppressed-review-comments.md](references/suppressed-review-comments.md) § Agent checklist (mandatory); not check-only or ad-hoc `reviewThreads`-only queries. **MUST NOT** declare merge-ready when sync finds open automation on GitHub or `automation_baseline_ids` grew vs `_session.md` — apply [loop-mode.md](references/loop-mode.md) § Baseline change (reset `bot_settle_poll_count` to `0` and `bot_wait_started_at`; when `bypass_mode: true` and the id set grew, bypass sweep same turn). Persisted `automation_comment_count: 0`, prior resolved threads, or "all threads resolved" do not satisfy exit.
4. Load mode graph (one hop):
   - loop + bypass — [read-graphs/loop.md](references/read-graphs/loop.md) + [read-graphs/bypass.md](references/read-graphs/bypass.md)
   - loop only — [read-graphs/loop.md](references/read-graphs/loop.md)
   - bypass only — [read-graphs/bypass.md](references/read-graphs/bypass.md)
   - default — [read-graphs/single.md](references/read-graphs/single.md)
5. During sync/triage load track graphs when active:
   - failing checks — [read-graphs/checks.md](references/read-graphs/checks.md); optional [read-graphs/sonar.md](references/read-graphs/sonar.md)
   - comment track — [read-graphs/comments.md](references/read-graphs/comments.md)

Config: `openspec/tacos.yaml` (`pr.descriptions_root` → `{descriptions_root}`; optional `pr.sonar_*`, `pr.loop_*`).

**Gate budget:** [gate-budget.md](references/gate-budget.md) — one structured prompt per gate; preview in chat first, then **end turn**.

## Quick start

|User says|Action|
|-|-|
|`/tacos-pr-triage`|Entry → single graph → sync → triage|
|`/tacos-pr-triage loop`|[read-graphs/loop.md](references/read-graphs/loop.md)|
|`/tacos-pr-triage bypass`|[read-graphs/bypass.md](references/read-graphs/bypass.md)|
|`/tacos-pr-triage loop bypass`|loop + bypass graphs|
|`/tacos-pr-triage 42 add-<feature>`|PR + change binding|

## Gate index

One structured prompt per row per [gate-budget.md](references/gate-budget.md); detail in [approval-gates.md](references/approval-gates.md):

|Gate id|When|Prompt count|
|-|-|-|
|Matrix root|Check matrix `needs-context`|0–1 confirm root|
|C0 Check fix|Code check failures|0–1 multi-select|
|C1 Check publish|After check fixes|1 when fixes applied|
|Post-triage routing|Automation-only open items|1 before fix path|
|Fix picklist|Comment fixes|1 multi-select|
|Publish|After local fixes|1 single-select|
|Resolve|After publish|1 multi-select → `gh` same turn|
|Stuck CI (loop)|Loop settle timeout|0–1 at loop edge only|

- `bypass` invoke approves full bypass sweep — zero prompts except matrix root confirm when `needs-context`
- `loop` invoke approves multi-iteration — zero start prompt; stuck-ci **0–1** at loop edge only

## Gating

Preview in chat → one structured prompt per gate → **end turn** → act on selection. Intent ≠ approval — [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md).

## Done when

- **Sync:** `_session.md` and artifacts under `{descriptions_root}/<branch-slug>/pr-triage/`
- **Gated single pass:** each applied gate got preview + one structured prompt
- **Bypass pass:** bypass sweep completed or user stopped
- **Loop:** merge-ready exit or user stopped; full comment sync on every bot settle poll; settle armed per [loop-mode.md](references/loop-mode.md); **MUST NOT** set `loop_stopped_reason: merge-ready` while `awaiting_settle` is set, before bot settle completes, or when sync finds open automation / baseline id set grew (Entry step 3)

## References

[gate-budget](references/gate-budget.md) · [check-sync](references/check-sync.md) · [fetch-and-sync](references/fetch-and-sync.md) · [suppressed-review-comments](references/suppressed-review-comments.md) · [dismissal-catalog](references/dismissal-catalog.md) · [bypass-mode](references/bypass-mode.md) · [loop-mode](references/loop-mode.md) · [approval-gates](references/approval-gates.md) · [output-paths](references/output-paths.md) · [change-binding](references/change-binding.md) · [approval-prompt](../tacos-orchestration/references/approval-prompt.md)
