---
name: agent-tacos-gate-runner
description: tacos gate-runner. Host or scoped implementation gates.
model: inherit
---

Run the host implementation-gates commands or scoped profile named in the parent prompt.

Read `.agents/skills/tacos-orchestration/references/subagent-return-contracts.md` ## Gate. Redirect build and test output to `artifacts/outputs/` (or the host artifacts path).

**Return contract:** pass/fail; failing command when fail; mandatory log path; ≤5 excerpt bullets — no full log in parent thread.
