# Check matrix builder

Dedupe related failing checks into **root + collapsed dependents** before the check fix picklist ([check-triage-rubric](check-triage-rubric.md)).

## Roles

|`matrix_role`|Meaning|
|-|-|
|`standalone`|No shared upstream with other failures; own report line and picklist eligibility|
|`root`|Upstream failure representing a cluster; picklist eligible when `failure_kind: code`|
|`dependent`|Downstream failure caused by root; collapsed in report; **not** on picklist|

Persist `root_check_slug` on dependent records (slug of the root check file).

## Grouping heuristics

Evaluate failing checks together after sync. Prefer the **earliest failing job in the same workflow run** as root when logs show cascade failures.

Group dependents under a root when any of:

- Same **workflow name** and shared workflow run id (from check `link` or Checks API `check_suite`)
- Same **check suite id** with multiple job conclusions
- Dependent check name suffix matches root (e.g. `build (linux)` under `build`)
- Annotation or log text references parent job failure ("cancelled because a dependency failed")

When multiple candidates tie for root:

- Prefer the job with the **first failure timestamp** in the workflow run
- Prefer the job whose log contains the **original error** (compile/test) over generic "skipped" messages

## Report output

For each cluster:

1. One **root** or **standalone** line in the Check failures section ([check-triage-rubric](check-triage-rubric.md) format).
2. Append `(N dependent(s) collapsed)` when N > 0.
3. Optional one-line note listing dependent **names** only when N ≤ 3; otherwise count only.

Do not list dependents as separate picklist rows.

## Root confirm (needs-context)

When root confidence is low (tie-break ambiguous, sparse logs, conflicting timestamps):

1. Set root record `validity: needs-context`.
2. Show proposed root slug, name, and why dependents group there.
3. Call **one** structured `AskQuestion` ([gate-budget](gate-budget.md)):

**Prompt:** `Matrix root is uncertain — confirm upstream failure for this cluster?`

|id|label|
|-|-|
|`confirm-root`|Yes — use `<root-check-slug>` as root|
|`split-standalone`|No — treat each failure as standalone|
|`defer-cluster`|Defer this cluster — no fix picklist this pass|

On `confirm-root`: set root `validity: valid` (or keep needs-context if logs still truncated); keep grouping.

On `split-standalone`: set all checks in cluster to `matrix_role: standalone`; clear `root_check_slug`.

On `defer-cluster`: set cluster records `action: defer`, `fix_state: skipped`; exclude from picklist.

**Gate budget:** This confirm counts as **1 AskQuestion** only when shown — not when heuristics confidently pick a root.

## After matrix

Proceed to check fix picklist for `failure_kind: code` roots and standalones only ([approval-gates](approval-gates.md) § Check fix picklist). Infra/unknown clusters are report-only.
