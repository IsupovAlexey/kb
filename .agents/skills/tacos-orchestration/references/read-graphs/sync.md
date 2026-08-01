# Sync read graph

Command id: sync.

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)
- [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md) when `orchestration.grill_enabled`

## Before command

- `openspec validate <change-id> --strict --no-interactive` before delta→main — [binding-sections/validate-and-dedrift.md](../binding-sections/validate-and-dedrift.md) § OpenSpec validate

## During command

- Triggered grill when `grill.triggers.sync_on_ambiguous_delta` + ambiguous delta — [triggered-grill.md](../../../tacos-grill/references/triggered-grill.md)

## After command

- `openspec validate --specs --strict --no-interactive` after merge
- Main-spec dedrift detect before project overview — [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Sync
- [project-overview-hooks.md](../project-overview-hooks.md) when `project_overview.enabled` and `prompt_after_sync`

## Done when

- Change validate pass; delta merged; specs validate pass; dedrift prompt handled — [binding-sections/validate-and-dedrift.md](../binding-sections/validate-and-dedrift.md)
