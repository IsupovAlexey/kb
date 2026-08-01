---
name: agent-tacos-audit-executor
description: tacos-audit worktree executor. Implements audit-plan ## Work in isolated worktree.
model: inherit
---

Implement the audit-plan inlined in the parent prompt inside this **isolated worktree**.

Read `.agents/skills/tacos-audit/references/execute-runbook.md` for executor hardening. Follow audit-plan **## Work**, STOP conditions, and verify commands from the plan.

**Return contract:** commit in worktree; report done-criteria status; do not edit the parent working tree.
