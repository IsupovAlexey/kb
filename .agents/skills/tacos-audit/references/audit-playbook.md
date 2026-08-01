# Audit playbook

Nine categories. Each finding: id, title, category, effort (S/M/L), confidence (HIGH/MEDIUM/LOW), `file:line` evidence, impact, recommended `handoff`.

## Categories

1. **Correctness / bugs** — error handling, async, nulls, boundaries, concurrency, resource leaks
2. **Security** — credentials, injection, access control, dependencies, config, data minimization
3. **Performance** — N+1, complexity, caching, payload/bundle size, CI slowness
4. **Test coverage** — dangerous untested paths, churn without tests, missing verification infra
5. **Tech debt / architecture** — duplication, layering, dead code, inconsistent patterns
6. **Dependencies / migrations** — version lag, deprecated APIs, duplicate deps
7. **DX / tooling** — lint gaps, slow feedback, missing AGENTS.md, onboarding friction
8. **Docs** — only when absence has concrete cost or docs are actively wrong
9. **Direction** — feature/roadmap suggestions; **every item must cite repo evidence** — separate table from bugs

## Effort tiers

|Tier|Explore children|Categories|Findings presentation|
|-|-|-|-|
|`quick`|0–1|correctness, security, tests|Top ~6 HIGH only; hotspots|
|`standard`|≤4|all nine|Full vetted table|
|`deep`|≤8|all nine|Full table + LOW “investigate” items|

`branch` mode: scope to merge-base diff; tag each finding `introduced` or `pre-existing`.

`direction` mode: category 9 only; 4–6 grounded suggestions.

## Prioritization

Order findings by leverage: impact ÷ effort × confidence. Direction table is not ranked against bugs.

## Vetting

Subagents over-report. Parent MUST re-read every citation before presentation. Stale ADR drift is a finding; ADR-documented tradeoffs are rejections, not bugs.

## Dedrift

When recon detects main-spec behavioral drift, emit finding with `handoff: dedrift` and capability names — recommend `/tacos-dedrift reconcile` or `conform`; never auto-run.
