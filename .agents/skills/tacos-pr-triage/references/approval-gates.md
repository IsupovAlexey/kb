# Approval gates

**Gate budget:** [gate-budget.md](gate-budget.md) — **one `AskQuestion` per gate**; forbidden duplicate prompts.

**Intent ≠ approval.** Structured prompts per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) when the runtime supports them ([interview-prompt.md](../../tacos-grill/references/interview-prompt.md)). Generic [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md) GitHub rules **do not** add a second resolve approve after resolve picklist.

[bypass-mode](bypass-mode.md), when `bypass_mode: true`, runs the bypass sweep **after** check assess and comment triage report, **before** comment gated picklists: autofix **code** check failures + triage-gated automation autofix (local edit only `valid` + `fix-in-code`; refused items get refusal reply + resolve), autopublish, re-fetch checks, autoresolve automation threads with no picklists (`bypass` invoke or `bypass-sweep` routing is approval). Infra and unknown check failures stay report-only. Otherwise automation items use the same gated picklists as human comments when the user chooses [Fix picklist](#fix-picklist) (via routing or mixed PR).

## Runtime (structured ask)

|Runtime|Tool|
|-|-|
|Cursor|`AskQuestion`|
|Claude Code|`AskUserQuestion`|
|Other / unavailable|Plain text — [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable|

Structured selection is the answer. Silence, implied consent, or chat prose without a picklist selection does not advance the workflow.

## Multi-select picklist ordering

Every `allow_multiple: true` picklist (**fix**, **resolve**) **MUST** list options in this order:

1. **`all-actionable`** — first row; label includes count, e.g. `All actionable items (7)`
2. **Per-item rows** — one option per eligible comment/thread (`<comment-id>`)
3. **`none`** — last row

On `all-actionable`: treat as selecting every per-item row currently shown (same ids as rows 2…n−1). Do not implement items excluded from the picklist (e.g. `fix_state` not `pending`, `action: defer`, review-summary without a thread).

**Never counts as picklist selection:** prose such as “fix all P1” without choosing `all-actionable` or explicit per-item ids in structured UI.

## Post-triage routing (mandatory when `bypass_mode: false`)

After the **comment** triage report (SKILL.md step 4), **before** any local comment fix, GitHub write, or comment workflow branch:

When `bypass_mode: true`, **skip** this section — run [bypass-mode](bypass-mode.md) sweep directly (zero `AskQuestion` for automation).

When `bypass_mode: false`:

1. When structured tools are available, the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) and the table below — **forbidden:** prose menu in chat. When unavailable, use plain-text fallback per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable — **do not** skip the gate.
2. **Forbidden:** `Next steps` / `What would you like to do?` walls; invented options (e.g. resolve-without-code to ignore planning history); prose bulk intent without structured `all-actionable` or per-item selection.

|Condition|Gate|
|-|-|
|Open human and/or automation with `fix_state: pending`; `bypass_mode: false`; at least one human item|[Fix picklist](#fix-picklist) — all pending items (`allow_multiple: true`)|
|Open `author_kind: automation` only; `bypass_mode: false`|[Automation routing](#automation-routing) — single-select, then branch|
|`bypass_mode: true` after comment triage|Run [bypass-mode](bypass-mode.md) sweep (step 7b); no picklists for automation|
|No open items|When `loop_mode: false`: short status; stop. When `loop_mode: true`: proceed to SKILL.md step 5 ([loop-mode](loop-mode.md)) — do not stop after one pass.|

**End turn** after every routing gate until the user selects an option (plain-text fallback: same ids; wait for next message).

### Automation routing

When triage finds open automation items and no human item needs [Fix picklist](#fix-picklist) this pass.

**Prompt:** `Only automation comments are open. How should we proceed?`

|id|label|
|-|-|
|`fix-picklist`|Gated workflow — fix picklist → publish gate → resolve picklist|
|`bypass-sweep`|Run automation bypass sweep (triage-gated autofix + refusal replies + autoresolve; no picklists)|
|`defer`|Stop this pass — no local fixes or GitHub writes|

On `fix-picklist`: proceed immediately to [Fix picklist](#fix-picklist) with all open automation items (`fix_state: pending`). **End turn** until multi-select completes.

On `bypass-sweep`: set `_session.md` `bypass_mode: true`; run [bypass-mode](bypass-mode.md) with **no further** `AskQuestion` for automation (equivalent to `/tacos-pr-triage bypass` invoke).

On `defer`: **end turn**.

**Forbidden:** prose-only menus when structured tools are available; skipping [Fix picklist](#fix-picklist) when the user chose `fix-picklist`.

## Gate summary (gated path)

|Step|GitHub write|Gate|
|-|-|-|
|Sync checks / comments|No|None|
|Check assess + matrix|No|None|
|Matrix root confirm|No|**0–1** when needs-context|
|Check fix picklist|No|**0–1** multi-select (code only)|
|Check publish|Yes (`git commit`, `git push`)|**1** single-select when check fixes applied|
|Re-fetch checks|No|**0**|
|Bypass automation sweep|Yes (automation only)|**0** prompts — autopublish + autoresolve; see [bypass-mode](bypass-mode.md)|
|Fix picklist|No|**1** multi-select|
|Ambiguous fix|No|**1** single-select per ambiguous item only|
|Publish gate|Yes (`git commit`, `git push`)|**1** single-select — diff review in preview, not a separate gate — [commit-and-push](commit-and-push.md)|
|Resolve picklist|Yes (`gh` reply + resolve)|**1** multi-select — selection executes `gh`; no follow-up approve|

## Check fix picklist

Runs after check assessment and optional matrix root confirm ([check-matrix](check-matrix.md)). **MUST NOT** edit repository files for a gated check until the user selects that check in this gate.

**Eligible rows:** persisted checks with `failure_kind: code`, `matrix_role` in (`root`, `standalone`), `fix_state: pending`, `action: fix-in-code`. Omit infra, unknown, dependents, and deferred clusters.

1. Show Check failures summary (or reference persisted `checks/*.md`).
2. Call structured prompt **once** with `allow_multiple: true`.

**Prompt:** `Which failing checks should I fix locally?`

|Order|id|label|
|-|-|-|
|1|`all-actionable`|`All actionable checks (<n>)` — every eligible row below|
|2…|`<check-slug>`|`[<check-slug>] <name> — <short summary>`|
|last|`none`|None — skip check fixes this pass|

3. **End turn** on plain-text fallback until selection.
4. On `all-actionable` or per-slug selection: implement only those checks; update `fix_state` and `applied_summary` on records.
5. On `none`: when any eligible check already has `fix_state: applied`, proceed to [Check publish](#check-publish); otherwise skip check fixes and go to comment triage.

Infra and unknown checks MUST NOT appear as picklist rows.

## Check publish

After check fix implementations. Same semantics as [Publish gate](#publish-gate) — diff preview in chat, **one** `AskQuestion`, then commit+push on approve. When comment fixes will follow in the same invoke, this is a **separate** gate from comment publish ([gate-budget](gate-budget.md)).

After approve: re-fetch checks per [check-sync](check-sync.md) with **0** additional AskQuestion.

## Fix picklist

**MUST NOT** edit repository files for a gated comment until the user selects that comment in this gate.

1. Show triage summary for remaining eligible items (or reference persisted records).
2. Call structured prompt **once** with `allow_multiple: true`.

**Prompt:** `Which comments should I fix locally?`

**Eligible rows:** `fix_state: pending` (human always; automation when user chose `fix-picklist` or PR is mixed). Omit `action: defer` and non-fix rows from the picklist.

|Order|id|label|
|-|-|-|
|1|`all-actionable`|`All actionable items (<n>)` — every eligible row below|
|2…|`<comment-id>`|`[<id>] <author_kind> — <short summary>`|
|last|`none`|None — skip local fixes this pass|

3. **End turn** on plain-text fallback until selection.
4. On `all-actionable` or per-id selection: implement only those ids; update `fix_state` and `applied_summary`.
5. On `none`: skip to [Publish gate](#publish-gate) when prior `fix_state: applied` gated items exist; else when `loop_mode: false` stop with short status; when `loop_mode: true` proceed to SKILL.md step 5 ([loop-mode](loop-mode.md)).

### Ambiguous fix (per comment)

**Prompt:** `Comment [<id>] is ambiguous — how should I address it?`

|id|label|
|-|-|
|`clarify`|I'll clarify in chat|
|`skip`|Skip this comment|
|`wontfix`|Mark wontfix with note|

**End turn** on `clarify`. Do not guess and edit.

## Publish gate

After fix picklist implementations. **Replaces a separate “review fixes” prompt** — diff review is preview text only, then **one** `AskQuestion`.

See [commit-and-push.md](commit-and-push.md) § Publish gate and [gate-budget.md](gate-budget.md).

**Forbidden:** a separate “ready to commit?” or “commit and push?” prompt before or after publish gate.

Optional re-sync: one chat line before resolve picklist ([fetch-and-sync](fetch-and-sync.md)); not a separate `AskQuestion`.

## Resolve picklist (one gate)

Eligible gated rows: `fix_state: applied` or action `reply-only` / `resolve-without-code`; `thread_id` when resolving on GitHub.

**One structured prompt only** — picklist selection **is** approval to reply and resolve on GitHub. **Forbidden:** a second `AskQuestion` (“resolve these?”) after the user already selected threads.

1. Draft reply text per eligible row ([commit-and-push](commit-and-push.md) § Reply commit link when `last_pushed_commit` is set).
2. Show a compact reply preview in chat (id → reply one-liner) **before** `AskQuestion`.
3. Call structured prompt **once** with `allow_multiple: true` — **end turn** until selection.

**Prompt:** `Which threads should I reply to and resolve on GitHub? (selection approves the previewed replies)`

|Order|id|label|
|-|-|-|
|1|`all-actionable`|`All actionable items (<n>)` — every eligible row below|
|2…|`<comment-id>`|`[<id>] <short summary>`|
|last|`none`|None — stop without GitHub writes|

4. On `all-actionable` or per-id selection: run [resolve-threads](resolve-threads.md) for **all** selected ids in the **same turn** — **no** follow-up `AskQuestion`, **no** per-thread approve, **no** `approval-prompt.md` GitHub double-ask.
5. On `none`: stop without GitHub writes.

To change a reply: user says so in chat → re-run resolve picklist once. **MUST NOT** add per-thread resolve confirms.

### Plain-text fallback (resolve)

Show reply preview → one multi-select or numbered list ask → **end turn** → selection executes. Still **one** confirmation turn — no second “proceed?” ask.

## Never counts as approval (human gates)

- Starting invoke wording (including `bypass` alone — that approves the **full bypass sweep** per [bypass-mode](bypass-mode.md), including code check autofix; infra/unknown checks stay report-only)
- Silence or implied consent.
- Triage report alone.
- Preview without structured picklist selection (resolve) or `approve` (publish / review).
- A **second** resolve confirmation after resolve picklist selection.

Shared PR rules: [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync).
