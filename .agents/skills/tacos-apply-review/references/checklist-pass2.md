# tacos apply review — pass 2 (code quality)

Use with `tacos-apply-review` `SKILL.md` step 5. Load only after pass 1 and **## Main-spec drift** pass with no open BLOCKER/CRITICAL. Pass 1: [checklist-pass1.md](checklist-pass1.md). Router: [checklist.md](checklist.md).

For narrative rubric detail, read [tacos-implementation-conventions](../../tacos-implementation-conventions/SKILL.md) when a finding needs depth — not required for every line item.

## Pass 2 — Code quality

Run only after pass 1 passes (no open BLOCKER/CRITICAL from pass 1) **and** **## Main-spec drift** passes (no open BLOCKER). Includes maintainability, conventions, host standards, tests, prose quality, and [## Intra-stage cohesion](#intra-stage-cohesion).

## Intra-stage cohesion

Evaluate the **cumulative stage diff** across all implementation tasks in the reviewed stage (full tacos `## N.` section or tacos-work **## Work**). Distinct from [## Cohesion](#cohesion) (type ownership). Intra-stage cohesion runs in apply-review pass 2 only — the parent orchestrator does not pre-scan; delegate via Task **`tacos-apply-review`**.

- [ ] **Duplicate logic** — same or near-same code across tasks in this stage without domain reason → **BLOCKER** when ~5+ identical lines; remediation cites placement ([authoring.md § Extraction placement](../../tacos-implementation-conventions/references/authoring.md#extraction-placement-language-agnostic)) — inline on owner when one production consumer
- [ ] **Superseded dead code** — earlier-task code in the stage diff left behind but no longer called after later tasks → **BLOCKER**
- [ ] **Naming drift** — same concept named differently across tasks in the stage → **MAJOR** (elevate to **BLOCKER** when drift caused duplicate top-level helpers)
- [ ] **Missed shared abstractions** — repeated pattern that should share logic but remains duplicated → **MAJOR** when 2+ production consumers or drift risk; **MINOR** when the only missing step is a single-consumer top-level file (inline on owner satisfies DRY)
- [ ] Taste-only style preferences with no drift or duplication risk → **MINOR**

Record a short **## Intra-stage cohesion** subsection under **## Pass 2: Code quality** when any theme applies; **N/A** when the stage has a single trivial task or docs-only diff.

## Prose quality (planning guidance diffs)

When the diff touches schema artifact instructions, planning templates, spec-review rubric, or grill prompts:

- [ ] Adds or updates guidance per [artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md) (slop patterns)
- [ ] Does not introduce evaluative leads, marketing labels, or imagined contrast in the new guidance text
- [ ] Spec-review rubric requires canonical pattern ids and concrete rewrite suggestions; schema instructions summarize and point only — not a forbidden-word grep

When the diff touches chat-facing skill prose (`SKILL.md` description, `tacos-direct-output`, or orchestration binding kernel lines):

- [ ] Chat voice follows [direct-output.md](../../tacos-direct-output/references/direct-output.md) kernel; carve-outs reachable ≤1 hop; no duplicate full rubric inlined in read graphs

TDD procedure findings run in [checklist-pass1.md](checklist-pass1.md) ## TDD procedure (pass 1); artifact section **## TDD compliance** records pass/fail per slice. Omit when TDD does not apply.

## When the diff is code

- [ ] Key design decisions reflected in code

## Structural maintainability (BLOCKER)

When the diff includes code, evaluate per [structural-maintainability.md](../../tacos-implementation-conventions/references/structural-maintainability.md). Each row is **BLOCKER** when introduced or left unresolved in the stage diff — cite the matching section and include concrete remediation.

- [ ] **Code judo** — no new parallel layer or helper module when extending the canonical owner or reusing an existing helper would clearly keep the diff smaller ([§ Code judo](../../tacos-implementation-conventions/references/structural-maintainability.md#code-judo))
- [ ] **Spaghetti growth** — no opaque multi-step chains, deep nesting without guard clarity, or ad-hoc branches in already-busy control paths ([§ Spaghetti growth](../../tacos-implementation-conventions/references/structural-maintainability.md#spaghetti-growth))
- [ ] **Thin wrappers** — no delegate-only indirection without added behavior, validation, or boundary translation ([§ Thin wrappers](../../tacos-implementation-conventions/references/structural-maintainability.md#thin-wrappers))
- [ ] **Boundary and type cleanliness** — no domain→infrastructure leaks, orchestration in leaf helpers, or contract widening to avoid defining the boundary ([§ Boundary and type cleanliness](../../tacos-implementation-conventions/references/structural-maintainability.md#boundary-and-type-cleanliness))
- [ ] **Orchestration and atomicity** — no gratuitous serialization of independent work; no brittle multi-step durable updates without rollback, compensation, or idempotent recovery; no scattered workflow steps when one named boundary is clearer ([§ Orchestration and atomicity](../../tacos-implementation-conventions/references/structural-maintainability.md#orchestration-and-atomicity))
- [ ] **Canonical layer** — new cross-cutting types justified; persistence, invariants, and orchestration stay on established owners ([§ Canonical layer](../../tacos-implementation-conventions/references/structural-maintainability.md#canonical-layer))
- [ ] **Heuristic file decomposition** — touched file with material diff growth does not add a second unrelated concern domain without split or extract ([§ Heuristic file decomposition](../../tacos-implementation-conventions/references/structural-maintainability.md#heuristic-file-decomposition))
- [ ] **Duplication** — no ~5+ line identical logic blocks without domain reason to keep copies separate — consolidate with placement cite ([§ Duplication](../../tacos-implementation-conventions/references/structural-maintainability.md#duplication); [authoring.md § Extraction placement](../../tacos-implementation-conventions/references/authoring.md#extraction-placement-language-agnostic))

**Mixed diffs:** when the stage diff includes both code and skills/schema/docs, run this section **and** [## Non-code maintainability (BLOCKER)](#non-code-maintainability-blocker) below.

## Non-code maintainability (BLOCKER)

When the diff includes skills, schema, or documentation, evaluate at the same **BLOCKER** bar. Omit code-only rows above when the diff has no code; when code is also present, run both sections.

- [ ] No tangled conditional prose in `SKILL.md` or references (nested MUST/SHALL branches that obscure the default path)
- [ ] No duplicate guidance across sibling references without a single source of truth or explicit cross-link
- [ ] No wrapper skill that only re-exports sibling skills without added procedure, rubric, or host-specific checks

Output uses exact heading `## Non-code maintainability` per [review-format.md](review-format.md).

## Extend before invent

- [ ] Existing types or modules extended where possible
- [ ] New service/layer/abstraction justified in design or review notes
- [ ] No parallel abstraction for the same domain concern

## Cohesion

- [ ] New behavior on the type that already owns the area
- [ ] New top-level type only with clear boundary or design note

## KISS / YAGNI

- [ ] New type or interface without `design.md` note or 2+ real uses
- [ ] Method mixes coordination with low-level logic; unrelated deps in one type
- [ ] Intentional patterns from `context` not flagged as over-engineering
- [ ] No speculative features beyond spec; dead code removed
- [ ] No unnecessary interfaces or indirection; straight-line flow

## DRY (minimal, reviewable diffs)

Duplicate or near-duplicate logic makes staged review harder — flag when the diff adds review surface without domain reason. DRY governs **whether** to consolidate; [authoring.md § Extraction placement](../../tacos-implementation-conventions/references/authoring.md#extraction-placement-language-agnostic) governs **where** — ask production consumer count before recommending a new top-level file.

- [ ] Shared helper or constant only after rule of three or clear drift risk
- [ ] No identical logic blocks (~5+ lines) repeated without consolidation — **BLOCKER** when copies are truly identical without domain reason (see [## Structural maintainability (BLOCKER)](#structural-maintainability-blocker)); single consumer → inline on owner, not new file
- [ ] Before MAJOR for "extract helper": count production consumers; one consumer → private on owner satisfies DRY
- [ ] Repeated validation/guard sequences consolidated when drift risk is real (placement per extraction table)
- [ ] Magic literals centralized when used in many places
- [ ] Similar-but-distinct domain logic not forced into one abstraction
- [ ] Copy-paste with slight edits that should share logic — consolidate inline when one production consumer; shared top-level module only with 2+ consumers
- [ ] Duplication of same concern not left across unrelated types

## SRP

- [ ] Type describable in one sentence; dependencies serve one concern
- [ ] Methods at one abstraction level (coordinate or compute, not both)
- [ ] No very long methods mixing unrelated steps without delegation
- [ ] Entry points validate → delegate → return; domain types avoid direct infrastructure
- [ ] Orchestration carve-outs respected (coordinators, mappers, test classes)

## Naming and comments

- [ ] Names consistent with peers in the same area
- [ ] Names describe responsibility, not a single consumer
- [ ] Comments explain non-obvious “why,” not obvious “what”

## Host standards

- [ ] Follows repo `AGENTS.md` and `openspec/config.yaml` `context`
- [ ] Project structure and naming match peers
- [ ] **Implementation gates** — when `AGENTS.md` has **non-empty** commands between `<!-- tacos-implementation-gates-begin -->` and `<!-- tacos-implementation-gates-end -->`, and a gates-listed local dev command failed during the stage with output saved under `artifacts/outputs/` (or the host's documented artifacts path): report **BLOCKER** citing the **failing command** and **log path**. Do **not** emit a gates BLOCKER when the block is empty or `<!-- tacos-doctor-discovery: empty -->`.

## Tests

- [ ] New/changed behavior covered per host test norms
- [ ] Edge and error paths included where behavior matters
- [ ] Test type appropriate (unit vs integration vs e2e) per host docs
- [ ] **BLOCKER** when a **Verify Decision N** row or stage acceptance names a negative/error path or pipeline obligation and no matching automated test or cited existing coverage exists ([pipeline-and-verification.md](pipeline-and-verification.md))
- [ ] **MAJOR** when verification done-when is vague without named test or trace criterion

## Severity

- **BLOCKER** — must fix before stage sign-off (spec violation, broken behavior, **structural maintainability regression** per [## Structural maintainability (BLOCKER)](#structural-maintainability-blocker) or [## Non-code maintainability (BLOCKER)](#non-code-maintainability-blocker))
- **CRITICAL** — should fix before proceeding
- **MAJOR** — should fix soon
- **MINOR** — nice to have

**Readiness:** **Not ready** when any maintainability **BLOCKER** remains — spec/behavior compliance alone does not yield **Ready** or **Ready after fixes**. Maintainability **BLOCKER**s MUST be fixed in the same stage before **Human review:**. Detail: [review-format.md](review-format.md) ## Maintainability readiness invariant.
