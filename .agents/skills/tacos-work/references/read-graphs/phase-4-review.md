# tacos-work phase 4 — Apply review

Parent MUST enter this phase in the same turn when Phase 3 completes — forbidden: ending the turn after implementation with apply-review still unchecked.

## MUST read

- [../../../tacos-apply-review/SKILL.md](../../../tacos-apply-review/SKILL.md) Entry — tacos-work mode bundle
- [../../../tacos-orchestration/references/post-artifact-signoff.md](../../../tacos-orchestration/references/post-artifact-signoff.md) **Apply review — parallel launch** when `review.apply_review_additional_skills` non-empty
- [../../../tacos-orchestration/references/review-gate-pass.md](../../../tacos-orchestration/references/review-gate-pass.md)
- [../minor-polish-gate.md](../minor-polish-gate.md)

## During phase

|Step|Action|
|-|-|
|4.0|`tacos-work [<slug>]: review — apply-review` (same turn as Phase 3 completion)|
|4.1|When `review.apply_review_additional_skills` non-empty: parent MUST evaluate applicability per [post-artifact-signoff.md](../../../tacos-orchestration/references/post-artifact-signoff.md) **Apply review — parallel launch** step 1 and [host-additional-skills.md](../../../tacos-apply-review/references/host-additional-skills.md) **Apply review applicability** — skip spawn when confident zero-match; record **`## Skipped additional skills`** in merge. Parallel Task: core `agent-tacos-apply-review` + `agent-tacos-additional-apply-review` per **applicable** path; else core only → `artifacts/openspec-reviews/<slug>/apply-review.md`|
|4.2|Inputs: session diff, `tasks.md`, touched `openspec/specs/**`|
|4.3|On fail: append **Re-review after fixes** per review-gate-pass → **`agent-tacos-orchestrator-fixes`** when Task supported → same-turn STOP → fresh re-review Task; on pass, gate-runner when skip does not apply per task-stage-contract **Gate-runner skip**|
|4.4|Main-spec drift: [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md) § tacos-work before Phase 5|
|4.5|When APPROVE + Ready with open **Optional (MINOR)** rows: [minor-polish-gate.md](../minor-polish-gate.md) — conservative auto-fix, structured multi-select for complicated remainder, **Polish outcome**; then check off **Apply review:**|

## Done when

- Latest merged apply-review is APPROVE + Ready; no pending **Re-review after fixes**
- MINOR polish complete per minor-polish-gate when **Optional (MINOR)** had open rows
- **Apply review:** checked off
- Next: [phase-5-done.md](phase-5-done.md) — human gate only
