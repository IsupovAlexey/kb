# Bypass mode

Bypass invoke removes picklists for automation and code checks — it does **not** remove triage judgment. The agent MUST complete comment triage (validity, severity, action) and persist results **before** the bypass sweep. Local autofix applies only to automation items triaged `valid` with `action: fix-in-code` after reading the referenced code; incorrect, nitty, or non-applicable bot suggestions MUST be refused (no local edit), replied to on GitHub with a factual rationale, then resolved. Bypass also autofixes eligible **code** failing checks. Human reviewer comments keep the normal [approval-gates](approval-gates.md) picklists. **Infra** and **unknown** check failures remain report-only — bypass does not autofix them.

## Invoke

|Form|Example|
|-|-|
|Flag / token|`/tacos-pr-triage bypass`|
|With PR and change|`/tacos-pr-triage 42 add-<feature> bypass`|
|Natural language|"triage PR comments bypass bots"|

Set `_session.md` `bypass_mode: true`. **`bypass` invoke is approval for the entire bypass sweep** — check autofix (code only), automation autofix, scoped `git commit`, `git push`, and `gh` reply/resolve on automation threads. **Zero `AskQuestion` calls** while `bypass_mode: true` except matrix root confirm when a check cluster root is `needs-context`.

With **`loop`**, keep `bypass_mode: true` across iterations — each iteration runs the bypass sweep before settle; see [loop-mode](loop-mode.md).

## Order

1. Sync checks and comments; run [dismissal-catalog](dismissal-catalog.md) consult after upsert; assess checks and **triage open automation comments** per [triage-rubric](triage-rubric.md) — skip re-triage for catalog-matched refused items (report **Check failures** and comment triage). Persist `validity`, `severity`, and `action` on each record before step 2.
2. Run the bypass sweep below — no check fix picklist, check publish gate, comment fix picklist, publish gate, or resolve picklist for automation or code checks.

## Triage discipline (mandatory before sweep)

- Automation comments are **not** presumed correct — apply [triage-rubric](triage-rubric.md) § Automation skepticism (default) before any `fix-in-code`.
- MUST read referenced code and change context before assigning `action: fix-in-code` to automation items.
- MUST NOT assign `fix-in-code` when `validity` is `invalid-or-nit` or the suggestion is incorrect or too nitty for this change.
- Label bot style nits and wrong suggestions `invalid-or-nit` with `reply-only` or `resolve-without-code` — not `fix-in-code`.
- `needs-context` — skip local edit in sweep; disposition per triaged `action` (reply/resolve only).
- Bypass approval is approval to **execute triage outcomes** without picklists — not approval to apply every bot suggestion in code.

## Bypass sweep

When `bypass_mode: true`, after assessment, triage report, and persisted triage fields:

1. **Check autofix** — for each failing check with `failure_kind: code`, `matrix_role` in (`root`, `standalone`), and `action: fix-in-code`: apply the implied fix locally; set `fix_state: applied` and `applied_summary`. Skip `infra`, `unknown`, and dependents. On failure: set `notes` with the failure reason.
2. **Automation select** — all open records matching [loop-mode § Open automation baseline](loop-mode.md#open-automation-baseline) (`author_kind: automation`, `is_resolved_on_github: false`, actionable status), including `source: review_suppressed` per [suppressed-review-comments](suppressed-review-comments.md). Requires accurate [automation-detection](automation-detection.md) at sync and suppressed-body parse on sync.
3. **Automation autofix** — for each automation item with `validity: valid` **and** `action: fix-in-code`: apply the implied fix locally; set `fix_state: applied` and `applied_summary`. Items with `validity: invalid-or-nit`, `needs-context`, `reply-only`, `resolve-without-code`, or `defer`: skip local edit; set `fix_state: skipped` with a one-line `notes` reason (why refused or not applicable). On failure: set `status: wontfix`, `fix_state: skipped`, and `notes` (failure reason). After each refused automation item, upsert [dismissal-catalog](dismissal-catalog.md) per write triggers.
4. **Autopublish** — when check or automation autofix touched repo files and changes are not on `origin`: `git add` scoped paths only → `git commit` with a short imperative message (e.g. `fix: address automation review`) → `git push` to the PR branch → set `_session.md` `last_pushed_commit` / `last_pushed_at`. **No** [commit-and-push](commit-and-push.md) publish gate — bypass invoke already approved commit and push.
5. **Re-fetch checks** — silent refresh per [check-sync](check-sync.md) after autopublish when check autofix ran.
6. **Autoresolve** — for each automation item with `thread_id`: post reply per [resolve-threads](resolve-threads.md) — commit link + `applied_summary` when `fix_state: applied`; refusal rationale when `fix_state: skipped` — then `resolveReviewThread`. For `source: review_suppressed` without `thread_id`: set local `status: resolved` after disposition per [suppressed-review-comments](suppressed-review-comments.md). Issue/review-summary automation: update local `status` only when no thread resolve API exists.
7. **Report** — counts: checks autofixed, automation autofixed, automation refused/skipped (with reasons), automation resolved, suppressed-without-thread resolved locally; commit sha when pushed.

Then continue with human comments only when any `author_kind: human` open items remain. When only automation and checks were open: when `loop_mode: false`, stop after the sweep report; when `loop_mode: true`, run SKILL.md step 5 ([loop-mode](loop-mode.md)) — evaluate merge-ready, settle when pushed or checks pending, then next iteration.

## What bypass does not change

- Sync, triage, and `gh` preflight still run
- **Infra** and **unknown** check failures — report and advisory hints only; no autofix
- **Human** open comments after the sweep still use full [approval-gates](approval-gates.md) (fix picklist → publish gate → resolve picklist)
- Author-ownership rules unchanged

## Forbidden during bypass sweep

- Check fix picklist, check publish gate, or comment picklists for automation or code checks
- Any `AskQuestion` for automation (publish, resolve, routing, fix picklist)
- Autofix for **infra** or **unknown** checks
- Showing publish preview then asking — autopublish immediately after autofix
- `approval-prompt.md` GitHub double-ask for automation threads
- Entering automation autofix before comment triage is complete and persisted
- Marking automation `fix-in-code` without reading referenced code
- Applying local edits for `invalid-or-nit`, incorrect, or nitty bot suggestions because bypass was invoked
- Resolving automation threads without a reply (applied fix summary or refusal rationale)
