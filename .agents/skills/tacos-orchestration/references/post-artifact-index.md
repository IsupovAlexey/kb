# POST-ARTIFACT index

When `orchestration.enabled` is true and planning artifacts reach apply-ready, continue the **same turn** after **propose**, **ff**, **continue**, or **update** — not stock opsx end state. Toggle names are under `orchestration:` unless qualified — [config-notation.md](config-notation.md).

Propose/ff/continue After hooks: [read-graphs/propose.md](read-graphs/propose.md) ## After command. **Update** After hooks: [read-graphs/update.md](read-graphs/update.md) ## After command. Command ids: [openspec-commands.md](openspec-commands.md). Hub: [orchestration-binding.md](orchestration-binding.md) § POST-ARTIFACT.

## Toggle matrix

|Toggle|Default (template `openspec/tacos.yaml`)|Effect|
|-|-|-|
|`enabled`|true|Master switch; when false, skip all rows below|
|`planning_review_enabled`|true|Spec review → fixes → delta re-review (r2) → human sign-off|
|`e2e_enabled`|false|E2E closure via `agent-tacos-e2e-scenarios` subagent|
|`staged_apply_enabled`|true|`tasks.md` stage contract + apply-stage apply review lines|
|`slice_pr.enabled`|true (template)|Delivery-time `/tacos-slice-pr` nudge when spec review Complexity & split is Med/High|

## Step order (planning turn)

Load step bundles only when that step runs:

1. E2E closure — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### E2E closure (when `e2e_enabled`)
2. Planning spec review — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### Planning spec review (when `planning_review_enabled`)
3. Orchestrator fixes — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### Orchestrator fixes
4. Delta re-review — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### Delta re-review (mandatory after review-led planning changes)
5. Split recommendation — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### Split recommendation (when `slice_pr` enabled and `planning_review_enabled`)
6. OpenSpec validate — [post-artifact-planning-review.md](post-artifact-planning-review.md) ### OpenSpec validate (always when `orchestration.enabled`)
7. Human planning sign-off — [post-artifact-signoff.md](post-artifact-signoff.md) ### Human planning sign-off (when `planning_review_enabled`)

**MUST-delegate — planning turn (≤1 hop):** POST-ARTIFACT orchestrator MUST launch **`agent-tacos-e2e-scenarios`** (when enabled), **`agent-tacos-spec-review`** + **`agent-tacos-additional-spec-review`** (per configured path), and **`agent-tacos-orchestrator-fixes`** on planning remediation via Task when supported — parallel launch + parent merge per step bundles; not inline-only unless waived or Task unavailable ([runtime-delegation.md](runtime-delegation.md) **MUST-delegate matrix**).

**MUST-delegate — apply turn:** see [post-artifact-signoff.md](post-artifact-signoff.md) ## Apply turn (apply) — **`agent-tacos-apply-review`** + **`agent-tacos-additional-apply-review`** (per **applicable** path), **`agent-tacos-apply-implement`**, **`agent-tacos-orchestrator-fixes`**, **`agent-tacos-gate-runner`**, and **`agent-tacos-dedrift-detect`**; applicability skips recorded under **`## Skipped additional skills`**.

## Apply turn

When `staged_apply_enabled`: [post-artifact-signoff.md](post-artifact-signoff.md) ## Apply turn (apply). Stage contract: [task-stage-contract.md](task-stage-contract.md).

## Done / not done (planning turn)

**Done when:** apply-ready artifacts exist; E2E file or omit reason (`e2e_enabled`) or skipped; spec review artifacts under `artifacts/openspec-reviews/<change>/` when `planning_review_enabled`; delta re-review (r2) when substantive fixes occurred; `openspec validate <change-id> --strict --no-interactive` exited `0` or user waived (after review-driven edits when those gates ran); human sign-off or waiver when `planning_review_enabled`.

**Not done if:**

- Human sign-off ran while `openspec validate` still failed and the user did not waive
- Planning spec review was inline-only without waiver
- `review.spec_review_additional_skills` was non-empty but only a core **`agent-tacos-spec-review`** child ran (no **`agent-tacos-additional-spec-review`** per path and no parent merge) unless lightweight-host bypass or user waiver
- E2E closure (`agent-tacos-e2e-scenarios`) was inline-only without waiver when `e2e_enabled`
- Turn ended right after stock opsx without POST-ARTIFACT (unless user asked to stop)
- `tasks.md` violates stage contract when `staged_apply_enabled` (merged review lines, missing human line)
- On **apply**, `Human review:` was checked off or the next stage started without human sign-off or waiver
- On **apply**, `review.apply_review_additional_skills` was non-empty but only a core **`agent-tacos-apply-review`** child ran (no **`agent-tacos-additional-apply-review`** for applicable paths, no applicability skips recorded, and no parent merge) unless lightweight-host bypass or user waiver
- e2e-testable change with `e2e_enabled` but no file and no omit reason
- Existing `e2e-scenarios.md` was overwritten

## Waivers

User may waive any gate in chat; state the waiver in the summary or review artifact.
