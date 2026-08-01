# tacos apply review checklist

Router for pass-scoped bundles. Use with `tacos-apply-review` `SKILL.md`. Host-specific items: list skill paths under `review.apply_review_additional_skills` in `openspec/tacos.yaml`, or add bullets in `openspec/config.yaml` `context` — not in tacos core.

- **Pass 1 (initial load)** — [checklist-pass1.md](checklist-pass1.md) + [spec-compliance-pass.md](spec-compliance-pass.md)
- **Pass 2 (after pass 1 clean)** — [checklist-pass2.md](checklist-pass2.md); load [tacos-implementation-conventions](../../tacos-implementation-conventions/SKILL.md) when narrative depth is needed

## Reviewer verification

Before closing the artifact, confirm each row:

|Check|Pass when|
|-|-|
|Delegation|Review ran via Task subagent when supported; re-review after fixes used a **fresh** Task child — [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md)|
|Dual-pass order|**Pass 2: Code quality** recorded only after pass 1 and **## Main-spec drift** pass; otherwise **Pass 2: Skipped** with reason|
|Maintainability lane|**Readiness** reflects open structural or non-code **BLOCKER**s — [review-format.md](review-format.md) ## Maintainability readiness invariant|
|Layered compliance|Pass 1 does not BLOCKER/CRITICAL solely for design/tasks deferrals when specs state the governing behavioral rule|
|Non-code lane|**## Non-code maintainability** section present when the diff touches skills, schema, or docs|
|Parallel launch|When `review.apply_review_additional_skills` has applicable paths: core + additional children launched; parent merged — not core-only|
|TDD evidence|When TDD applies: **## TDD compliance** records pass/fail per slice; **Green** has chat-visible red paste or documented waiver|
|Summary alignment|**Status** / **Readiness** match **Must address (BLOCKER / CRITICAL)** and **Should address (MAJOR)** rows — not prose-only blocking|
|Gate pass|Artifact supports orchestrator [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) before **Apply review:** / **Human review:** checkoff|
|Pipeline trace|**## Pipeline trace** filled when planning documents pipeline/gate ordering and the diff touches the entrypoint — [pipeline-and-verification.md](pipeline-and-verification.md)|
|Verify Decision N|Named **Verify Decision N** / pipeline verification obligations met or flagged with severity|

Delegation, severity alignment, and gate-pass rules are not satisfied by narrative alone.

## Dual-pass workflow

Each apply review runs **two sequential passes** in one Task invocation ([spec-compliance-pass.md](spec-compliance-pass.md)):

1. **Pass 1 — Spec compliance** — [checklist-pass1.md](checklist-pass1.md) rows only. Record outcome under **## Pass 1: Spec compliance** in the review artifact.
2. **Pass 2 — Code quality** — [checklist-pass2.md](checklist-pass2.md) rows only when pass 1 has **no open BLOCKER/CRITICAL**. Record under **## Pass 2: Code quality**. When pass 1 fails, write **Pass 2: Skipped** with reason.

**Verify Decision 3:** Pass 2 **MUST NOT** run until pass 1 completes with no open BLOCKER/CRITICAL **and** the **## Main-spec drift** lane passes (no open BLOCKER).

For full tacos staged apply, use **Pass 1** with the change folder planning set.

## Work binding (`tacos-work`)

When the review is for a **tacos-work** session (not `openspec/changes/<name>/`), see [checklist-pass1.md](checklist-pass1.md) ## Work binding.
