# POST-ARTIFACT planning review bundle

Load when the matching step runs. Index and toggle matrix: [post-artifact-index.md](post-artifact-index.md). Sign-off and apply turn: [post-artifact-signoff.md](post-artifact-signoff.md).

## Gate checklist (planning turn)

Run enabled steps in order after apply-ready (`proposal`, `design`, `specs/**`, `tasks` exist).

### E2E closure (when orchestration.e2e_enabled)

Read `../../tacos-e2e-scenarios/SKILL.md`. Schema artifact id: `e2e-scenarios` (`openspec instructions e2e-scenarios --change <name>` when `ready`).

**MUST** invoke the **`agent-tacos-e2e-scenarios`** host subagent via Task when the runtime supports it — `subagent_type` / spawn name **MUST** match agent `name:`; do **not** pass `model` from yaml (host reads `model:` from the installed agent file — [runtime-delegation.md](runtime-delegation.md) **Reference hosts**, **MUST-delegate matrix**, **Host install**). Not inline-only in the POST-ARTIFACT orchestrator thread unless the human waives or Task is unavailable. When delegation is unavailable, run inline and note inline execution in the summary.

See `runtime-delegation.md` (MUST-delegate matrix).

- If e2e-testable → create `openspec/changes/<name>/e2e-scenarios.md` when missing
- If not → one-line omit reason in summary
- **Never overwrite** existing `e2e-scenarios.md`

Skip when orchestration.e2e_enabled is false (manual `/tacos-e2e-scenarios` still allowed).

### Planning spec review (when orchestration.planning_review_enabled)

Read `../../tacos-spec-review/SKILL.md` and named subagents in `runtime-delegation.md`.

**MUST** delegate via Task using **`agent-tacos-spec-review`** (core) and **`agent-tacos-additional-spec-review`** (per additional path when configured) — not inline-only in the authoring thread unless the human waives or Task is unavailable (lightweight-host bypass). Do **not** pass `model` from yaml on Task.

When `grill-summaries.md` exists, reviews **MUST** check grill alignment (artifacts vs resolved **Decisions** / **User inputs** per phase), including **BLOCKER** when `grill.planning` or a phase is `complete` without `grill_mode` or without **User inputs** (evidence parent interview was skipped). See `../../tacos-spec-review/references/spec-review.md` (grill-summaries alignment).

**Rubric:** Initial POST-ARTIFACT, manual, and delta-r2 core children load full `../../tacos-spec-review/references/dimensions.md` per `../../tacos-spec-review/references/delta-r2.md`.

#### Planning spec review — parallel launch and parent merge

When `review.spec_review_additional_skills` is empty → one fresh core **`agent-tacos-spec-review`** subagent only.

When the array is non-empty and the runtime supports concurrent Task spawns (Cursor and Claude Code when the runtime allows multiple child agents in one turn; other hosts → sequential fallback below):

1. Launch **parallel** subagents in **one parent turn** when the host allows (do **not** stop after the core child returns):
   - **Core:** fresh Task with `subagent_type` **`agent-tacos-spec-review`** — do **not** load additional skills inside the core child (orchestrator owns merge; see `../../tacos-spec-review/references/host-additional-skills.md`)
   - **Per entry:** one fresh Task with **`agent-tacos-additional-spec-review`** per repo-relative path string, scoped to **that** host skill plus shared inputs (change folder, artifact scope, `grill-summaries.md` when present)
2. **Parent merge:** After subagents return or fail, the **parent** writes **one** tacos-format review artifact combining outputs. Preserve **Grill alignment**, **Complexity & split**, and severity-capped findings. Host-skill findings **MUST** cite the skill path.
3. When a parallel subagent fails or times out, the merged artifact **MUST** include `## Parallel delegation warnings` with each failed path and a one-line error summary; **WARN** and continue with successful outputs — do not block the gate on one failed child.

**Sequential fallback:** When Task is supported but concurrent multi-spawn is not, run core then each additional subagent **in array order**, parent-merge, and note sequential fallback in the turn summary or merged review preamble.

**Manual `/tacos-spec-review`:** When the array is non-empty and you are the **parent** (not already inside a core-only child), **MUST** use the same parallel launch + parent merge as POST-ARTIFACT above — not core-only, not inline host skills on the parent. Sequential fallback only when Task is supported but concurrent multi-spawn is not ([host-additional-skills.md](../../tacos-spec-review/references/host-additional-skills.md)).

|Mode|When|Output|
|-|-|-|
|Default|continue or per-artifact|`artifacts/openspec-reviews/<change>/<artifact>-review.md`|
|Batch|after **propose** or **ff** only if user requests|`planning-bundle-review.md`|

### Orchestrator fixes

