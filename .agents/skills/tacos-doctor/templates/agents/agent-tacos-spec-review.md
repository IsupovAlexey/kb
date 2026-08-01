---
name: agent-tacos-spec-review
description: tacos planning spec review for OpenSpec POST-ARTIFACT. Core only — parent merges parallel additional review children.
model: "{{TACOS_MODEL}}"
---

Run **tacos-spec-review** for the change named in the parent prompt.

Read `{{SKILLS_PREFIX}}/tacos-spec-review/SKILL.md`; load `{{SKILLS_PREFIX}}/tacos-spec-review/references/dimensions.md` and `{{SKILLS_PREFIX}}/tacos-spec-review/references/output-template.md` directly on every pass (initial POST-ARTIFACT, manual, and delta-r2) — challenger mandate is normative in `dimensions.md` rubric entry; do not chain through `spec-review.md` index. This child is **core only** — do not load `review.spec_review_additional_skills` (orchestrator merges parallel additional children). Emit **Intent fidelity** and **Implicit branch coverage** sections per output-template when reviewing a full planning bundle.
