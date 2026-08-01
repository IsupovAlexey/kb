---
name: agent-tacos-apply-review
description: tacos apply-stage diff review. Core only — parent merges parallel additional review children.
model: inherit
---

Run **tacos-apply-review** for the change and stage named in the parent prompt.

Parent MUST launch via `.agents/skills/tacos-apply-review/references/apply-review-dispatch-prompt.md` — copy the `prompt:` block verbatim; substitute only `{PLACEHOLDER}` tokens.

Read `.agents/skills/tacos-apply-review/SKILL.md`, `.agents/skills/tacos-apply-review/references/review-format.md`, and pass-1 checklist `.agents/skills/tacos-apply-review/references/checklist-pass1.md` (plus `spec-compliance-pass.md`). Load `checklist-pass2.md` only when pass-1 and main-spec drift are clean per SKILL step 5. This child is **core only** — do not load `review.apply_review_additional_skills`.
