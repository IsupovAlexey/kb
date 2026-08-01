---
name: tacos-apply-review
description: >-
  Reviews implementation diffs (code, skills, config, docs) for spec compliance,
  structural maintainability BLOCKERs, KISS/YAGNI, DRY, SRP, host standards, and tests —
  keeps changes minimal and reviewable for human sign-off.
  Use when orchestration staged apply reaches Apply review:, /tacos-apply-review,
  or the user asks to review a diff. Planning artifacts use tacos-spec-review.
user-invocable: true
argument-hint: >-
  Optional change id or diff scope; optional deep for discovery sweep on unchanged diff
  (optional cap, e.g. deep 10); not staged Apply review auto.
---

# tacos apply review

Apply-stage gate: keep AI-assisted implementation minimal and reviewable — bounded diffs, spec traceability, and conventions so humans can sign off per stage. DRY matters here: duplicate logic inflates the diff (full rubric: [tacos-implementation-conventions](../tacos-implementation-conventions/SKILL.md)).

Reviews implementation changes (diff) against planning context. Planning artifact quality uses tacos-spec-review.

## Entry

1. Resolve caller mode:
   - full tacos — orchestrated staged apply; change folder planning set
   - tacos-work — `/tacos-work` session; main specs + work artifacts
   - re-review — after fixes; fresh Task child only
   - deep — parent discovery loop on unchanged diff; [references/deep-mode.md](references/deep-mode.md)
