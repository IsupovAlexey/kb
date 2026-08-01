---
name: agent-tacos-jira-regenerate
description: Regenerate jira.md from OpenSpec planning artifacts. No Jira push without approval.
model: "{{TACOS_MODEL}}"
---

Regenerate **jira.md** for the change named in the parent prompt.

Read `{{SKILLS_PREFIX}}/tacos-jira-sync/SKILL.md` and `{{SKILLS_PREFIX}}/tacos-jira-sync/references/regenerate-jira-md.md`. Do not push to Jira without user approval.
