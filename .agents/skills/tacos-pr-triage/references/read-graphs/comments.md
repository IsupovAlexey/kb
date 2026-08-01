# Pr-triage — comments track

Load after checks branch completes or skips.

## MUST read

- [triage-rubric.md](../triage-rubric.md)
- [gate-budget.md](../gate-budget.md) ## Gated path — comment track
- [approval-gates.md](../approval-gates.md#post-triage-routing) · [#fix-picklist](../approval-gates.md#fix-picklist) · [#publish-gate](../approval-gates.md#publish-gate) · [#resolve-picklist-one-gate](../approval-gates.md#resolve-picklist-one-gate) · [#runtime-structured-ask](../approval-gates.md#runtime-structured-ask)
- [commit-and-push.md](../commit-and-push.md#publish-gate-one-prompt--includes-fix-review) · [#reply-commit-link](../commit-and-push.md#reply-commit-link)
- [resolve-threads.md](../resolve-threads.md)
- [suppressed-review-comments.md](../suppressed-review-comments.md)
- [dismissal-catalog.md](../dismissal-catalog.md) — consult after sync before triage; required on loop wake and bot-settle polls
- [automation-detection.md](../automation-detection.md)

## During phase

Before triage report or bypass sweep:

1. Confirm [suppressed-review-comments](../suppressed-review-comments.md) § Agent checklist (mandatory) completed — suppressed parse summary line in chat **and** `comments/suppressed-*.md` upserts when markers were present.
2. Triage every open record per [triage-rubric](../triage-rubric.md) § Automation skepticism (default).

## Gates (gated path)

|Gate|When|Prompt count|
|-|-|-|
|Post-triage routing|Automation-only open items|1|
|Fix picklist|Pending fix items|1 multi-select|
|Publish|After local comment fixes|1|
|Resolve|Before `gh` reply/resolve|1 multi-select — executes `gh` same turn|

Bypass path: [bypass-mode.md](../bypass-mode.md) sweep after assess; human comments keep gated picklists.

## Done when

- Comment track gates complete or bypass sweep done; loop continuation when `loop_mode`
