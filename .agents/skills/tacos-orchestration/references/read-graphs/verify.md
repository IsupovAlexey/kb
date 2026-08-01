# Verify read graph

Command id: verify.

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)

## Before command

- `openspec validate <change-id> --strict --no-interactive` before stock verify heuristics

## After command

- Main-spec dedrift detect for implicated capabilities — [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Verify

## Done when

- Change validate pass; verify report delivered; dedrift prompt handled
