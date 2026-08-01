# Pr-triage — bypass mode

When invoke includes `bypass` token or `_session.md` has `bypass_mode: true`. Combines with [loop.md](loop.md) when both tokens present.

## MUST read

- [bypass-mode.md](../bypass-mode.md) — triage before sweep; triage-gated automation autofix; refusal replies; report-only infra/unknown
- [dismissal-catalog.md](../dismissal-catalog.md) — consult after sync; upsert on refuse; cross-id reuse on loop wake
- [single.md](single.md) — preflight + sync
- [gate-budget.md](../gate-budget.md) — bypass invoke approves full sweep; zero prompts except matrix root confirm when `needs-context`

## During bypass

- Comment triage MUST complete and persist before automation autofix
- Code check autofix in bypass sweep; infra/unknown report-only
- Automation: local edit only `valid` + `fix-in-code`; refused items get refusal reply + resolve
- Human comments keep gated picklists per [comments.md](comments.md)
- Matrix root confirm: **0–1** when `needs-context`

## Done when

- Bypass sweep completed or user stopped; artifacts under `{descriptions_root}/<branch-slug>/pr-triage/`
