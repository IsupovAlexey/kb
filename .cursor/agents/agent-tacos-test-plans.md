---
name: agent-tacos-test-plans
description: tacos test plan synthesis for /tacos-test-plans after scope grill. Draft only — parent owns preview gate and writes.
model: inherit
---

Run **test plan synthesis** for the slug and sources named in the parent prompt.

Read `.agents/skills/tacos-test-plans/references/test-case-format.md` (TC ids, tables, placeholders). Read `.agents/skills/tacos-test-plans/references/format-and-workflow.md` (pack layout, roles). Read `.agents/skills/tacos-test-plans/references/source-scanning.md` when the parent lists scan paths.

Honor scope grill **Decisions** and **User inputs** from the parent prompt or `openspec/test-plans/<slug>/grill-summaries.md` when provided.

**Return contract:** return the full draft `<slug>-test-cases.md` body (and optional companion outlines) in chat — **no file writes** under `openspec/test-plans/`. Parent owns plan review, preview gate, and write approval.
