# Review gate pass (orchestrator)

Binding for **planning spec review** (POST-ARTIFACT), **staged apply review**, **tacos-work** apply review, and manual `/tacos-apply-review` / `/tacos-spec-review` parents. Reviewers write Summary bullets per skill templates; orchestrators **MUST** treat them as hard gates — not advisory prose.

## Pass criteria (read merged artifact before advancing)

After the parent writes `artifacts/openspec-reviews/<change>/` (or `<slug>/` for tacos-work), read `## Summary`, **Must address**, and **Should address** (apply) or **Must address** / severity tables (planning).

**Pass** only when **all** of the following hold:

|Check|Pass|Fail (remediation required)|
|-|-|-|
|**Status**|`APPROVE` only|`APPROVE WITH CHANGES`, `NEEDS REVISION`|
|**Readiness**|`Ready` only|`Ready after fixes`, `Not ready`|
|**Must address**|Zero open `BLOCKER` rows; zero open `CRITICAL` rows that block the gate|Any blocking `BLOCKER` or `CRITICAL`|
|**Should address**|Zero open `MAJOR` rows (apply-review **Should address (MAJOR)**; planning **Must address** MAJOR lines)|Any open `MAJOR` the reviewer placed in scope for this cycle|

**Status / Readiness meanings (reviewer):**

|Status|When to use|
|-|-|
|`APPROVE`|No open `BLOCKER`, `CRITICAL`, or `MAJOR` in scope — `MINOR` / **Deferred** only|
|`APPROVE WITH CHANGES`|Open `MAJOR` items remain — orchestrator **MUST** fix before gate pass|
|`NEEDS REVISION`|Open blocking `BLOCKER` or `CRITICAL`|

|Readiness|When to use|
|-|-|
|`Ready`|Gate-pass bar met (pairs with `APPROVE`)|
|`Ready after fixes`|Open `MAJOR` (or other should-fix items) remain — fix then re-review|
|`Not ready`|Maintainability **BLOCKER** or other hard block (pairs with `NEEDS REVISION` or blocking severity)|

`APPROVE WITH CHANGES` and `Ready after fixes` are **not** pass states — they mean fix **Should address** / agreed **MAJOR** (and planning **Must address** MAJOR when used), re-run checks (apply), then fresh re-review until `APPROVE` + `Ready` or human waiver.

**Do not** infer pass from zero table rows when Summary says `APPROVE WITH CHANGES`, `NEEDS REVISION`, `Ready after fixes`, or `Not ready`. **Do not** check off **`Apply review:`**, open **`Human review:`**, suggest planning complete, or apply handoff until pass or explicit human waiver in chat.

## MINOR polish (orchestrator, after apply-review pass)

