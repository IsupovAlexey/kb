---
name: tacos-implementation-conventions
description: >-
  Language-agnostic authoring rubric: extend before invent, cohesive types, KISS/YAGNI, DRY (rule of three), SRP (light), structural maintainability (code judo, spaghetti, wrappers, boundaries, orchestration/atomicity, file decomposition).
  Use when tacos-apply-review loads conventions for implementation diffs, during apply when extending existing code, or when the user asks for KISS, YAGNI, DRY, SRP, or extend before invent.
user-invocable: false
---

# tacos implementation conventions

Authoring guidance for implementation and review — not a substitute for `tacos-apply-review` or `tacos-spec-review`.

## Quick start

- Inputs: Stage diff (paths or unified diff) from `tacos-apply-review`, `/opsx:apply`, or a direct conventions question
- Read order:
  1. [checklist.md](references/checklist.md)
  2. [structural-maintainability.md](references/structural-maintainability.md) — BLOCKER rubric ([§ Reviewer stance](references/structural-maintainability.md#reviewer-stance), duplication, code judo, spaghetti, wrappers, boundaries, orchestration/atomicity, file decomposition)
  3. [authoring.md](references/authoring.md) — extend, simplicity, DRY (~5+ identical-line BLOCKER depth), SRP
- Sample cite: `structural-maintainability.md § Code judo` or `authoring.md § DRY (when to share, when to repeat)`

## Execution contract

When `tacos-apply-review` (or `/opsx:apply` with conventions in scope) loads this skill:

1. Findings — Cite `structural-maintainability.md § <section>`, `authoring.md § <section>`, or the checklist section name. Do not paste full rubric text into the review file.
2. Host standards — Repo-specific rules belong in the consuming project's agent docs and `openspec/config.yaml` `context`; apply-review covers them under Host standards, not this skill. Host layout skills govern extraction **placement**; this skill governs consolidation **timing** (rule of three, ~5+ line BLOCKER) — see [authoring.md § Extraction placement](references/authoring.md#extraction-placement-language-agnostic).

## Done when

- Each finding names a rubric section cite — not pasted rubric prose.
- Host-specific rules defer to apply-review Host standards and project `context` — not restated in this skill.

## References

- [structural-maintainability.md](references/structural-maintainability.md) — structural BLOCKER rubric with examples
- [authoring.md](references/authoring.md) — extend, simplicity, DRY, SRP, naming
- [checklist.md](references/checklist.md) — quick reviewer checklist
- [../tacos-apply-review/SKILL.md](../tacos-apply-review/SKILL.md) — apply-stage diff review (maintainability BLOCKER enforcement)
