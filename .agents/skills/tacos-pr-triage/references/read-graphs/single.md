# Pr-triage — single pass

Default when invoke has no `loop` or `bypass` tokens.

## MUST read

- [gate-budget.md](../gate-budget.md) ## Agent procedure (one turn = one gate) — track tables load with checks/comments graphs
- [check-sync.md](../check-sync.md)
- [fetch-and-sync.md](../fetch-and-sync.md)
- [suppressed-review-comments.md](../suppressed-review-comments.md) — § Agent checklist (mandatory); not satisfied by thread-only sync
- [output-paths.md](../output-paths.md)
- [change-binding.md](../change-binding.md) — pr-triage modes only; on tie or auto-bind load [assisted-review change-binding § Strong evidence / Multiple candidates / Heuristic confirm](../../../tacos-assisted-review/references/change-binding.md#strong-evidence-auto-bind) only
- `openspec/tacos.yaml` — `pr.descriptions_root`, optional `pr.sonar_*`, `pr.loop_*`

## Procedure (single pass)

1. Set `loop_mode: false`, `bypass_mode: false` in `_session.md`
2. Sync per [check-sync.md](../check-sync.md) (1a) then [fetch-and-sync.md](../fetch-and-sync.md) (1b) — preflight and author ownership in those refs only
3. When failing checks → load [checks.md](checks.md); optional [sonar.md](sonar.md) when Sonar detected
4. Comment track → load [comments.md](comments.md)
5. When not composed by [loop.md](loop.md): stop after one pass

## Done when

- Gated gates got preview + one structured prompt per [gate-budget.md](../gate-budget.md) ## Agent procedure (one turn = one gate)
- `_session.md` under `{descriptions_root}/<branch-slug>/pr-triage/` reflects latest state
