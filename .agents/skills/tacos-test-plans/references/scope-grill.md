# Test-plan scope grill

Dedicated interview before synthesis on `/tacos-test-plans`. Uses tacos-grill mechanics ([interview-prompt.md](../../tacos-grill/references/interview-prompt.md)) — **not** the eng OpenSpec planning grill bundle (`proposal` / `specs` / `design` / `tasks` topics).

Parent runs on the invoking agent after [source-scanning.md](source-scanning.md) and before synthesis.

## When to run

|Situation|Grill|
|-|-|
|New slug, unfamiliar domain, terse invoke|Offer `full` or `short`|
|Replace/update with bound scope in prompt or prior `grill-summaries.md`|`short` or `defaults`|
|User already supplied detailed scope contract in the same message|`defaults` or `skip` with recorded rationale|

## Grill mode offer

Call once at scope grill start. Substitute `<slug>` in the prompt.

**Prompt:** `How do you want to grill scope for test plan <slug>?`

|id|label|
|-|-|
|`full`|Full grill — all topics below until exhausted|
|`short`|Short grill — up to ~4 highest-impact topics; MUST ask ≥1 after mode offer|
|`defaults`|Use scan + prompt defaults — MUST ask ≥1 after mode offer; ask further topics only on conflict or missing `answeredBy`|
|`skip`|Skip scope grill — record rationale in **User inputs**|

Per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md): mode offer alone does **not** complete scope grill when user chooses `full`, `short`, or `defaults`.

## Topic script

Pick topics that apply — do not pad when scope is already bound.

|Topic|Ask when|
|-|-|
|Slug and mode|Always unless unambiguous from invoke — create vs replace vs append-only|
|Declared sources|Sources missing, conflicting, or unreachable|
|Coverage in/out|Scope ambiguous — which requirements, capabilities, or flows are in this pack|
|AREA derivation|Slug-derived AREA unclear or multi-domain pack needs distinct AREA codes|
|Placeholder policy|Unconfirmed design or source conflicts — how to mark pending cases|
|Diagram need|Wizard, multi-tab, or multi-step domain per [test-case-format.md](test-case-format.md) § Diagrams|
|Eng handoff|Whether `journeys.md` is needed for TDD kickoff|

## Gather (optional)

When Task is supported, MAY launch `agent-tacos-grill-gather` with this topic script before the mode offer. Parent still owns mode offer and topic interview.

## Record outcomes

Write or update `openspec/test-plans/<slug>/grill-summaries.md` when the pack folder exists or will be created on Proceed. Until first write, record **User inputs** in chat and copy into the pack file on write.

Pack template: [templates/grill-summaries.md](templates/grill-summaries.md)

Minimum sections:

```markdown
---
grill_mode: short
---

# Grill summaries — <slug>

## scope

### Summary

### Decisions

### Open questions

### User inputs
```

**User inputs** MUST trace to user replies from this session's structured or plain-text topic turns — not unilateral summaries of scanned specs alone.

## On `skip`

- Record skip rationale under **User inputs**.
- Proceed to synthesis; note `Scope grill: skipped` in preview summary when relevant.

## Forbidden

- Using eng planning grill topics (architecture, work breakdown, implementation-quality lenses unrelated to test coverage).
- Checking off scope grill without at least one topic turn when user chose `full`, `short`, or `defaults`.
- Inferring **User inputs** from `design.md` or change-folder artifacts without user-facing prompts.
