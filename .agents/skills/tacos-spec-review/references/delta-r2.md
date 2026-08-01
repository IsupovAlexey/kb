# Delta re-review protocol

Second and later planning review passes after substantive fixes (delta re-review, **r2** and higher).

**Trigger (mandatory):** Any **review → changes** cycle on planning artifacts — e.g. POST-ARTIFACT orchestrator fixes after `*-review.md`, user-requested edits after `/tacos-spec-review`, or repeated fix loops. If planning files under `openspec/changes/<name>/` changed because of review findings, run delta re-review before treating the planning turn as complete. Human may waive trivial-only edits in chat.

**Anti-short-circuit:** When rN Summary is **NEEDS REVISION**, **Not ready**, **APPROVE WITH CHANGES**, or **Ready after fixes**, orchestrator fixes are step 2 only — **MUST** write r(N+1) via fresh Task before planning sign-off, validate handoff, or apply suggestion. Append **Delta re-review after fixes** to `openspec/changes/<name>/tasks.md` per [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) **Dynamic re-review checkbox** when review fails. **Same-turn STOP:** after fixes, next action MUST be Task spawn — **FORBIDDEN** parent-authored r2. Record [Turn-summary delegation record](../../tacos-orchestration/references/review-gate-pass.md#turn-summary-delegation-record). Prior rN fail Summary is not cleared by parent edits. Cross-artifact contradiction **BLOCKER**s are not waivable as trivial-only.

**Delegation (mandatory):** Run via a **fresh** Task subagent or Task tool session — **not** inline in the thread that produced the prior review or applied the fixes. The parent passes prior `*-review.md`, current artifact path(s), and the output path (`*-review-r2.md`, then `-r3`, …). **Rubric:** load full [dimensions.md](dimensions.md) in addition to prior review input.

When `review.spec_review_additional_skills` is non-empty and the runtime supports concurrent Task spawns, **match initial POST-ARTIFACT parallel launch** — fresh core **`agent-tacos-spec-review`** plus one **`agent-tacos-additional-spec-review`** per path in **one parent turn**; parent merge into `*-review-r2.md` (per [post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) **Delta re-review**).

When the array is non-empty but concurrent multi-spawn is unavailable, **one** child runs core tacos rubric then each additional skill **sequentially** in array order (per `SKILL.md` ## Workflow step 1 and **Parent delegation**); parent merge notes sequential fallback.

1. Read prior review fully
2. Compare to current artifacts; mark resolved / open / regression
3. New issues only in Must address / Weak points
4. Write output at caller path (`*-review-r2.md`, increment as needed)

Skip only for trivial edits (typo/formatting with no requirement impact); state why in the planning turn summary.
