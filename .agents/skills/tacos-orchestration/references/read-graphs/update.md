# Update read graph

Command id: update.

## MUST read

- [stock-override-binding.md](../stock-override-binding.md)
- [update.md](../../../tacos-grill/references/read-graphs/update.md) + [grill-gates.md](../grill-gates.md) § Update grill when `orchestration.grill_enabled`
- [artifact-prose.md](../artifact-prose.md)
- [entry-conditionals.md](../entry-conditionals.md) matching rows

## Before command

- STOP if `grill.update` is `pending` (fresh revision) — run **tacos-grill** update phase first; skip when `grill.update` is `complete` or `skipped` (re-entry on same change)
- Redirect to **propose** when scope-pivot (new change intent, not refinement) or missing-frontier (target artifacts/files from `existingOutputPaths` do not exist — stock **update** cannot create them); do not write new artifact files on redirect
- **Stay on update** when user refines existing planning artifacts in place (coherence pass, targeted revision)

### Redirect examples

|Branch|User signal|tacos action|
|-|-|-|
|Scope-pivot → **propose**|"Scrap this change and build OAuth from scratch instead of refining the API-key design"|STOP **update**; recommend **propose** (or **new** + planning path) — do not rewrite artifacts as refinement|
|Missing-frontier → **propose**|User asks to add `design.md` but `openspec status` shows design blocked / no `existingOutputPaths`|STOP **update**; recommend **propose** or **continue** — stock **update** must not create missing artifacts|
|Stay on **update**|"Align tasks with the revised design section" when `design.md` and `tasks.md` exist|Run update grill when pending → reconcile existing files only|

## During command

- Stock reconcile: edit only `artifactPaths.<id>.existingOutputPaths`; per-artifact user confirm before each write (preserve stock guardrail)
- Before each artifact write: [artifact-prose.md](../artifact-prose.md)
- Before writing **proposal** / **specs** / **tasks** during reconcile: [planning-artifact-loop.md](../planning-artifact-loop.md) ## Generation-time contract — same gates as propose/ff (Modified Capabilities research, specs main-spec **STOP**, FORBIDDEN Verify Decision embed, Apply review **medium** line — not one-line pointer)
- Before writing **tasks** during reconcile: also [task-stage-contract.md](../task-stage-contract.md) ## Per-stage checklist order
- When behavioral claims in delta specs change: **tacos-spec-grounding** per [spec-grounding-explore.md](../spec-grounding-explore.md) before spec writes
- Apply-in-progress: when implementation tasks are checked off and revised planning changes behavioral obligations — full-resync affected planning artifacts + `grill-summaries.md`; STOP with explicit **apply** handoff; MUST NOT edit implementation code in the **update** turn

## After command

- When apply-ready after reconcile: same-turn POST-ARTIFACT — load [post-artifact-index.md](../post-artifact-index.md) first; step bundles on demand (parity with **propose** / **ff** / **continue**)
- E2E / spec review / validate / sign-off per toggles in post-artifact-index

## Done when

- `grill.update` complete or skipped (or skipped on re-entry); reconcile complete; no implementation code edited; POST-ARTIFACT gates pass when apply-ready

## On demand

- [binding-sections/stock-overrides-matrix.md](../binding-sections/stock-overrides-matrix.md) — **update** rows before reconcile writes
- [orchestration-binding-index.md](../orchestration-binding-index.md) — **update** section when apply-ready
- [post-artifact-planning-review.md](../post-artifact-planning-review.md) — when planning spec review step runs
- [post-artifact-signoff.md](../post-artifact-signoff.md) — when human sign-off runs

## Hub authority

[orchestration-binding.md](../orchestration-binding.md) wins on conflict with this index.
