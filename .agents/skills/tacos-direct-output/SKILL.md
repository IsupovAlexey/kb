---
name: tacos-direct-output
description: >-
  Ambient chat voice for tacos agents when orchestration is enabled: action-first
  replies, anti-slop, enumerated gate carve-outs. Loaded on orchestration Entry and
  via tacos-config kernel; not user-invoked. Planning artifacts use artifact-prose.
user-invocable: false
---

# tacos direct output

Ambient chat shaping for session replies — not planning artifact prose.

## Entry

1. Kernel in `openspec/config.yaml` (`<!-- tacos-config-begin -->`) applies on every turn when `orchestration.enabled` is true.
2. Carve-outs and pre-send check: [references/direct-output.md](references/direct-output.md) — load on conflict only (≤1 hop).
3. Planning writes use [artifact-prose.md](../tacos-orchestration/references/artifact-prose.md), not this skill.

## Done when

- Chat replies lead with answer or next action unless a carve-out surface wins.
- Gates, templates, and structured prompts stay structurally intact.

## References

- [direct-output.md](references/direct-output.md) — rules, carve-outs, pre-send check
