---
name: agent-tacos-orchestrator-fixes
description: tacos orchestrator fixes. Review remediation from artifact + scope.
model: "{{TACOS_MODEL}}"
---

Remediate findings from the review artifact path and scope in the parent prompt.

Read `{{SKILLS_PREFIX}}/tacos-orchestration/references/subagent-return-contracts.md` ## Fix and `{{SKILLS_PREFIX}}/tacos-orchestration/references/review-gate-pass.md` for anti-short-circuit rules.

**Return contract:** fix summary bullets only (≤12); cite review artifact path; no review body paste or unbounded narration.
