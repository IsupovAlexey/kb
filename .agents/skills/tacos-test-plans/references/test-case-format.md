# Test case markdown format

Normative shape for `<slug>-test-cases.md` under `openspec/test-plans/<slug>/`. Templates: [templates/](templates/) and host copy at `openspec/schemas/tacos/templates/test-plan-pack/`.

## Document shell

```markdown
# {Capability title} — Test Cases

**Source:** {scanned paths, URLs, or host gather refs}
**Spec:** {primary spec path(s)}
**Pulled into repo:** {YYYY-MM-DD}
**Updated:** {YYYY-MM-DD} — {brief note when re-run}

{Optional intro: scope, grooming notes, placeholder policy}

---

## Requirement: {verbatim `### Requirement:` title from spec}

### TC-{AREA}-{NNN} — {short title}

|#|Scenario|Setup / preconditions|Expected result|
|-|-|-|-|
|1|{name}|{setup}|{observable outcome}|

---

## Summary

|Requirement section|Case IDs|Placeholders|
|-|-|-|
|…|TC-…|n|

### Notes for QA implementation

1. …
```

## TC id rules

Pattern: `TC-{AREA}-{NNN}`

|Part|Rule|
|-|-|
|`{AREA}`|Uppercase abbreviation, max 5 characters. **Default:** derive from the test-plan slug's primary token (first hyphen segment) — e.g. `scheduling-versions` → `SCHED`, `billing-export` → `BILL`. When one pack spans multiple domains, MAY use distinct AREA codes per requirement section; document in Summary.|
|`{NNN}`|Three digits, zero-padded (`001` … `999`).|
|`000`|Reserved for multi-record count blocks when needed (see below).|

Hosts MAY extend AREA codes per product domain. Non-normative examples: `SCHED`, `CP`, `BU`, `TECH` — see [samples.md](samples.md).

### AREA derivation (default)

1. Take the slug's first hyphen segment (primary token).
2. Uppercase; use up to 5 characters from the start of that token.
3. When the acronym would be fewer than 3 characters, add letters from the next segment; keep the total AREA length at or below 5 characters.

## Requirement sections

- One `## Requirement:` per verbatim `### Requirement:` title from scanned specs.
- Group all `TC-*` cases for that requirement under the section.
- When a requirement has no spec `#### Scenario:` lines, derive cases from requirement body text and user prompt.

## Case headings

```markdown
### TC-{AREA}-{NNN} — {short title}
```

Optional suffix in title: `(Required)`, `(Recommended)`, `(placeholder)`, or `(Known inconsistency)` when sources conflict.

## Scenario table

Four columns — always present:

|#|Scenario|Setup / preconditions|Expected result|
|-|-|-|-|

- Number rows starting at `1` per case.
- **Scenario** — short label for the path under test.
- **Setup** — role, data, navigation context; use `N/A` when obvious.
- **Expected result** — user-visible, testable outcome; no implementation detail (selectors, internal APIs).

## Placeholders and conflicts

When design is unconfirmed or sources conflict:

- Include the case with `(placeholder)` or `(Known inconsistency)` in the heading or scenario row.
- State **Pending design confirmation** or cite conflicting sources in Expected result.
- Include an explicit do-not-automate note in Expected result — placeholders are not ready for automation.
- MUST NOT assert final behavior as confirmed.
- Count placeholders in Summary and preview gate.

## Multi-record block (optional)

When an entity needs count coverage before field-level cases:

```markdown
### TC-{AREA}-000 — Number of {entities}

|#|Scenario|Setup / preconditions|Expected result|
|-|-|-|-|
|1|None exist|…|…|
|2|One exists|…|…|
```

## Diagrams

When the plan covers a wizard, multi-tab workflow, or other multi-step domain, include at least one mermaid or ASCII diagram in the intro or in `diagrams.md`. Trivial single-case plans: diagrams not required.

## Footer

`## Summary` — table of requirement sections, case id lists, placeholder counts.

`### Notes for QA implementation` — numbered list: automation hints, empty-copy rules, entity-level vs field-level, deferred decisions.

## ID assignment

- New cases: next free `{NNN}` for that `{AREA}`.
- Prefer updating in place over renumbering on replace runs.
- On replace approval, document adds/updates/removals in preview and post-run summary.
