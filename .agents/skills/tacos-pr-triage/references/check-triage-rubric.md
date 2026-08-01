# Check triage rubric

Advisory labels for failing PR checks. **Not** apply-review or spec-review gate severities. Classify **before** the check fix picklist ([approval-gates](approval-gates.md) § Check fix picklist).

## failure_kind

|Value|Meaning|Fix picklist|
|-|-|-|
|`code`|Actionable test, lint, build, compile, or Sonar quality-gate failure in available logs|Eligible when `matrix_role` is `root` or `standalone`|
|`infra`|Runner, timeout, rate limit, billing/quota, org policy, external service unavailable, cancelled without test output|**Never** — report only|
|`unknown`|Low classification confidence; treat like infra for gates|**Never** — report only|

### Infra signals (non-exhaustive)

- Runner lost / job cancelled by infrastructure
- Job or workflow **timeout**
- GitHub **rate limit** or API throttle messages
- Billing, quota, or org policy blocks
- External service unavailable (registry, artifact store, Sonar unreachable)
- `ACTION_REQUIRED` without actionable test output in log tail

When `failure_kind: infra`, include advisory **wait/retry** hints. Example (no gate):

```text
Advisory: re-run workflow when infra clears — `gh run rerun <run_id> --failed` or re-push when appropriate.
```

Do not offer local code fix for infra or unknown.

### Code signals (non-exhaustive)

- Test assertion or stack trace pointing at project code
- Lint, formatter, or analyzer failure with rule id and path
- Compile/build error with file and line
- Sonar quality gate failed with issue list ([sonarqube-triage](sonarqube-triage.md))

Route Sonar-named checks to [sonarqube-triage](sonarqube-triage.md) for parsing hints before final `failure_kind`.

## Validity

Same semantics as [triage-rubric](triage-rubric.md):

|Value|Meaning|
|-|-|
|`valid`|Actionable failure; fix or wait/retry hint is meaningful|
|`needs-context`|Log truncated, matrix root uncertain, or log too sparse to classify|
|`invalid-or-nit`|Rare for checks — flaky duplicate or already resolved upstream|

When `log_truncated: true` on the check record → set `validity: needs-context` and **do not** set primary action to `fix-in-code` with high confidence — **unless** SonarQube Web API enrichment succeeded (`sonar_enriched: true`) with enumerated issues in `## Sonar enrichment` ([sonarqube-enrichment](sonarqube-enrichment.md)). In that case, classify from API issues: `failure_kind: code`, `validity: valid`, and `action: fix-in-code` when issues warrant a local fix.

## Severity

Reuse comment severity ([triage-rubric](triage-rubric.md)):

|Value|Typical use|
|-|-|
|`P0`|Merge-blocking correctness, security, or broken build/test|
|`P1`|Should fix; meaningful quality or maintainability|
|`P2`|Nice-to-have; low risk if deferred|

## Action hint (checks)

|Value|Meaning|
|-|-|
|`fix-in-code`|Local code or doc change expected (`failure_kind: code` only)|
|`wait-retry`|Infra or transient; re-run workflow or wait|
|`needs-human`|Policy, permissions, or external system — author decides|
|`defer`|Track locally; not in this pass|

Infra and unknown failures MUST use `wait-retry`, `needs-human`, or `defer` — never `fix-in-code` on the picklist.

## Report format

Emit a **Check failures** section **before** the comment triage report when any failing checks exist after sync.

Group by matrix cluster (root first) or standalone. Each root or standalone line:

```text
- [<slug>] failure_kind=<value> matrix_role=<value> validity=<value> severity=<value> action=<hint> — <one-line summary> (<n> dependent(s) collapsed when root)
```

For `matrix_role: dependent`, do **not** emit separate report lines — count collapsed under the root summary.

After steps 1–4 below, run matrix root confirm at step 5 when needed ([check-matrix](check-matrix.md)), then step 6 check fix picklist or skip to comment triage per [gate-budget](gate-budget.md).

## Classify-before-picklist

1. Assess every persisted failing check (`checks/*.md` with `status` not `resolved`).
2. Apply [check-matrix](check-matrix.md) grouping; update `matrix_role` and `root_check_slug` on records.
3. Set `failure_kind`, `validity`, `severity`, `action` on each assessed record.
4. Emit the **Check failures** report section (format above).
5. Root confirm (0–1 AskQuestion) when matrix root is `needs-context`.
6. Check fix picklist: **roots + standalone** with `failure_kind: code`, `action: fix-in-code`, and `fix_state: pending` only.

## Forbidden output

Same as comment triage: no BLOCKER, CRITICAL, MAJOR, MINOR, APPROVE, NEEDS REVISION, or spec-compliance findings from bound planning artifacts.
