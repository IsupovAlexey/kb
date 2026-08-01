# Automated plan review

Rubric pass on the **draft** test plan after synthesis and **before** the human [preview gate](preview-gate.md). Parent owns merge and preview gate — review subagent does not write pack files.

## When to run

After synthesis (inline or `agent-tacos-test-plans`) and before presenting the preview summary.

|Runtime|Behavior|
|-|-|
|Task + `agent-tacos-test-plan-review` installed|MUST delegate plan review to subagent; parent merges findings in chat|
|Task unavailable|Parent MAY run inline per rubric below; note `Inline plan review` in post-run summary|

When `agent-tacos-test-plan-review` is not installed, inline review satisfies this reference.

## Parent merge

After subagent returns (or inline pass completes), parent MUST show in chat:

|Section|Content|
|-|-|
|Review status|Pass · Pass with MAJOR · Blocked|
|Must address|BLOCKER and CRITICAL rows|
|Should address|MAJOR rows (fix before preview when practical)|
|Deferred|MINOR or waived items|

Open **BLOCKER** or **CRITICAL** findings MUST be fixed or explicitly waived in chat before opening the preview Proceed gate.

## Severity guide

|Level|Meaning|Preview gate|
|-|-|-|
|BLOCKER|Missing requirement coverage, format violation, silent overwrite risk|STOP — fix or waive|
|CRITICAL|Traceability gap, wrong TC id pattern, confirmed behavior asserted for placeholder|STOP — fix or waive|
|MAJOR|Weak expected results, missing diagram for multi-step domain, incomplete Summary|Fix when practical; note in preview|
|MINOR|Style, optional companion omission|Defer; note in preview|

## Review dimensions

### Requirement traceability

- Every scanned `### Requirement:` with testable behavior has a matching `## Requirement:` section.
- Each requirement has at least one `TC-*` case unless explicitly marked out-of-scope in scope grill **User inputs**.
- Verbatim requirement titles — no paraphrase in section headings.

### Format compliance

Per [test-case-format.md](test-case-format.md):

- Metadata header complete (**Source**, **Spec**, **Pulled into repo**, **Updated**).
- Four-column scenario tables on every case.
- `TC-{AREA}-{NNN}` ids unique within pack; AREA follows slug-derived rule or scope grill decision.
- `## Summary` and `### Notes for QA implementation` present.

### Placeholder policy

- Unconfirmed design uses placeholder rows or headings — no asserted final behavior.
- **Known inconsistency** callouts when sources conflict.
- Placeholder count matches preview summary.

### Diagram rule

- Multi-step / wizard / multi-tab scope includes diagram in intro or `diagrams.md`.
- Trivial single-case plans: flag MAJOR only if diagram was required by scope grill and missing.

### Eng handoff

- Read-only boundary not violated in draft (no eng-path edits suggested inline).
- `journeys.md` present when scope grill or synthesis noted TDD handoff need.

### Scope grill alignment

- Draft honors **Decisions** and **User inputs** from `grill-summaries.md` when present.
- New cases outside grilled coverage flagged CRITICAL unless user expanded scope in chat.

## Subagent return contract

When `agent-tacos-test-plan-review` runs, it returns:

- Status line: `Plan review: Pass | Pass with MAJOR | Blocked`
- Findings table: `Severity | Location | Finding | Remediation`
- Counts: requirements covered / scanned; placeholder count; open BLOCKER/CRITICAL

Parent does not treat subagent Pass as preview approval — human preview gate still required.

## Re-review

When parent fixes BLOCKER/CRITICAL findings, re-run plan review (fresh subagent or inline) before preview. Note `Plan review: Re-run` in chat.

## Forbidden

- Opening preview Proceed while BLOCKER/CRITICAL remain open without explicit user waiver.
- Writing pack files during plan review.
- Treating plan review Pass as substitute for preview gate or replace approval.