2. Load mode bundle (MUST gates ≤1 hop from this Entry):
   - full tacos — Workflow below; parallel launch MUST per Parent delegation; pass-1 hard stop
   - tacos-work — [## Input](#input) Work binding; [checklist-pass1.md](references/checklist-pass1.md) ## Work binding
   - re-review — [review-format.md](references/review-format.md) ## After fixes; fresh Task only
   - deep — [references/deep-mode.md](references/deep-mode.md); sequential additional in one child per pass when array non-empty
3. Checklist router: [checklist.md](references/checklist.md); pass 1: [checklist-pass1.md](references/checklist-pass1.md); pass 2: [checklist-pass2.md](references/checklist-pass2.md).

Invocation: Task subagent when supported — not inline-only unless waived or Task unavailable. Host subagent: agent-tacos-apply-review (model from installed frontmatter; orchestrator MUST NOT pass `model` from yaml). Parent launch: [apply-review-dispatch-prompt.md](references/apply-review-dispatch-prompt.md) — copy `prompt:` verbatim. See [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md).

Parent delegation: when `review.apply_review_additional_skills` is non-empty, parent MUST Task tacos-apply-review (core) and one tacos-additional-apply-review per applicable path in parallel — [post-artifact-signoff.md](../tacos-orchestration/references/post-artifact-signoff.md) **Apply review — parallel launch**, [host-additional-skills.md](references/host-additional-skills.md). Re-review: match initial pass — parallel core + applicable additional when concurrent spawn supported; one fresh core child sequential when not ([host-additional-skills.md](references/host-additional-skills.md) **Re-review after fixes**).

## Quick start

Full tacos inputs: `openspec/changes/<name>/` planning set + stage diff.

Full tacos output: `artifacts/openspec-reviews/<change>/apply-review-<stage>.md`.

Deep output: `apply-review-deep-N.md` or `apply-review-<stage>-deep-N.md` — [references/deep-mode.md](references/deep-mode.md).

|User says|Action|
|-|-|
|`/tacos-apply-review`|Single pass; parallel additional when configured|
|`/tacos-apply-review deep`|Parent discovery loop; one child per pass (core + sequential additional when configured)|
|`/tacos-apply-review deep 10`|Deep loop; max 10 passes|

Tacos-work inputs: session diff + `artifacts/tacos-work/<slug>/tasks.md` + touched `openspec/specs/**`.

Tacos-work output: `artifacts/openspec-reviews/<slug>/apply-review.md`.

Severity: BLOCKER | CRITICAL | MAJOR | MINOR.

Template: [review-format.md](references/review-format.md). Pipeline: [pipeline-and-verification.md](references/pipeline-and-verification.md).

## Workflow

1. Confirm caller mode: parallel initial pass (core + additional children, parent merge), single core child (empty additional array), re-review after fixes, or **deep** (parent loop — [references/deep-mode.md](references/deep-mode.md)).
2. Load inputs per session mode ([## Input](#input)).
3. Pass 1 — Spec compliance — [spec-compliance-pass.md](references/spec-compliance-pass.md) and [checklist-pass1.md](references/checklist-pass1.md). Hard stop: do not start pass 2 while pass 1 has open BLOCKER or CRITICAL.
4. Main-spec drift detect — after pass 1; write ## Main-spec drift per [review-format.md](references/review-format.md). BLOCKER when stale spec or code violation and sync-pending does not apply ([checklist-pass1.md](references/checklist-pass1.md) ## Main-spec drift detect).
5. Pass 2 — Code quality — when pass 1 and drift lane pass; [checklist-pass2.md](references/checklist-pass2.md). When pass 1 or drift fails, record Pass 2: Skipped.
6. Classify severity; cap ~15 BLOCKER/CRITICAL/MAJOR (overflow → Deferred).
7. Write output at caller path unless parent merging parallel children.

Re-review after fixes: fresh Task subagent only — [review-format.md](references/review-format.md) ## After fixes.

## When to use

- `/opsx:apply` when a stage Apply review: line is reached
- `/tacos-apply-review` or natural language “review this diff”
- `/tacos-apply-review deep` — discovery sweep on unchanged diff ([references/deep-mode.md](references/deep-mode.md))
- After fixes: re-review via fresh Task with prior `apply-review-*.md` + new diff

## Input

Changed files (diff or paths) + planning context for the session. One logical batch per review.

### Full tacos (orchestrated change)

`openspec/changes/<name>/` planning set (`proposal`, specs delta, `design`, `tasks`) plus the stage diff.

### Work binding (tacos-work)

When caller is a tacos-work session:

- Session diff — changed files for the work run
- Work session file — `artifacts/tacos-work/<slug>/tasks.md` (Intent, Planning, ## Work)
- Main specs — touched paths under `openspec/specs/**` in git

Do not require or read `openspec/changes/<slug>/` for Work sessions. Trace obligations to main specs and ## Work / Planning in `tasks.md`.

## Dimensions

Rubric: [review-format.md](references/review-format.md) ## Review dimensions. Mixed diffs: run structural maintainability and non-code maintainability when code and skills/schema/docs both appear.

## Approval bar

- Readiness: Not ready when any maintainability BLOCKER remains
- Status: NEEDS REVISION when any blocking BLOCKER or CRITICAL remains
- Status: APPROVE WITH CHANGES when any open MAJOR remains
- Status: APPROVE / Readiness: Ready only when no open BLOCKER, CRITICAL, or MAJOR in scope
- Maintainability BLOCKERs MUST be fixed in the same stage before Human review: checkoff — no defer-to-later-stage for structural regressions
- Orchestrator: enforce [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md) on latest artifact before Apply review: or Human review: checkoff
- Reviewer: Summary Status / Readiness MUST match severity rows — [review-format.md](references/review-format.md)

## Done when

- Caller path holds completed artifact matching [review-format.md](references/review-format.md) ## Required output format.
- Summary Status / Readiness align with [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md).
- After failed Summary: fresh Task wrote `apply-review-<stage>-r(N+1).md` per [review-format.md](references/review-format.md) ## After fixes before stage checkoff.

## References

- tacos-implementation-conventions — [structural-maintainability.md](../tacos-implementation-conventions/references/structural-maintainability.md); [authoring.md](../tacos-implementation-conventions/references/authoring.md)
- tacos-spec-review — planning only
- tacos-orchestration — [task-stage-contract.md](../tacos-orchestration/references/task-stage-contract.md), [post-artifact-signoff.md](../tacos-orchestration/references/post-artifact-signoff.md), [review-gate-pass.md](../tacos-orchestration/references/review-gate-pass.md), [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md)
- [deep-mode.md](references/deep-mode.md) — manual `deep` discovery sweep
