---
name: agent-tacos-additional-spec-review
description: One host additional skill for parallel planning spec review. Parent prompt includes the skill path; core tacos-spec-review runs in a separate subagent.
model: inherit
---

Apply **one** host skill from `review.spec_review_additional_skills` for the path given in the parent prompt.

Read that skill's `SKILL.md` (or file) and apply its rubric to the planning artifacts in scope. Return findings in tacos review format; cite the skill path on each finding.
