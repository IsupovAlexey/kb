# Gate budget (mandatory)

**One `AskQuestion` / `AskUserQuestion` per gate** per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md). Preview in chat first; then **one** structured prompt (**next tool call MUST** be the tool when available); **end turn**; act on selection. **MUST NOT** ask the same concern twice in different wording.

## Checks track (when failures exist after sync)

|#|Gate|When|`AskQuestion` count|
|-|-|-|-|
|C0|Matrix root confirm|Root confidence `needs-context`|**0–1** single-select|
|C1|Check fix picklist|`failure_kind: code` roots/standalones with `fix_state: pending`|**0–1** multi-select (1 when eligible; skip when advisory-only or none pending)|
|C2|Check publish|After local check fixes|**1** single-select|
|C3|Re-fetch checks|After check publish|**0** — silent refresh|

**Skip checks track:** when no failing checks exist after sync — go straight to comment triage.

**Advisory-only checks:** when all failures are `infra` or `unknown` — report Check failures section; skip C1–C2; proceed to comment triage.

**Picklist scope:** matrix **roots** and **standalone** code failures only — dependents collapsed ([check-matrix](check-matrix.md)).

**Dual publish:** when both checks and comments require local fixes in one invoke, run **C2** then later comment publish (row 2 below) as **separate** gates — do not merge.

## Gated path — comment track (after checks branch completes or skips)

|#|Gate|When|`AskQuestion` count|
|-|-|-|-|
|0|Post-triage routing|Automation-only PR, not `bypass_mode`|**1** (then branch — no second routing ask)|
|1|Fix picklist|Pending fix items|**1** multi-select|
|2|Publish|After local comment fixes; includes diff review + commit + push|**1** single-select|
|3|Resolve picklist|Before `gh` reply/resolve|**1** multi-select — **selection executes `gh`**|

**Total for typical automation gated pass (comments only):** routing (1) + fix (1) + publish (1) + resolve (1) = **4** — not more.

**With checks + comments code fixes:** add up to C0 (0–1) + C1 (0–1) + C2 (1) before comment rows 0–3.

**Skip rows:** no publish when already on remote; no routing when humans are open (go straight to fix picklist); no check picklist when no code failures.

## Bypass path (`bypass_mode: true`)

|Step|`AskQuestion`|
|-|-|
|Check assess + matrix|**0** (matrix root confirm **0–1** when `needs-context` only)|
|Bypass sweep (code check autofix + automation autofix → autopublish → re-fetch checks → autoresolve)|**0** — `/tacos-pr-triage bypass` or routing `bypass-sweep` is approval for check autofix, commit, push, and automation `gh` writes|
|Human comments remain open|gated comment path per table above|

**Infra/unknown checks:** report only — no autofix during bypass.

## Loop path (`loop_mode: true`)

|Step|`AskQuestion`|
|-|-|
|Each triage iteration (`bypass_mode: false`)|Gated path — **one prompt per gate** per iteration|
|Each triage iteration (`bypass_mode: true`)|Bypass path for checks + automation; human comments keep picklists|
|Settle (CI watch, comment quiet poll)|**0**|
|Bot settle poll finds new automation (`bypass_mode: true`)|**0** — exit settle; full comment sync; bypass sweep same turn (no re-arm); reset `bot_wait_started_at` and `bot_settle_poll_count`|
|Stuck CI timeout at loop boundary|**0–1** — `continue-wait` / `stop-loop` only|

**Loop invoke is approval** for multi-iteration triage — no separate start-loop gate. **`loop` + `bypass`** is approval for multi-iteration bypass sweeps. See [loop-mode](loop-mode.md).

**When `loop_mode: true`:** routing/fix/resolve rows that say "stop" for single-pass apply only when `loop_mode: false`. After comment or bypass track completes with nothing left to gate, run SKILL.md step 5 — evaluate merge-ready exit, settle, next iteration; do not treat single-pass Done when as session complete.

## Forbidden duplicate prompts

|Forbidden|Use instead|
|-|-|
|“Want to commit and push?” then publish gate|Publish gate only (preview includes diff + message)|
|Review gate **and** publish gate|Publish gate preview = fix review|
|Resolve picklist **then** “resolve these?” / per-thread approve|Resolve picklist selection only|
|Matrix confirm **and** duplicate root question in picklist|Matrix confirm once per cluster when shown|
|Check fix picklist **and** comment fix picklist merged|Separate picklists per track|
|`approval-prompt.md` GitHub preview+approve **after** resolve picklist|Resolve picklist supersedes — see below|
|Prose `Next steps` / `What would you like?` instead of `AskQuestion`|Matching gate from [approval-gates](approval-gates.md)|
|Optional re-sync as its own `AskQuestion`|One-line offer in chat; user may reply next turn or skip silently|
|Re-fetch checks as its own `AskQuestion`|Silent after check publish (C3)|

## Overrides generic GitHub approval

[approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md) § GitHub PR create and sync **does not apply** to pr-triage resolve/reply. **Do not** add a second approve prompt because that section says “ask before `gh`” — resolve picklist **is** that ask.

Publish gate **is** the only approve step for `git commit` / `git push` on the **gated** path. **Bypass** autopublish does not use publish gate — invoke is approval.

## Agent procedure (one turn = one gate)

1. Complete preview text in the assistant message.
2. Call `AskQuestion` **once** for the current gate.
3. **End turn** — no `git commit`, `git push`, or `gh` in the same turn as the question.
4. Next turn: execute the selected action; advance to the **next** gate only (never re-ask the same gate).