Apply BLOCKER/CRITICAL (and agreed MAJOR) to planning artifacts. Escalate ambiguous tradeoffs.

When Task spawn is supported, **MUST** delegate remediation to **`agent-tacos-orchestrator-fixes`** — child consumes failing review artifact path and scope only (not full parent session history). Return per [subagent-return-contracts.md](subagent-return-contracts.md) ## Fix. When Task is unavailable, parent fixes inline and notes inline execution in the turn summary.

**Fix alone does not pass the gate** — orchestrator edits after a failing rN do not clear rN Summary. **MUST** run **Delta re-review** (below) before sign-off unless the human waives in chat. Append **Delta re-review after fixes** to `openspec/changes/<name>/tasks.md` per [review-gate-pass.md](review-gate-pass.md) **Dynamic re-review checkbox** when review fails. Record [Turn-summary delegation record](review-gate-pass.md#turn-summary-delegation-record) before gate checkoff. See [review-gate-pass.md](review-gate-pass.md) ## Anti-short-circuit.

**Gate pass before sign-off:** After merge (and after each delta pass), read **latest** review Summary per [review-gate-pass.md](review-gate-pass.md). **STOP** human planning sign-off until **`APPROVE` + `Ready`** (no open **MAJOR**) — fix artifacts and run **Delta re-review** until pass or human waiver. **MUST NOT** narrate "fixed before handoff" without citing r(N+1) path and pass Summary.

### Delta re-review (mandatory after review-led planning changes)

After orchestrator applies BLOCKER/CRITICAL (and agreed MAJOR) to planning artifacts, **MUST** re-run review via a **fresh** Task subagent — not inline in this thread. **Forbidden:** skipping this step because fixes were applied in the parent session.

When `review.spec_review_additional_skills` is empty → one fresh core **`agent-tacos-spec-review`** subagent only.

When the array is non-empty and the runtime supports concurrent Task spawns, **match initial POST-ARTIFACT parallel launch**:

1. Launch **parallel** subagents in **one parent turn** when the host allows:
   - **Core:** fresh Task with `subagent_type` **`agent-tacos-spec-review`** — prior `*-review.md` is mandatory input; load full `dimensions.md`; do **not** load additional skills inside the core child
   - **Per entry:** one fresh Task with **`agent-tacos-additional-spec-review`** per repo-relative path string
2. **Parent merge:** After subagents return or fail, the **parent** writes **one** tacos-format artifact at `*-review-r2.md` (increment as needed). Preserve severity-capped findings per [review-format.md](../../tacos-spec-review/references/review-format.md). Include **`## Parallel delegation warnings`** when a child fails.

**Sequential fallback:** When Task is supported but concurrent multi-spawn is not, run core then each additional subagent **in array order**, parent-merge, and note sequential fallback in the turn summary.

Any **review → changes → …** loop **MUST** trigger another delta pass until findings are resolved or the human waives. Skip only if trivial; say so in summary.

### Split recommendation (when `slice_pr` enabled and orchestration.planning_review_enabled)

After planning spec review, orchestrator fixes, and delta re-review when run, read the **Complexity & split** block from the review artifact(s) under `artifacts/openspec-reviews/<change>/`.

When `slice_pr.enabled` is true in `openspec/tacos.yaml` and the review reports **Medium** or **High** complexity:

- Include **one line** in the planning turn summary (before human sign-off): suggest running `/tacos-slice-pr` on the feature branch **after implementation** (ordered review passes on one squash merge PR).
- Do **not** auto-run slice-pr, block apply, or edit planning artifacts for split.
- Do **not** imply multiple merge PRs to trunk.

When complexity is **Low**, `slice_pr.enabled` is false, or orchestration.planning_review_enabled is false (no review artifact), do **not** mention `/tacos-slice-pr` unless the user asks.

This nudge is complementary to `tacos-spec-review` — POST-ARTIFACT surfaces delivery-time slice_pr only; it does not re-assess complexity.

### OpenSpec validate (always when `orchestration.enabled`)

**When:** Apply-ready — `proposal`, `design`, delta `specs/**`, and **`tasks.md`** exist. Confirm with `openspec status --change <name> --json`.

Run **after** E2E closure (when orchestration.e2e_enabled), planning spec review, orchestrator fixes, and delta re-review when those gates ran — **last automated POST-ARTIFACT step**, before human planning sign-off. Spec review and fixes often change planning artifacts; validate the final bundle.

`openspec validate <change-id> --strict --no-interactive`. On failure: summarize CLI errors, **fix reported errors** in planning artifacts and change delta specs, re-run until exit `0`. **STOP** human sign-off until pass or explicit user waiver in chat.

When orchestration.planning_review_enabled is false and orchestration.e2e_enabled is false, run validate immediately after apply-ready (no prior POST-ARTIFACT gates).
