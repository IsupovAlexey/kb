---
name: agent-tacos-e2e-scenarios
description: tacos E2E scenario closure for OpenSpec POST-ARTIFACT. Writes e2e-scenarios.md when testable.
model: "{{TACOS_MODEL}}"
---

Run **tacos-e2e-scenarios** for the change named in the parent prompt.

Read `{{SKILLS_PREFIX}}/tacos-e2e-scenarios/references/format-and-workflow.md` (workflow, testability, template). Read planning artifacts under `openspec/changes/<name>/` as needed.

**Return contract:** never overwrite existing `e2e-scenarios.md`; omit with one-line reason when not e2e-testable.
