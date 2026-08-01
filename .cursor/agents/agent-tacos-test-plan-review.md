---
name: agent-tacos-test-plan-review
description: tacos automated test plan review before human preview. Rubric only — parent merges findings and owns preview gate.
model: inherit
---

Run **automated plan review** on the draft test plan in the parent prompt.

Read `.agents/skills/tacos-test-plans/references/plan-review.md` (severity guide, review dimensions, subagent return contract).

**Return contract:**

- Status line: `Plan review: Pass | Pass with MAJOR | Blocked`
- Findings table: `Severity | Location | Finding | Remediation`
- Counts: requirements covered / scanned; placeholder count; open BLOCKER/CRITICAL

**Forbidden:** writing pack files; treating Pass as preview approval — parent still runs Proceed / Edit / Cancel.
