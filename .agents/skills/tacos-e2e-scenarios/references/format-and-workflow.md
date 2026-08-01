# E2E scenarios: format and workflow

Use with [../SKILL.md](../SKILL.md) (workflow, testability, post-run summary). Orchestration timing: [post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) ### E2E closure. Journey-level synthesis rules below are canonical — SKILL links here one hop.

## Audience and readability

For QA, test authors, and reviewers — quick grasp during planning and review without reading full specs.

- **Sections:** **Scope** → **Scenarios** → **Traceability** → **Notes for automation**
- **Scenarios:** `### Scenario: [short outcome-oriented title]` — not spec IDs. One blank line between scenarios.
- **Lists over prose:** bullets or numbered steps; no wall-of-text.
- **Metadata first:** `Automation:` and optional `Priority:` at the top of each scenario.
- **Outcome first:** state what the user should see; optional sub-bullets for regressions.
- **Traceability:** verbatim `### Requirement:` titles; keep table rows scannable.

## Level of detail

Journey-level flows — **not** spec `#### Scenario:` granularity. **Synthesize many-to-few:** map coverage in **Traceability**, do not mirror every spec scenario.

Produce **2–8** end-to-end user journeys (happy path, key error path). High-level user/QA language only — suitable for manual execution and for deriving automated tests.

**Anti-patterns:**

- One e2e scenario per spec `#### Scenario:` line
- One scenario per field, screen, or control
- Duplicate flows differing only by tiny step tweaks

## Scenario body shape (high level first)

**Default for simple flows** — one line:

```markdown
**Verify that** [primary user-visible outcome].
```

**Multi-step journeys** (multi-screen, wizard-like) — use three labeled blocks with lists:

- **Setup** — role, data, or context (optional; omit if obvious)
- **Steps** — numbered user-visible actions
- **Outcome** — what the user should see (lead with the main result)

Use **Verify that** when a single outcome line is enough. Use **Setup / Steps / Outcome** only when there is more than one meaningful step. No implementation detail (selectors, endpoints, internal APIs) in scenario bodies.

## Automation intent

Each scenario: **Automated** or **Manual**.

- Typical: **1–2 Automated** scenarios per change; rest Manual
- **Automated:** stable, repeatable, deterministic paths worth CI cost
- **Manual:** exploratory or fragile flows; may still be long journeys

Optional line when **Automated:** `Maps to one automated test: yes | no` — one journey should map to one CI test, not one click.

The consuming project documents runner, mocks, and file patterns in **Notes for automation**. The core skill does not prescribe test frameworks, runners, or repository-specific paths.

## Required output format

```markdown
# E2E Scenarios: [change name]

## Scope

[Brief in-scope / out-of-scope for e2e testing.]

## Scenarios

### Scenario 1: [Short outcome-oriented name]

- Priority: High | Medium | Low _(optional)_
- Automation: Automated | Manual
- Maps to one automated test: yes | no _(Automated only)_

**Verify that** [user can complete X and sees Y.]

### Scenario 2: [Multi-step journey]

- Automation: Automated
- Maps to one automated test: yes

- **Setup**
  - …
- **Steps**
  1. …
  2. …
- **Outcome**
  - …

## Traceability

|Scenario|Covers _(verbatim `### Requirement:` titles from delta specs)_|Automation|
|-|-|-|
|1|…|Automated|
|2|…|Automated|

## Notes for automation

[Runner, mocks, journey test layout — per the consuming project's test documentation.]
```
