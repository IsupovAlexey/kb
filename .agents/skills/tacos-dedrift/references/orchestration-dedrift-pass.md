# Orchestration main-spec dedrift pass

Mandatory main-spec drift **detect** on orchestration surfaces when `orchestration.enabled` is true; user-choice reconcile/conform/skip when drift is found. Binding: [orchestration-binding.md](../../tacos-orchestration/references/orchestration-binding.md) § Main-spec dedrift pass.

**Not** change-folder apply drift (`grill.triggers.apply_on_spec_drift`). **Not** triggered `grill.triggers.sync_on_ambiguous_delta` (delta-merge semantics).

## Explicit-invoke lens preflight (advisory)

On user `/tacos-dedrift` (not orchestration surfaces below), agents MAY consult optional lens validate/staleness before mode/scope — [lens-preflight.md](lens-preflight.md). Preflight is advisory capability triage only; semantic detect, preview gates, and writes are unchanged. Orchestration hooks in this file MUST NOT require or gate on lens output.

## Shared contract

1. **Detect** — parent **MUST** compare implicated capabilities' main specs under `openspec/specs/<capability>/spec.md` against the codebase for **behavioral** obligations on every orchestration surface below, subject to each surface's scope gates in the sections that follow (same layering as apply-review and apply-time spec drift). When Task spawn is supported, parent **MUST** delegate detect to **`agent-tacos-dedrift-detect`** per [delegation.md](delegation.md) — **no** inline parent detect (including scoped count ≤ 6). When Task is unavailable, parent detects inline and notes inline execution. Detect runs even when no drift is expected where the surface applies.
2. **When drift detected** — parent offers reconcile / conform / skip via structured prompt per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) and [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#orchestration-dedrift-choice); plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools are absent.
3. **When no drift detected** — continue without dedrift prompt; record **no-drift** in that surface's chat summary.
4. **MUST NOT** invoke `/tacos-dedrift` automatically without user choice on any orchestration surface below.
5. **MUST NOT** invoke `/tacos-dedrift` with the `deep` token from orchestration hooks — deep is explicit user invoke only ([deep-mode.md](deep-mode.md)).
6. **On skip** — record skip in that surface's chat summary; continue the workflow without dedrift.
7. **On reconcile** → `/tacos-dedrift reconcile` for implicated capabilities (preview before write per `../SKILL.md`).
8. **On conform** → `/tacos-dedrift conform` for implicated capabilities (preview before write).

## Implicated capabilities

- Capabilities touched by the active change (delta specs, main spec paths, tasks, or implementation paths in session scope).
- **tacos-work** / **tacos-ask** — capabilities whose main specs or implementation paths the session edited or referenced.
- **apply-review** — capabilities implicated by the stage or session diff scope; apply-review **## Main-spec drift** findings inform parent prompt without re-detect when the artifact already reports drift.
- When scope is ambiguous, infer from session changed paths and chat context; use mode/scope structured prompt only when mapping is unclear ([structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md); plain-text only when tools absent).
- Behavioral obligations only — not presentation or inventory detail deferred to design/tasks.

## Combined sync + archive turn

When **sync** and **archive** run in the **same turn**:

- Run at most **one** optional dedrift pass after **sync** (when drift detected or user prompted).
- Set a turn-local flag when sync completes the dedrift step (offered and answered, or no drift detected).
- **archive** in the same turn **MUST NOT** offer a duplicate dedrift prompt when that flag is set.

## Verify

After stock **verify** heuristics and `openspec validate` for the change folder. Detail: [verify-hook.md](verify-hook.md).

## Sync

After stock **sync** succeeds and `openspec validate --specs --strict --no-interactive` passes (fix errors and re-run until pass per orchestration binding).

**Before** [project-overview-hooks.md](../../tacos-orchestration/references/project-overview-hooks.md) when `project_overview.enabled` and `prompt_after_sync`.

Record choice in the **sync** summary. When **archive** follows in the same turn, apply [Combined sync + archive turn](#combined-sync-archive-turn).

## Archive

After change-folder `openspec validate <change-id> --strict --no-interactive` passes (and `--specs` when sync ran in the same turn per binding).

**Before** the archive move.

**Before** [project-overview-hooks.md](../../tacos-orchestration/references/project-overview-hooks.md) when `project_overview.enabled` and `prompt_after_archive`.

Skip when [Combined sync + archive turn](#combined-sync-archive-turn) already handled dedrift. Record choice in the **archive** summary.

## Staged apply

After each stage **Apply review** gate pass and orchestrator fixes/re-runs checks, and **before** **Human review** / human sign-off ([task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md)).

When the merged apply-review artifact **## Main-spec drift** reports **code violation**, or **stale spec** without sync-pending coverage, parent offers reconcile/conform/skip per [Shared contract](#shared-contract) using artifact scope.

When apply-review completed with **## Main-spec drift** listing only **aligned** or **stale spec (pending sync)** (no code violation), treat as **no orchestration drift** — continue to **Human review** without dedrift prompt; record **sync-pending** or **no-drift** in the stage summary.

When apply-review completed with **## Main-spec drift** (aligned or drift), use the artifact — do not run a second orchestration detect pass; apply-review always runs the drift lane.

Record choice in the stage turn summary or stage notes before checking off **Human review:** when dedrift was offered.

## tacos-work

After **Apply review** completes and before **Human review** / human sign-off ([session-runbook.md](../../tacos-work/references/session-runbook.md) Phase 4–5).

When the apply-review artifact **## Main-spec drift** reports **code violation**, or **stale spec** without sync-pending coverage, parent offers reconcile/conform/skip per [Shared contract](#shared-contract) using artifact scope.

When apply-review completed with **## Main-spec drift** listing only **aligned** or **stale spec (pending sync)** (no code violation), continue without dedrift prompt; record **sync-pending** or **no-drift** in the session summary.

When apply-review completed with **## Main-spec drift** (aligned or drift), use the artifact — do not run a second orchestration detect pass; apply-review always runs the drift lane.

Record choice in the session summary or `artifacts/tacos-work/<slug>/tasks.md` Human review note when dedrift was offered.

## tacos-ask

After a **confirmed** small edit write ([small-edits.md](../../tacos-ask/references/small-edits.md)) when the edit touched `openspec/specs/**` or implementation paths outside change-folder-only Q&A.

Record choice in chat before ending the ask turn. Do not write under `artifacts/`.
