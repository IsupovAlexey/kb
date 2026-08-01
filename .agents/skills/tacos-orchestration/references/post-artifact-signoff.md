# POST-ARTIFACT sign-off and apply bundle

Load when human sign-off or apply-turn gates run. Index: [post-artifact-index.md](post-artifact-index.md). Planning review steps: [post-artifact-planning-review.md](post-artifact-planning-review.md).

### Human planning sign-off (when orchestration.planning_review_enabled)

After POST-ARTIFACT planning gates pass (spec review, orchestrator fixes, delta re-review when run, split recommendation when applicable), run a structured **intent playback** gate before apply handoff. Load [structured-gate-convention.md](structured-gate-convention.md) — option ids `proceed`, `edit_planning`, `cancel`.

#### Intent playback gate

1. **Playback summary** — state in chat before the structured prompt:
   - **Do** — what the change will deliver (from proposal **What Changes**, grill **User inputs** scope contract, and relevant **Decisions**)
   - **Not do** — explicit out-of-scope and rejected adjacent items (from `design.md` **Boundaries**, grill scope contract)
   - **Complexity note** — one line on size/risk (from proposal, design **Risks / Trade-offs**, or spec-review **Complexity & split** when present)
   - **Split recommendation** — when `slice_pr` enabled and review reported Medium/High complexity, include the split line from planning review
   - **Open decisions** — any unresolved items from grill or review (if none, say so)
2. **Structured prompt** — when structured tools are available, the **next parent tool call MUST** be `AskQuestion` / `AskUserQuestion` with option ids `proceed`, `edit_planning`, `cancel`. Plain-text fallback per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools absent.
3. **On `proceed`** — record choice in turn summary; MAY suggest **apply** or mark planning handoff complete.
4. **On `edit_planning`** — MUST NOT start **apply**; route to planning artifact edits (`proposal`, `specs`, `design`, `tasks`, `grill-summaries`) and re-run planning review + playback when apply-ready again.
5. **On `cancel`** — stop; do not suggest apply.
6. **Waiver** — when the user explicitly waives human planning sign-off in chat after viewing the playback summary, record the waiver and MAY proceed — chat waiver substitutes for selecting **proceed**; it does not skip presenting the playback summary or exempt the structured prompt when the user has not yet waived.

Do not suggest **apply** or “planning complete” until the user chooses **`proceed`**, completes **`edit_planning`** and a subsequent playback passes, or explicitly waives in chat.

## Apply turn (apply)

When orchestration.staged_apply_enabled:

- Follow `task-stage-contract.md` per `tasks.md` stage
- **Same-turn apply review** — when stage implementation through **Tests** (and **Project overview:** when present) completes, parent **MUST** delegate **Apply review** in the **same turn** — **forbidden:** ending the turn with **Apply review:** unchecked. **Human review:** is the only required pause before the next stage.
- At each `Apply review:` line: follow **Apply review — parallel launch** below; then read merged artifact for [review-gate-pass.md](review-gate-pass.md). On fail: fix → re-run checks → **Re-review after fixes** until pass or human waiver. On pass: re-run checks → when merged **## Main-spec drift** reports stale spec or code violation, parent offers reconcile/conform/skip per [orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Staged apply (use artifact scope; no second detect pass) → **Human review:** pause.

Read `../../tacos-apply-review/SKILL.md` and named subagents in `runtime-delegation.md`.

**MUST** delegate via Task using **`agent-tacos-apply-review`** (core) and **`agent-tacos-additional-apply-review`** (per **applicable** additional path when configured — applicability per step 1 below) — not inline-only in the apply orchestrator thread unless the human waives or Task is unavailable (lightweight-host bypass). Do **not** pass `model` from yaml on Task.

**Pass-scoped checklist:** Initial core child loads `../../tacos-apply-review/references/checklist-pass1.md` and `spec-compliance-pass.md` only; pass-2 bundle (`checklist-pass2.md`, `tacos-implementation-conventions` when depth needed) loads in SKILL step 5 or re-review when pass-1 and main-spec drift are clean per `../../tacos-apply-review/SKILL.md`.

**Dispatch contract:** Parent MUST read `../../tacos-apply-review/references/apply-review-dispatch-prompt.md` and copy its `prompt:` block verbatim into each core **`agent-tacos-apply-review`** Task — substitute only `{PLACEHOLDER}` tokens; do not improvise the child prompt body.

#### Apply review — parallel launch and parent merge

When `review.apply_review_additional_skills` is empty → one fresh core **`agent-tacos-apply-review`** subagent only.

When the array is non-empty and the runtime supports concurrent Task spawns (Cursor and Claude Code when the runtime allows multiple child agents in one turn; other hosts → sequential fallback below):

