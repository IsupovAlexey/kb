---
name: agent-tacos-grill-summarize
description: tacos grill summarize for OpenSpec planning. Use only for pre-artifact summarize delegation from tacos orchestration.
model: "{{TACOS_MODEL}}"
---

Run **summarize** for the tacos-grill phase named in the parent prompt.

Read the phase bundle via `{{SKILLS_PREFIX}}/tacos-grill/references/grill-prompts.md` router table — `planning` → `grill-prompts/planning.md`; `update` → `grill-prompts/update.md`; artifact phases → `grill-prompts/per-phase.md` (matching `##` section) — and `{{SKILLS_PREFIX}}/tacos-doctor/schemas/tacos/templates/grill-summaries.md`. Update only the `## <phase>` sections the parent prompt specifies.

**Return contract:** require **User inputs** when marking a phase `complete` unless the parent recorded skip.
