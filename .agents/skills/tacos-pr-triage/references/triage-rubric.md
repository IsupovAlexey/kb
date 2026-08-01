# Triage rubric

Advisory labels for the author. **Not** apply-review or spec-review gate severities.

## Validity

|Value|Meaning|
|-|-|
|`valid`|Actionable feedback; address or reply|
|`needs-context`|Unclear without code read or thread history|
|`invalid-or-nit`|Likely noise, style-only, or incorrect (common for bots)|

## Severity

|Value|Typical use|
|-|-|
|`P0`|Must fix before merge (correctness, security, broken behavior)|
|`P1`|Should fix; meaningful quality or maintainability|
|`P2`|Nice-to-have; low risk if deferred|

In normal mode, P0 items on the fix picklist should be addressed before resolve unless the user explicitly defers. Automation-only PRs: [approval-gates](approval-gates.md) § Automation routing offers gated fix picklist vs bypass sweep.

## Action hint

|Value|Meaning|
|-|-|
|`fix-in-code`|Local code or doc change expected|
|`reply-only`|Discussion/clarification; no code change|
|`resolve-without-code`|Already fixed or not applicable; resolve on GitHub|
|`defer`|Track locally; not in this pass|

## Report format

Group by file (inline) or category (issue/review). Each line:

```text
- [<id>] validity=<value> severity=<value> action=<hint> — <one-line summary> (`path:line` when inline)
```

Flag `is_outdated: true` threads in the line text — do not auto-dismiss.

After the report, route via [approval-gates](approval-gates.md) § Post-triage routing — **must** call structured `AskQuestion` before fixes or GitHub writes.

## Forbidden output

Do not emit: BLOCKER, CRITICAL, MAJOR, MINOR, APPROVE, NEEDS REVISION, or spec-compliance findings from bound planning artifacts. Bound change context may inform hints only ([change-binding](change-binding.md)).

## Automation skepticism (default)

Automation reviewers (Copilot, Bugbot, Claude review apps, etc.) MAY be wrong, nit-picky, or lack PR and repo context. `author_kind: automation` is **not** implicit `validity: valid`.

Before assigning `valid` + `fix-in-code` to any automation item:

1. Read the referenced path and line (or thread context when path is missing).
2. Read enough surrounding change context (diff hunk, requirement, or call site) to verify the suggestion applies to **this** PR.
3. Confirm the issue is not already fixed, out of scope, or a style preference the change intentionally rejects.

When code has not been read: default `needs-context` — not `fix-in-code`. Bypass waives picklists, not this bar ([bypass-mode](bypass-mode.md) § Triage discipline).

|Signal|Default lean|
|-|-|
|`source: review_suppressed` or `is_minimized: true`|`needs-context` or `invalid-or-nit` unless step 1–3 clearly support fix|
|Thread on same file, different body|Triage each record independently — do not copy disposition|
|Catalog match `(catalog match)`|Execute stored disposition; skip re-read when rationale still applies|

MUST triage every `review_suppressed` record before bypass sweep. Normal mode: user picklists for human comments. Bypass: execute persisted triage — local autofix only for `valid` + `fix-in-code` after the read bar above; refuse incorrect or nitty items per [bypass-mode](bypass-mode.md).

## Automation vs human (classification)

Set `author_kind` at sync per [automation-detection](automation-detection.md) (GitHub `Bot` type, login patterns, built-in catalog, optional `automation-authors.yaml` for in-house reviewers). Label automation comments `invalid-or-nit` or low severity when appropriate — bots are often wrong.

## Catalog-matched automation

After sync, [dismissal-catalog](dismissal-catalog.md) consult runs before this report. Catalog-matched open automation records arrive with persisted `validity`, `action`, and `notes` — list them with `(catalog match)` and do not treat as untriaged. Bypass: execute refused catalog disposition without picklists. Normal mode: include in report; user picklists apply only to non-catalog human items and untriaged automation.
