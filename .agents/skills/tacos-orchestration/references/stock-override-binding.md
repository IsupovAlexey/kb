# Stock override binding (mandatory)

When `orchestration.enabled` is true:

- Stock OpenSpec CLI steps and host opsx command/prompt files are incomplete for tacos.
- MUST NOT follow stock opsx or host opsx steps when they conflict with tacos-orchestration or tacos-grill.
- MUST NOT use stock "prefer reasonable decisions" (or inline guess) instead of tacos-grill when `orchestration.grill_enabled` is true — STOP and run grill per [binding-sections/stock-overrides-matrix.md](binding-sections/stock-overrides-matrix.md).
- MUST load [binding-sections/stock-overrides-matrix.md](binding-sections/stock-overrides-matrix.md) before inferring workflow steps from opsx alone; propose/apply paths load the full matrix on demand per read graph — not the hub router file.
- Hub wins on conflict with read graphs or slim indexes.

Full stock-vs-tacos matrix: [binding-sections/stock-overrides-matrix.md](binding-sections/stock-overrides-matrix.md).
