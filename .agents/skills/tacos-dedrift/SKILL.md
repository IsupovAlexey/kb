---
name: tacos-dedrift
description: >-
  Reconciles or conforms main openspec/specs/ with shipped code on /tacos-dedrift;
  scheduled unattended reconcile per references/scheduled-job-prompt.md; ambient
  in-session spec updates when behavioral direction is clear. Use when apply-review,
  orchestration workflow surfaces, repo-wide implementation edits, or the user
  reports main-spec drift.
user-invocable: true
argument-hint: >-
  [reconcile|conform] [capability ...|all] [deep [N]] (reconcile-only; e.g. reconcile apply-review deep)
---

# tacos dedrift

Align canonical main specs (`openspec/specs/<capability>/spec.md`) with shipped behavior. Behavioral obligations only — scope table in [modes](references/modes.md) ## Comparison scope.

## Entry

1. Resolve invoke path:
   - explicit — user `/tacos-dedrift` or natural-language dedrift intent
   - ambient — implementation changes outside explicit invoke; [ambient-heuristics](references/ambient-heuristics.md)
   - orchestration-hook — workflow surface when `orchestration.enabled`; [orchestration-dedrift-pass](references/orchestration-dedrift-pass.md)
2. Load path bundle:
   - explicit — optional [lens preflight](references/lens-preflight.md) before mode/scope; when `deep` token present load [deep-mode](references/deep-mode.md); then ## Explicit invoke procedure + [preview-gate](references/preview-gate.md) (preview gate ≤1 hop)
   - ambient — [ambient-heuristics](references/ambient-heuristics.md); no preview when direction clear
   - orchestration-hook — [orchestration-dedrift-pass](references/orchestration-dedrift-pass.md); [verify-hook](references/verify-hook.md) on verify
3. Structured gates: [structured-gate-convention](../tacos-orchestration/references/structured-gate-convention.md); plain-text fallback per [interview-prompt](../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable.

## Quick start

|User says|Action|
|-|-|
|`/tacos-dedrift reconcile apply-review deep`|Deep re-detect loop → verify re-detect → one preview → write ([deep-mode](references/deep-mode.md))|
|`/tacos-dedrift reconcile all deep 10`|Deep loop; max 10 passes (override `dedrift.deep_max_iterations`)|
|`/tacos-dedrift reconcile apply-review`|Detect → preview → structured gate → write ([preview-gate](references/preview-gate.md))|
|`/tacos-dedrift conform post-artifact-orchestration`|Propose code edits to match main spec obligations|
|`/tacos-dedrift reconcile all`|All capabilities; batch detect when count > threshold|
|`/tacos-dedrift` (no mode or scope)|Mode + scope via structured prompt before detect|
|`/tacos-dedrift reconcile` (mode only, no scope)|Capability scope or confirm `all` before detect|
|Scheduled dedrift job|[scheduled-job-prompt](references/scheduled-job-prompt.md) — unattended reconcile, draft PR; depth from `dedrift.scheduled_depth` or host overlay|
|Behavioral code change (ambient)|[ambient-heuristics](references/ambient-heuristics.md)|

## Gates

Preview → structured prompt (next tool call MUST be AskQuestion / AskUserQuestion when available) → else plain text + end turn.

|Step|Before writes|Option ids / binding|
|-|-|-|
|Mode / scope|Detect when omitted|per [modes](references/modes.md)|
|Preview|Spec or code edits|`proceed` / `edit` / `cancel` — [preview-gate](references/preview-gate.md)|

Ambient path: no preview gate when behavioral direction is clear — [ambient-heuristics](references/ambient-heuristics.md).

## Explicit invoke procedure

1. Parse — mode, capability scope, and optional `deep` token from args or structured prompt. Contract: `/tacos-dedrift [reconcile|conform] [capability ...|all] [deep [N]]`. Reject `conform … deep` per [deep-mode](references/deep-mode.md). Mode MUST be chosen before preview when omitted.
2. Detect — when `deep`, parent loop per [deep-mode](references/deep-mode.md); else compare each scoped capability to codebase per [modes](references/modes.md). Multi-capability: [delegation](references/delegation.md).
3. Preview — show all proposed spec and code changes; structured approval per [preview-gate](references/preview-gate.md). No writes until `proceed` (or explicit proceed-after-edit).
4. Write — parent serializes all file changes after confirm; subagents detect only.
5. Report — chat summary + [output artifact](references/output-format.md).

On Cancel: state cancelled; no pending preview; no writes.

## Ambient obligation

When implementation changes outside explicit `/tacos-dedrift` as the direct result of a user prompt, follow [ambient-heuristics](references/ambient-heuristics.md): clear behavioral direction → update main specs in-session and notify in chat; ambiguous → offer `/tacos-dedrift`, no silent spec writes.

## Orchestration hooks

When `orchestration.enabled` is true, parent runs optional main-spec dedrift detect on workflow surfaces — [orchestration-dedrift-pass](references/orchestration-dedrift-pass.md).

## Split from apply-time drift

|Surface|Scope|
|-|-|
|`grill.triggers.apply_on_spec_drift` during change-folder apply|Change-folder planning artifacts|
|tacos-dedrift|Main `openspec/specs/**` vs codebase post-hoc|

## Done when

- Explicit invoke: mode and scope chosen; detect completed; preview with structured gate per [preview-gate](references/preview-gate.md); writes only after `proceed`; chat summary and [output artifact](references/output-format.md) written (or user waived artifact).
- Cancel: cancelled stated; no pending preview; no writes.
- Ambient: main specs updated when behavioral direction was clear; ambiguous cases offered `/tacos-dedrift` with no silent spec writes.
- Orchestration hooks: parent recorded no-drift or user reconcile/conform/skip per [orchestration-dedrift-pass](references/orchestration-dedrift-pass.md) before continuing the workflow surface.

## References

[modes](references/modes.md) · [deep-mode](references/deep-mode.md) · [lens-preflight](references/lens-preflight.md) · [preview-gate](references/preview-gate.md) · [scheduled-job-prompt](references/scheduled-job-prompt.md) · [ambient-heuristics](references/ambient-heuristics.md) · [delegation](references/delegation.md) · [output-format](references/output-format.md) · [orchestration-dedrift-pass](references/orchestration-dedrift-pass.md)
