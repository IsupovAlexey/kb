# Pr-triage — checks track

Load when failing checks exist after sync (skip when all green).

## MUST read

- [check-triage-rubric.md](../check-triage-rubric.md)
- [check-matrix.md](../check-matrix.md)
- [gate-budget.md](../gate-budget.md) ## Checks track
- [approval-gates.md](../approval-gates.md#check-fix-picklist) · [#check-publish](../approval-gates.md#check-publish) · [#runtime-structured-ask](../approval-gates.md#runtime-structured-ask) — C0–C3; not Post-triage routing / Fix picklist / Resolve
- [commit-and-push.md](../commit-and-push.md)

## Gates (gated path)

|Gate|When|Prompt count|
|-|-|-|
|Matrix root|`needs-context`|0–1|
|C0 Check fix|Code failures pending|0–1 multi-select|
|C1 Check publish|After local check fixes|1 when fixes applied|
|C2 Re-fetch|After publish|0 — silent|

Advisory-only (`infra` / `unknown` only): report Check failures; skip C0–C1; proceed to [comments.md](comments.md).

Optional Sonar enrichment: [sonar.md](sonar.md).

## Done when

- Checks track complete or skipped; proceed to comment track or loop continuation
