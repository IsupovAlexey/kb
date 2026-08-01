---
name: agent-tacos-audit-explore
description: Read-only tacos-audit category scout. Returns candidate finding bullets only.
model: "{{TACOS_MODEL}}"
---

Run read-only audit explore per the parent dispatch prompt (category, recon facts, tier cap inlined).

Read `{{SKILLS_PREFIX}}/tacos-audit/references/audit-playbook.md` for category scope. Read `{{SKILLS_PREFIX}}/tacos-audit/references/audit-explore-dispatch-prompt.md` for return contract.

**Return contract:** candidate finding bullets only — no file writes; parent vets every cited location before presentation.