1. **Applicability** — parent MUST evaluate applicability for each additional path before spawn: infer scope from skill name/description/path and match the review diff per `../../tacos-apply-review/references/host-additional-skills.md` **Apply review applicability**. Skip spawn when inference is confident and zero diff paths match; record under **`## Skipped additional skills`** in the merge (omit section when empty). Spawn when scope is not confidently inferable or diff is ambiguous.
2. Launch **parallel** subagents in **one parent turn** when the host allows (do **not** stop after the core child returns):
   - **Core:** fresh Task with `subagent_type` **`agent-tacos-apply-review`** — do **not** load additional skills inside the core child (orchestrator owns merge; see `../../tacos-apply-review/references/host-additional-skills.md`)
   - **Per applicable entry:** one fresh Task with **`agent-tacos-additional-apply-review`** per repo-relative path string, scoped to **that** host skill plus shared inputs (change folder, stage diff, prior `apply-review-*.md` when re-review)
3. **Parent merge:** After subagents return or fail, the **parent** writes **one** tacos-format file at `artifacts/openspec-reviews/<change>/apply-review-<stage>.md`. Preserve severity-capped findings. Host-skill findings **MUST** cite the skill path. Include **`## Skipped additional skills`** when any entry was applicability-skipped.
4. When a parallel subagent fails or times out, the merged artifact **MUST** include `## Parallel delegation warnings` with each failed path and a one-line error summary; **WARN** and continue with successful outputs — do not block the gate on one failed child.

**Sequential fallback:** When Task is supported but concurrent multi-spawn is not, evaluate applicability (step 1) then run core then each **applicable** additional subagent **in array order**, parent-merge with **`## Skipped additional skills`** when needed, and note sequential fallback in the turn summary or merged review preamble.

**Re-review after fixes:** **Mandatory** after orchestrator fixes from a failing apply review — parent self-report does not pass the gate ([review-gate-pass.md](review-gate-pass.md) ## Anti-short-circuit). Append **Re-review after fixes** per [review-gate-pass.md](review-gate-pass.md) **Dynamic re-review checkbox** before fixes.

When `review.apply_review_additional_skills` is empty → one fresh core **`agent-tacos-apply-review`** subagent; write `apply-review-<stage>-r2.md` or increment.

When the array is non-empty and the runtime supports concurrent Task spawns, **match initial apply review parallel launch**:

1. **Applicability** — same rules as initial pass (step 1 above)
2. Launch **parallel** subagents in **one parent turn** when the host allows:
   - **Core:** fresh Task with `subagent_type` **`agent-tacos-apply-review`**
   - **Per applicable entry:** one fresh Task with **`agent-tacos-additional-apply-review`** per repo-relative path
3. **Parent merge** into `apply-review-<stage>-r2.md` (or increment) with **`## Skipped additional skills`** and **`## Parallel delegation warnings`** when needed

**Sequential fallback:** When Task is supported but concurrent multi-spawn is not, evaluate applicability then run core then each applicable additional subagent **in array order**, parent-merge, and note sequential fallback.

**Same-turn STOP:** next action after fixes MUST be Task spawn — **FORBIDDEN** parent-authored r2. Record [Turn-summary delegation record](review-gate-pass.md#turn-summary-delegation-record). Repeat until [review-gate-pass.md](review-gate-pass.md) pass on **latest** artifact or human waiver.

**Orchestrator fixes (apply):** When apply review fails and remediation is required, **MUST** delegate to **`agent-tacos-orchestrator-fixes`** when Task is supported — child consumes failing `apply-review-*.md` path and stage scope only. When Task is unavailable, parent fixes inline and notes inline execution. Fresh re-review Task after fixes per anti-short-circuit rules above.

**Manual `/tacos-apply-review`:** When the array is non-empty and you are the **parent** (not already inside a core-only child), **MUST** use the same parallel launch + parent merge as staged apply above — not core-only, not inline host skills on the parent. Sequential fallback only when Task is supported but concurrent multi-spawn is not ([host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md)).

- **`Human review:` gate (required pause)** — after apply review **gate pass**, orchestrator fixes, re-runs checks, and any **Staged apply** dedrift prompt when **## Main-spec drift** reports drift ([orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Staged apply):
- **Stop** and present summary (what shipped in the stage, review path, open items)
- **Wait** for human sign-off in chat or an explicit waiver
- Do **not** check off the `Human review:` checkbox or start the next stage until then
- Do **not** check off `Human review:` before apply review **gate pass** per [review-gate-pass.md](review-gate-pass.md) and orchestrator fixes finish

When orchestration.staged_apply_enabled is false: implement tasks without mandatory terminal lines; triggered grill on ambiguity still applies per `grill.triggers.*`.
