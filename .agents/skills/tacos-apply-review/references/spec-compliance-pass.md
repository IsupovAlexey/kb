# Spec-compliance pass (apply review pass 1)

Use with `tacos-apply-review` `SKILL.md` **before** the code-quality pass. Checklist rows: [checklist-pass1.md](checklist-pass1.md) ## Pass 1 — Spec compliance. Pipeline trace when applicable: [pipeline-and-verification.md](pipeline-and-verification.md).

## Purpose

Answer **did we build the right thing** for this stage — traceability to planning artifacts and scenario coverage — without mixing style, maintainability, or convention findings.

## Scope (pass 1 only)

In scope (pass 1):

- Requirements and scenarios from change specs (full tacos) or main `openspec/specs/**` + work plan (tacos-work)
- Traceability to `tasks.md` and stage acceptance — **current** `## N.` stage only for full tacos staged apply
- Design **Decision N** and **Verify Decision N** obligations for the stage
- Layered compliance — behavioral SHALL/MUST vs design/tasks detail ([checklist-pass1.md](checklist-pass1.md) ### Layered compliance)
- Scope creep — untracked work outside stage tasks; not obligations explicitly deferred to later stages ([checklist-pass1.md](checklist-pass1.md) ### Layered compliance stage deferral row)
- Pipeline trace and gate-before-I/O when planning documents ordering
- TDD procedure evidence when `tdd.md` exists or tacos-work TDD rows apply — batched RED, refactor assessment, red paste
- QA test plan read-only boundary when the diff touches `openspec/test-plans/**` — [checklist-pass1.md](checklist-pass1.md) ## QA test plans (pass 1)
- Planning traceability — load per Input; cite planning paths in findings; no memory-only obligations

Out of scope (pass 2 — [checklist-pass2.md](checklist-pass2.md)):

- Naming, comments, formatting
- KISS/YAGNI, DRY, SRP
- Structural maintainability **BLOCKER**s
- Non-code maintainability **BLOCKER**s
- Prose-quality slop patterns ([artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md))
- Host implementation-gates command failures (pass 2 host standards)
- Test coverage gaps unrelated to TDD procedure
- Style, cohesion, or maintainability findings

## Procedure

1. **Planning context** — load per session mode ([SKILL.md](../SKILL.md) ## Input). Trace pass-1 findings to those artifacts. **Cite planning file paths** (requirement, scenario, design Decision N, task row) in findings. **Do not** use conversation memory as the **sole** source for obligations. Re-read from disk when context is stale, on re-review after planning edits, or when traceability is uncertain.
2. **Per-item obligation inventory (mandatory)** — before classifying findings, enumerate every concrete obligation in scope, then assess each line pass/fail:

   - Stage or session implementation checkboxes (exclude terminal Apply review / Human review lines unless verifying review procedure itself)
   - Each `### Requirement:` and each `#### Scenario:` implicated by the diff
   - Each design **Decision N** and each **Verify Decision N** row for the stage or session
   - Each proposal non-goal or scope boundary that could be violated by the diff

   Record the inventory under **## Obligation inventory** in the review artifact ([review-format.md](review-format.md)). Do not batch-assess without listing items first; unlisted pass is a procedure violation.

3. Read the stage diff; map each material change to a requirement, design decision, or task checkbox. For staged apply, treat unchecked tasks in later `## N.` stages and explicit design deferrals as out of scope for **BLOCKER** / **CRITICAL** — cite **Deferred** with target stage when noting them.
4. Run [checklist-pass1.md](checklist-pass1.md) ## Pass 1 — Spec compliance rows (including [## TDD procedure (pass 1)](#tdd-procedure-pass-1) when TDD applies).
5. After pass-1 findings, run [checklist-pass1.md](checklist-pass1.md) ## Main-spec drift detect — apply sync-pending exception before elevating stale main to **BLOCKER**.
6. When pipeline/gate ordering applies, run [pipeline-and-verification.md](pipeline-and-verification.md) and record under **## Pass 1: Spec compliance** in the review artifact.
7. Classify findings **BLOCKER** / **CRITICAL** / **MAJOR** / **MINOR** for pass-1 themes only.
8. **Hard stop:** If any open **BLOCKER** or **CRITICAL** remains from pass 1 (main-spec drift excluding sync-pending only), **MUST NOT** start pass 2. Record pass-1 status in the artifact; set overall Summary to **NEEDS REVISION** / **Not ready** with matching **Must address** rows.

## TDD procedure (pass 1)

When `tdd.md` exists (full tacos) or tacos-work **Intent** / **Planning** opted into TDD, evaluate procedure in pass 1 — not pass 2. Detail: [checklist-pass1.md](checklist-pass1.md) ## TDD procedure (pass 1); per-test rules in [red-green-refactor.md](../../tacos-tdd/references/red-green-refactor.md).

## Pass 1 outcome labels

Record in the review artifact under **## Pass 1: Spec compliance**:

- **Pass** — no open BLOCKER/CRITICAL from pass 1; proceed to code-quality pass
- **Fail** — open BLOCKER or CRITICAL from pass 1; **Pass 2: Skipped** until remediated and re-reviewed

## Finding tone

Cite the planning source (requirement title, scenario, design Decision N, task row):

```text
BLOCKER | tacos-apply-review/SKILL.md | Missing sequential pass workflow — violates Requirement: Sequential spec-compliance then code-quality passes
CRITICAL | src/Foo.cs:12 | Untracked behavior — no task or spec trace for new validation branch
```

## Cap

Soft cap ~8 actionable pass-1 items; overflow in **Deferred** with theme counts. Pass 2 has its own cap per [review-format.md](review-format.md).
