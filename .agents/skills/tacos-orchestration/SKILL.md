---
name: tacos-orchestration
description: >-
  Governs tacos OpenSpec workflow commands (propose, ff, continue, apply, sync,
  explore, verify, archive, update) and POST-ARTIFACT gates when openspec/tacos.yaml
  orchestration.enabled is true. Loaded before stock OpenSpec extension commands (opsx) steps; not user-invoked
  directly.
user-invocable: false
---

# tacos orchestration

Do not invoke directly for workflow commands. When `orchestration.enabled` is true, start here before stock [OpenSpec extension commands (opsx)](references/openspec-commands.md).

Ad-hoc artifact edits outside `/opsx:*`: [artifact-editing.md](references/artifact-editing.md) only (also host `AGENTS.md`).

## Stock override binding (mandatory)

MUST NOT follow stock opsx when conflicting. MUST NOT use stock "prefer reasonable decisions" instead of tacos-grill when `orchestration.grill_enabled` is true. Full block: [stock-override-binding.md](references/stock-override-binding.md). Matrix on demand: [binding-sections/stock-overrides-matrix.md](references/binding-sections/stock-overrides-matrix.md). Config keys: [config-notation.md](references/config-notation.md).

## Entry (always first)

1. Read `openspec/tacos.yaml`. If missing or `orchestration.enabled` is false, exit — stock behavior only.
   - When `orchestration.enabled` is true, load [../tacos-direct-output/SKILL.md](../tacos-direct-output/SKILL.md) Entry (ambient chat voice; kernel also in tacos-config).
2. Normalize command id per [openspec-commands.md](references/openspec-commands.md).
3. Load command read graph:

- explore — [read-graphs/explore.md](references/read-graphs/explore.md)
- propose, ff, continue — [read-graphs/propose.md](references/read-graphs/propose.md)
- update — [read-graphs/update.md](references/read-graphs/update.md)
- apply — [read-graphs/apply.md](references/read-graphs/apply.md)
- sync — [read-graphs/sync.md](references/read-graphs/sync.md)
- verify — [read-graphs/verify.md](references/read-graphs/verify.md)
- archive — [read-graphs/archive.md](references/read-graphs/archive.md)
- `/tacos-work` — [read-graphs/tacos-work.md](references/read-graphs/tacos-work.md)

4. Follow graph MUST read + Before / During / After / Done when sections; load [entry-conditionals.md](references/entry-conditionals.md) rows when the graph references them.
5. Load [orchestration-binding.md](references/orchestration-binding.md) sections the graph lists on demand.
6. Run stock opsx steps as modified by the active read graph — not stock column when conflicting.

## Jira

When `jira.enabled` or Jira link: [jira-hooks.md](references/jira-hooks.md). Approval: [approval-prompt.md](references/approval-prompt.md).
