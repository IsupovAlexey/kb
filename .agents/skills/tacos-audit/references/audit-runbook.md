# Audit runbook

Phase router for `/tacos-audit`. Report: `tacos-audit [<slug>]: <phase> — <next>`.

## Phases

|Phase|Trigger|Read-only?|Output|
|-|-|-|-|
|Recon|Start of audit|Yes|Recon notes in chat; commands table for plans|
|Explore audit|After recon|Yes|Raw child bullets (internal)|
|Vet|After explore merge|Yes|Vetted findings table → `findings.md`|
|Plan|`/tacos-audit plan`|Yes until Proceed|`audit-plan.md` / `plans/`|
|Review-plan|`/tacos-audit review-plan <file>`|Yes until Proceed|Revised plan file|
|Reconcile|`/tacos-audit reconcile <slug>`|Yes|`index.md` refresh; drift report in chat|
|Execute|`/tacos-audit execute`|No (worktree)|Code + apply-review|
|Handoff|Preview confirm + `handoff` ≠ execute|Depends|Downstream skill|

## Slug

`<slug>` — kebab-case session id under `artifacts/tacos-audit/<slug>/`. Default from audit topic + date or user-provided on plan/execute.

## Recon (parent)

1. Read README, AGENTS.md, CONTRIBUTING, root configs, CI, directory structure.
2. Identify build/test/lint commands from docs and CI — record in recon notes; do **not** execute during recon (mutating commands belong in execute worktree or explicit user-run verification).
3. Ingest intent docs when present: `docs/adr/`, `CONTEXT.md`, `DESIGN.md`, `PRODUCT.md`.
4. Run dedrift **detect** heuristics on touched capabilities — surface `handoff: dedrift` findings; do not auto-reconcile.
5. `git log --oneline -30` and churn hotspots for tier weighting.
6. Record recon summary in chat before launching explore children.

## Explore audit (parallel)

1. Choose tier: `quick` | `standard` (default) | `deep` — [audit-playbook](audit-playbook.md) ## Effort tiers.
2. Resolve host skills install root during recon (`/tacos-doctor diagnose` — prefer root where `tacos-orchestration` lives; [host-layout](../../tacos-host-skill/references/host-layout.md)). When copying [audit-explore-dispatch-prompt](audit-explore-dispatch-prompt.md), replace every `{{SKILLS_PREFIX}}` with that root **before** sending — children must receive concrete paths, not placeholders.
3. For each category cluster, parent copies the filled dispatch prompt with recon facts inlined.
4. Launch parallel `agent-tacos-audit-explore` or host `explore` with `readonly: true` when Task supported — cap per tier.
5. Sequential fallback when concurrent Task unavailable — note in summary.

## Vet (parent)

1. Merge child outputs; deduplicate.
2. When tier is **`deep`** and Task spawn is supported, parent **MAY** launch a lightweight read-only scout child to spot-check category scout `file:line` citations before presenting findings — parent retains vet authority; scout MUST NOT auto-promote candidates to findings.
3. Open every cited `file:line` — correct, downgrade, or reject (parent always owns this step; scout notes merge into parent judgment when scout ran).
4. Record rejections in `index.md` ## Findings considered and rejected.
5. Present findings table (leverage order) + separate direction table.
6. **STOP** — require user to pick finding ids before plan.

## Plan

1. User provides finding ids: `/tacos-audit plan <slug> 1,3`.
2. Compose plan(s) per [plan-template](plan-template.md) — single `audit-plan.md` unless 3+ unrelated findings → `plans/NNN-*.md`.
3. Preview gate per [preview-gate](preview-gate.md).
4. On Proceed: write files; stamp `planned_at_sha`.

## Review-plan

1. User provides plan path: `/tacos-audit review-plan artifacts/tacos-audit/<slug>/audit-plan.md`.
2. Critique per [review-plan-runbook](review-plan-runbook.md) — no new findings.
3. Preview gate per [preview-gate](preview-gate.md).
4. On Proceed: overwrite plan file only.

## Reconcile

1. User invokes `/tacos-audit reconcile <slug>` for an existing session under `artifacts/tacos-audit/<slug>/`.
2. Read `index.md` (if present), `findings.md`, `audit-plan.md`, and any `plans/*.md`.
3. Run `git rev-parse --short HEAD` — compare each plan frontmatter `planned_at_sha` to current HEAD; list drifted plan paths in chat.
4. For drifted plans, run `git diff --stat <planned_at_sha>..HEAD -- <paths from plan ## Scope>` and summarize whether re-plan or `review-plan` is advised — do not auto-rewrite plans.
5. Update `index.md` **Status** (e.g. `findings`, `planned`, `execute-pending`, `execute-complete`) and optional `reconciled_at_sha` / `reconciled_at` fields; record drifted plan paths under **## Plan drift** when any differ.
6. Writes only under `artifacts/tacos-audit/<slug>/` — no source mutation.

## Execute / handoff

See [execute-runbook](execute-runbook.md) and [handoff-routing](handoff-routing.md).