When merged apply-review **Status** is **`APPROVE` + `Ready`** and **Optional (MINOR)** lists open rows, the orchestrator **MUST** run the MINOR polish gate per [../../tacos-work/references/minor-polish-gate.md](../../tacos-work/references/minor-polish-gate.md) (tacos-work Phase 4; staged apply per [task-stage-contract.md](task-stage-contract.md) ## Mechanical MINOR sweep) **before** checking off **Human review:** or advancing to the next stage. On tacos-work, complete polish before **Apply review:** checkoff when MINORs existed — [minor-polish-gate.md](../../tacos-work/references/minor-polish-gate.md) ## Phase binding.

- Auto-fix **simple** rows per conservative rubric in minor-polish-gate (single-file, ≤3 lines, cross-link / carve-out / explicit MUST NOT).
- When complicated rows remain: structured **multi-select** gate — one option per MINOR; selections = fix now; unselected = waived (record in turn summary and **Polish outcome**).
- **Forbidden:** "ready for Human review" while open MINOR rows are neither fixed nor waived.

MINOR polish does not require `apply-review-r2.md`. Re-run host implementation gates after polish edits when the gates block is non-empty.

## Anti-short-circuit (orchestrator)

Orchestrator edits after a failing review **do not** change gate status on the prior artifact. A failing rN (`NEEDS REVISION`, `Not ready`, open **BLOCKER**/**CRITICAL**/**MAJOR**) stays failing until a **new** merged review artifact r(N+1) exists and its Summary is **`APPROVE` + `Ready`**.

**Forbidden:**

- Treating parent prose ("fixed before handoff", "BLOCKERs resolved", "aligned per review") as gate pass
- Skipping **Re-review** / **Delta re-review** because the orchestrator applied fixes in the same thread
- Citing rN Summary after orchestrator fixes — read **latest** `*-review-rN.md` (or `apply-review-<stage>-rN.md`) only
- Stock opsx closing lines ("Ready for implementation", "planning complete") while latest review Summary is not **`APPROVE` + `Ready`**

**Required turn summary on fail → fix path:** name prior review path and fail Summary → state fixes applied → spawn fresh re-review Task → cite r(N+1) path and pass/fail Summary before sign-off or handoff.

Parent self-attestation is step 2 of the remediation loop only — step 4 (**fresh Task** re-review) is **mandatory** unless the human waives in chat.

## Same-turn STOP (orchestrator)

After reading a **failing** review Summary on any path in [Where this binds](#where-this-binds), the orchestrator **MUST NOT** in the same turn (before a fresh Task re-review returns):

- Check off **`Apply review:`**, **`Human review:`**, planning sign-off, or apply handoff
- Write `*-review-r2.md`, `apply-review-<stage>-r2.md`, or increment on the parent
- Treat parent prose ("fixed", "BLOCKERs resolved") as gate pass

**After fixes** (and re-run checks for apply paths), the **next action MUST be** a fresh Task re-review spawn per [post-artifact-planning-review.md](post-artifact-planning-review.md) (**Delta re-review**) / [post-artifact-signoff.md](post-artifact-signoff.md) (**Re-review after fixes**) — not a user-facing completion summary and not gate checkoff.

## Dynamic re-review checkbox

When the latest review **fails** per the **Pass criteria** table above — including `APPROVE WITH CHANGES`, `NEEDS REVISION`, `Ready after fixes`, `Not ready`, or open **BLOCKER** / **CRITICAL** / **MAJOR** in scope — the orchestrator **MUST** append one unchecked line **immediately after reading the failing Summary** and **before** applying fixes. The line is **not** in the planning template; it appears only once the initial review (or prior re-review) has run and failed.

**Append position:** always **after** the review line that failed (`Apply review:`, planning review gate, or the previous **Re-review after fixes** / **Delta re-review** line) — never before the initial review line.

|Path|Append location|Line template|
|-|-|-|
|POST-ARTIFACT planning|End of `openspec/changes/<name>/tasks.md` (orchestrator-appended; not in schema template)|`- [ ] Delta re-review after fixes: fresh Task **tacos-spec-review** → artifacts/openspec-reviews/<change>/<artifact>-review-r2.md`|
|Staged **Apply review:**|Current `## N.` stage, immediately after the `Apply review:` line|`- [ ] Re-review after fixes: fresh Task **tacos-apply-review** → artifacts/openspec-reviews/<change>/apply-review-<N>-r2.md` (increment `-r3`, … as needed)|
|**tacos-work** **Apply review:**|`artifacts/tacos-work/<slug>/tasks.md` **## Work**, after **Apply review:**|`- [ ] Re-review after fixes: fresh Task **tacos-apply-review** → artifacts/openspec-reviews/<slug>/apply-review-r2.md` (increment as needed)|
|Manual `/tacos-apply-review` or `/tacos-spec-review`|No `tasks.md` — [Turn-summary delegation record](#turn-summary-delegation-record) only|—|

**Check off** a re-review line only when that pass's artifact exists on disk **and** its Summary is **`APPROVE` + `Ready`** (or human waiver). On another fail, append the next `-r(N+1)` line. **Forbidden:** checking the initial **`Apply review:`** line while a pending **Re-review after fixes** / **Delta re-review** line remains unchecked.

## Turn-summary delegation record

Before checking off any review gate or re-review checkbox, the parent turn summary **MUST** include:

1. **Prior review:** `<path>` — Summary `Status` / `Readiness`
2. **Fixes:** one-line scope (what changed)
3. **Delegation:** Task `subagent_type` (`agent-tacos-spec-review`, `agent-tacos-apply-review`, …) for re-review → expected output `<path>`
4. **After Task:** latest `<path>` — Summary `Status` / `Readiness` (pass or fail)

**Forbidden:** paste full review artifact bodies, file contents, diffs, or unbounded child/tool output in chat — cite path + Summary line only. Discovery merges: discovery ≤12 bullets per [subagent-return-contracts.md](subagent-return-contracts.md) and [task-stage-contract.md](task-stage-contract.md) ## Parent discovery discipline.

**Forbidden:** gate checkoff without this block when re-review was required; claiming re-review ran without citing the Task child output path.

## Remediation loop (mandatory on fail)

When any fail condition:

1. **STOP** — do not check off the review gate line; do not start the next stage or planning sign-off. **Immediately** append [Dynamic re-review checkbox](#dynamic-re-review-checkbox) after the line whose review just failed (when the checklist exists for this path).
2. **Fix** — when Task spawn is supported, **MUST** delegate remediation to **`agent-tacos-orchestrator-fixes`** (child consumes failing review artifact path and scope only). When Task is unavailable, orchestrator applies findings in scope inline:
   - `NEEDS REVISION` / `Not ready` / `BLOCKER` / `CRITICAL` → **Must address**
   - `APPROVE WITH CHANGES` / `Ready after fixes` / open `MAJOR` → **Should address** (apply) or **Must address** MAJOR (planning)
3. **Re-run checks** — apply: defer host implementation gates until after re-review passes ([task-stage-contract.md](task-stage-contract.md) **Re-runs checks** runs after apply review gate pass, not between fix and re-review; gate-runner skip uses merged re-review Summary); planning: edit artifacts only.
4. **Re-review** — **mandatory** fresh Task subagent per [post-artifact-planning-review.md](post-artifact-planning-review.md) (**Delta re-review** for planning → `*-review-r2.md` or increment) / [post-artifact-signoff.md](post-artifact-signoff.md) (**Re-review after fixes** for apply → `apply-review-<stage>-r2.md` or increment). **Do not** proceed to step 5 without a new review file on disk.
5. **Read latest artifact** — gate pass uses r(N+1) Summary only; prior rN fail status is not cleared by parent edits.
6. **Repeat** until `APPROVE` + `Ready` (and zero open blocking rows) or the human waives in chat (state waiver in the turn summary).

Cap: follow existing POST-ARTIFACT / staged-apply fix-loop norms; escalate to the human when stuck after good-faith cycles.

## Reviewer alignment (apply-review and spec-review)

Reviewers **MUST** keep Summary **Status** / **Readiness** consistent with severity rows:

- **MUST NOT** write `Status: NEEDS REVISION` or `Readiness: Not ready` without at least one matching **BLOCKER** or **CRITICAL** row in **Must address** for each blocking theme. Prose-only or unchecked checklist bullets alone are insufficient — add formal rows.
- **MUST** write `Status: NEEDS REVISION` when any blocking **BLOCKER** or **CRITICAL** remains.
- **MUST** write `Status: APPROVE WITH CHANGES` when any open **MAJOR** remains in **Should address** (apply) or **Must address** (planning); **MUST NOT** use `APPROVE WITH CHANGES` when zero **MAJOR** rows are open.
- **MUST** write `Status: APPROVE` only when no open **BLOCKER**, **CRITICAL**, or **MAJOR** in scope (**MINOR** / **Deferred** OK).
- **MUST** write `Readiness: Ready after fixes` when open **MAJOR** (or equivalent should-fix items) remain; `Readiness: Ready` only with `Status: APPROVE`.
- **MUST** write `Readiness: Not ready` when any maintainability **BLOCKER** remains open (apply-review maintainability invariant).
- Spec or behavioral failures that block stage sign-off or apply handoff → **BLOCKER** or **CRITICAL** row, not only `- [ ]` under **Spec compliance** without a **Must address** entry.

Orchestrators enforce Summary and open **MAJOR** rows even when reviewers mis-label status.

## Where this binds

|Gate|Orchestrator reads|Blocks until pass|
|-|-|-|
|POST-ARTIFACT planning spec review|`planning-bundle-review.md` or per-artifact `*-review.md` (+ delta `*-rN.md`)|**Delta re-review** checkbox (when appended), human planning sign-off, `openspec validate` handoff, apply suggestion|
|Staged **`Apply review:`**|`apply-review-<stage>.md` (+ delta `*-rN.md`)|**`Apply review:`** / **Re-review after fixes** checkboxes, **`Human review:`**, next `## N.` stage|
|**tacos-work** **Apply review:**|`artifacts/openspec-reviews/<slug>/apply-review.md` (+ `-rN`)|**Apply review:** / **Re-review after fixes** checkboxes; work session human sign-off per [session-runbook.md](../../tacos-work/references/session-runbook.md)|
|Manual **`/tacos-spec-review`**|`*-review.md` (+ delta `*-rN.md`)|Turn-summary delegation record before treating planning edits complete|
|Manual **`/tacos-apply-review`**|`apply-review-<scope>.md` (+ `-rN`)|Turn-summary delegation record before stage or session sign-off|

Detail: [task-stage-contract.md](task-stage-contract.md), [post-artifact-index.md](post-artifact-index.md), [../../tacos-apply-review/references/review-format.md](../../tacos-apply-review/references/review-format.md), [../../tacos-spec-review/references/spec-review.md](../../tacos-spec-review/references/spec-review.md).
