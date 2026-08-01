---
name: tacos-spec-review
description: >-
  Reviews OpenSpec planning artifacts for completeness, grill alignment, task-stage
  BLOCKERs, and actionability. Use when planning review is enabled, /tacos-spec-review,
  or delta re-review after planning edits. Planning only; implementation uses
  tacos-apply-review.
user-invocable: true
argument-hint: >-
  Optional change id or artifact scope; optional deep for discovery sweep on unchanged
  artifacts (optional cap, e.g. deep 10); not POST-ARTIFACT auto.
---

# tacos spec review

Reviews planning artifacts only. Implementation diffs use tacos-apply-review.

**Challenger mandate:** Normative rubric entry in [references/dimensions.md](references/dimensions.md). Delegate via Task when supported; authoring orchestrator MUST NOT satisfy initial planning review inline.

## Quick start

Inputs:

- Change name, artifact path(s), `grill-summaries.md` when present, prior `*-review.md` for delta
- `deep` invoke: unchanged artifacts; parent loop per [references/deep-mode.md](references/deep-mode.md); prior `*-deep-*.md` when pass > 1

Output:

- `artifacts/openspec-reviews/<change>/` — template in [references/output-template.md](references/output-template.md)
- Deep sweep: `*-deep-N.md` per pass; **## Deep pass outcome** with `new_hard_major_count`

|User says|Action|
|-|-|
|`/tacos-spec-review`|Single pass; parallel additional when configured|
|`/tacos-spec-review deep`|Parent discovery loop; one child per pass (core + sequential additional when configured); cap from yaml|
|`/tacos-spec-review deep 10`|Deep loop; max 10 passes|

Sample finding: `BLOCKER | design.md | Contradicts grill User inputs: API must be sync-only`

Invocation: Task subagent or Task tool when the runtime supports it — not inline-only in the orchestrator thread unless the human waives or Task is unavailable. See [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md).

Host subagent: Core review runs as `agent-tacos-spec-review` (model from `review.spec_review_models` in installed agent frontmatter). Orchestrator MUST NOT pass `model` from yaml on Task. After changing models, run `/tacos-doctor config` to refresh frontmatter.

Parent delegation: When `review.spec_review_additional_skills` is non-empty, the parent follows [post-artifact-planning-review.md](../tacos-orchestration/references/post-artifact-planning-review.md) Planning spec review — parallel launch and [host-additional-skills.md](references/host-additional-skills.md). Delta re-review after fixes: one fresh `tacos-spec-review` child per Delta re-review (r2) in that file.

## Workflow

1. Confirm caller mode: parallel initial pass (core + additional children, parent merge), single core child (empty additional array), delta re-review (`*-review-r2.md`, fresh subagent), or **deep** (parent loop — [references/deep-mode.md](references/deep-mode.md); sequential additional in one child per pass when array non-empty).
2. Load inputs: change folder, artifact scope, `grill-summaries.md`, prior `*-review.md` when delta; prior `*-deep-*.md` when deep pass > 1.
3. Assess dimensions per invocation mode: every pass → full [references/dimensions.md](references/dimensions.md) (POST-ARTIFACT, manual, delta-r2, and **deep**). Host rubrics per mode per [references/host-additional-skills.md](references/host-additional-skills.md). Include tasks.md Testable outcome per `## N.` stage and decision verification matrix per [dimensions.md](references/dimensions.md#tasksmd-structure-blockers). Emit **Intent fidelity** and **Implicit branch coverage** sections per [output-template.md](references/output-template.md).
4. Classify severity per [references/output-template.md](references/output-template.md#severity-guide); cap ~15 BLOCKER/CRITICAL/MAJOR (overflow → Deferred).
5. Write output at caller path unless the caller is the parent merging parallel children.

Delta re-review (mandatory after review-led planning changes): Fresh Task subagent only — not the thread that reviewed or applied fixes. When the additional-skills array is non-empty, one child runs core rubric then each host skill sequentially (no parallel swarm). Full protocol: [references/delta-r2.md](references/delta-r2.md).

## Approval bar

- Status: NEEDS REVISION when any blocking BLOCKER or CRITICAL remains
- Status: APPROVE WITH CHANGES / Readiness: Ready after fixes when any open MAJOR remains — fix before gate pass
- Status: APPROVE / Readiness: Ready only when no open BLOCKER, CRITICAL, or MAJOR in scope
- Readiness: Not ready when any hard block remains (pairs with blocking BLOCKER / CRITICAL)
- Orchestrator: after merge, enforce [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md) on the latest artifact before planning sign-off or apply handoff
- Reviewer: Summary Status / Readiness MUST match Must address — [output-template.md](references/output-template.md#summary-gate-pass-orchestrator--reviewer)

## Done when

- Caller path holds a completed review artifact matching [output-template.md](references/output-template.md#required-output-format).
- Summary Status / Readiness align with Must address per [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md).
- After a failed Summary: fresh Task wrote `*-review-r(N+1).md` per [delta-r2.md](references/delta-r2.md) before planning sign-off or apply handoff.

## When to use

- POST-ARTIFACT after propose, ff, or continue when `orchestration.planning_review_enabled`
- `/tacos-spec-review` or natural-language “review the spec”
- `/tacos-spec-review deep` — discovery sweep on unchanged artifacts ([references/deep-mode.md](references/deep-mode.md))
- Planning bundle after propose or ff when the user requests (`artifacts/openspec-reviews/<change>/planning-bundle-review.md`)
- After any spec review that led to planning artifact edits — delta re-review before treating the turn complete

## Scope

Artifacts: proposal, specs, design, tasks, e2e-scenarios, or a planning bundle. Rubric index: [references/spec-review.md](references/spec-review.md) → [dimensions.md](references/dimensions.md), [output-template.md](references/output-template.md), [delta-r2.md](references/delta-r2.md), [deep-mode.md](references/deep-mode.md).

## References

- `tacos-orchestration` — [post-artifact-planning-review.md](../tacos-orchestration/references/post-artifact-planning-review.md), [post-artifact-index.md](../tacos-orchestration/references/post-artifact-index.md), [task-stage-contract.md](../tacos-orchestration/references/task-stage-contract.md), [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md), [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md)
- `tacos-apply-review` — apply-stage only
- `tacos-grill` — [grill-summaries template](../tacos-doctor/schemas/tacos/templates/grill-summaries.md)
- Host `AGENTS.md` and `openspec/config.yaml` `context`
