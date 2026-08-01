# OpenSpec validate and main-spec dedrift

Lifecycle tables for **sync**, **archive**, and **verify**. Hub router: [orchestration-binding.md](../orchestration-binding.md).

## OpenSpec validate (sync, archive, verify)

When `orchestration.enabled` is true: `openspec validate <change-id> --strict --no-interactive`; after **sync**, `openspec validate --specs --strict --no-interactive`. On failure: summarize CLI errors, **fix reported errors** in the change or main specs, re-run until exit `0` before continuing the command (user may waive in chat).

|Command|When|
|-|-|
|**sync**|Validate change before delta→main; fix errors and re-run until pass; `--specs` after sync succeeds (fix main specs or deltas if `--specs` fails). Do not edit main specs until the change validates.|
|**archive**|Validate change before archive move (fix errors and re-run until pass); if sync ran in the same turn, `--specs` before the move.|
|**verify**|Validate change before stock verify heuristics; fix errors and re-run until pass, or list as CRITICAL and continue only if the user waives.|

## Main-spec dedrift pass

When `orchestration.enabled` is true, orchestration **MUST** run main-spec drift **detect** for implicated capabilities on every surface below. When drift is detected, orchestration **MUST** offer reconcile, conform, or skip. When no drift is detected, orchestration **MUST** continue without dedrift prompt and record **no-drift** in that surface's summary. Orchestration **MUST NOT** run dedrift writes automatically without user choice.

On skip, record skip in that surface's summary and continue without invoking `/tacos-dedrift`. On reconcile or conform, delegate to explicit `/tacos-dedrift` flow (preview before write per `../../../tacos-dedrift/SKILL.md`).

Procedure detail: [orchestration-dedrift-pass.md](../../../tacos-dedrift/references/orchestration-dedrift-pass.md). Surface-specific: [verify-hook.md](../../../tacos-dedrift/references/verify-hook.md).

|Surface|When|
|-|-|
|**verify**|After stock verify heuristics and change validate pass|
|**sync**|After stock sync and `openspec validate --specs` pass; **before** project-overview hook|
|**archive**|After change validate pass; **before** archive move; skip duplicate when sync handled dedrift in the same turn|
|**tacos-work**|After apply-review; before human sign-off; when session touched implementation or main specs|
|**staged apply**|After each stage apply-review gate pass and fixes; before **Human review:**; when **## Main-spec drift** reports drift|
|**tacos-ask**|After confirmed small edit writes that touched main specs or implementation|

Combined **sync** + **archive** in one turn: at most one dedrift prompt — after **sync**; **archive** MUST NOT repeat when sync already completed the dedrift step.
