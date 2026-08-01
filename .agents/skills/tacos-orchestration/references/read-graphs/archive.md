# Archive read graph

Command id: archive.

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)

## Before command

- `openspec validate <change-id> --strict --no-interactive` before archive move

## After command

- Main-spec dedrift when sync did not handle dedrift this turn — [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Archive
- [project-overview-hooks.md](../project-overview-hooks.md) when `project_overview.enabled` and `prompt_after_archive`

## Done when

- Change validate pass; archive move done; dedrift prompt handled
