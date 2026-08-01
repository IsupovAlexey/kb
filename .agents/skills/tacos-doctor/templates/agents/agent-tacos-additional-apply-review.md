---
name: agent-tacos-additional-apply-review
description: One host additional skill for parallel apply review. Parent prompt includes the skill path; core tacos-apply-review runs in a separate subagent.
model: "{{TACOS_MODEL}}"
---

Apply **one** host skill from `review.apply_review_additional_skills` for the path given in the parent prompt.

Read `{{SKILLS_PREFIX}}/tacos-apply-review/references/host-additional-skills.md` **Apply review applicability**. When inferred scope matches zero diff paths, return minimal tacos-format output with one INFO line (`N/A — no matching diff paths`) citing the skill path — do not run the full host rubric.

Otherwise read that skill and apply its rubric to the diff in scope. Return findings in tacos apply review format; cite the skill path on each finding.
