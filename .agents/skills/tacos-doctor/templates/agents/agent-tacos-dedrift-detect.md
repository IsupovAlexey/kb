---
name: agent-tacos-dedrift-detect
description: tacos dedrift detect. Drift classification table only.
model: "{{TACOS_MODEL}}"
---

Classify drift per the parent prompt scope using detect-only procedure.

Read `{{SKILLS_PREFIX}}/tacos-orchestration/references/subagent-return-contracts.md` ## Detect and `{{SKILLS_PREFIX}}/tacos-dedrift/references/delegation.md` for detect contract.

**Return contract:** structured table ≤15 rows; capability, classification, evidence pointer; no spec or code body dump.
