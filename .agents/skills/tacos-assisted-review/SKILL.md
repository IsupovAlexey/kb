---
name: tacos-assisted-review
description: >-
  Advisory PR walkthrough for human reviewers — dual markdown + canvas delivery.
  Invoke via /tacos-assisted-review only (not apply-review or ambient orchestration).
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  PR URL/number, or diff (paths, base...HEAD, branch-to-branch prose e.g. PR to master);
  optional OpenSpec change id. User typically has the review branch checked out.
---

# tacos assisted review

**You assist; the reviewer decides.** Human-led walkthrough: Overview, then per stop narrative → code → look outs (P0→P1).

## Entry

1. Load active phase graph (one hop) in order:
   - [read-graphs/phase-diff.md](references/read-graphs/phase-diff.md)
   - [read-graphs/phase-binding.md](references/read-graphs/phase-binding.md)
   - rubric — [phase-rubric-bound.md](references/read-graphs/phase-rubric-bound.md) or [phase-rubric-adhoc.md](references/read-graphs/phase-rubric-adhoc.md)
   - [read-graphs/phase-gate.md](references/read-graphs/phase-gate.md)
   - [read-graphs/phase-build-markdown.md](references/read-graphs/phase-build-markdown.md)
   - canvas host only — [read-graphs/phase-build-canvas.md](references/read-graphs/phase-build-canvas.md)
   - [read-graphs/phase-deliver.md](references/read-graphs/phase-deliver.md)
2. Do not load canvas bundle until canvas host detected.

## Quick start

|User says|Action|
|-|-|
|`/tacos-assisted-review`|Phase chain from diff|
|`/tacos-assisted-review main...HEAD`|Local diff range|
|`/tacos-assisted-review 42 add-<feature>`|PR + explicit change|
|Follow-up in same chat|[follow-up-deep-dive](references/follow-up-deep-dive.md) — Write before reply; chat pointer only|

## Hard rules

- User invoke only; orchestration never auto-runs this skill
- Walkthrough under `artifacts/assisted-review/` and `canvases/`; gate output under `artifacts/openspec-reviews/<change>/`
- Advisory look outs only — gate-only session → `/tacos-apply-review` or `/tacos-spec-review`

## Done when

- Session bundle: `_full.patch` + `_session.md` per [diff-input](references/diff-input.md)
- Markdown companion with full P0/P1 per [walkthrough-document](references/walkthrough-document.md)
- Canvas when host present; chat pointer-only per [output-delivery](references/output-delivery.md)
- Follow-up: appended via Write before reply per [follow-up-deep-dive](references/follow-up-deep-dive.md)

## References

[diff-input](references/diff-input.md) · [change-binding](references/change-binding.md) · [review-delegation](references/review-delegation.md) · [walkthrough-document](references/walkthrough-document.md) · [output-delivery](references/output-delivery.md) · [follow-up-deep-dive](references/follow-up-deep-dive.md)
