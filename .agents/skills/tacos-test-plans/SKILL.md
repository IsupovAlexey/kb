---
name: tacos-test-plans
description: >-
  QA-led test plan synthesis under openspec/test-plans/<slug>/ from specs and
  user sources. Invoke via /tacos-test-plans only; scope grill, plan review,
  preview gate before write.
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Test plan slug (e.g. scheduling-versions) — optional change name or source paths in prompt
---

# tacos test plans

Produces committed acceptance test case packs under `openspec/test-plans/<slug>/`. Format and workflow: [references/format-and-workflow.md](references/format-and-workflow.md).

Delegation:

- Synthesis — Task `agent-tacos-test-plans` when installed ([runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md)); note `Inline execution` when unavailable.
- Plan review — Task `agent-tacos-test-plan-review` when installed; note `Inline plan review` when unavailable.

## Quick start

|||
|-|-|
|Inputs|User prompt; `openspec/specs/**`; optional `openspec/changes/<name>/`; supplementary spec paths; explicit paths or URLs|
|Output|`openspec/test-plans/<slug>/` — required `<slug>-test-cases.md`; optional `sources.md`, `journeys.md`, `diagrams.md`, `grill-summaries.md`|
|Sample summary|First line `Test plan: Created`; bullets: path `openspec/test-plans/foo/foo-test-cases.md` · counts `12 cases (3 placeholders)` per [Post-run summary](#post-run-summary)|

## Workflow

1. Resolve slug — From `/tacos-test-plans <slug>` or user message. If missing, ask once. Slug is QA-chosen; no active OpenSpec change required.
2. Scan sources — User prompt plus paths per [source-scanning.md](references/source-scanning.md). Host gather skills are inputs only.
3. Branch reminder — QA SHOULD work on a branch before commit; state when the user has not mentioned one.
4. Scope grill — Dedicated test-plan interview per [scope-grill.md](references/scope-grill.md) (`full` / `short` / `defaults` / `skip`). Record `openspec/test-plans/<slug>/grill-summaries.md` on write.
5. Synthesize — Map requirements to `TC-*` cases per [test-case-format.md](references/test-case-format.md). Delegate to `agent-tacos-test-plans` when installed.
6. Plan review — Automated rubric pass per [plan-review.md](references/plan-review.md). Fix or waive BLOCKER/CRITICAL before preview.
7. Preview gate — Full plan summary in chat (include plan-review status); structured Proceed / Edit / Cancel per [preview-gate.md](references/preview-gate.md). On existing slug, require explicit replace approval.
8. Write — After approval only, write pack files. Prefer the doctor bundle at [../tacos-doctor/schemas/tacos/templates/test-plan-pack/](../tacos-doctor/schemas/tacos/templates/test-plan-pack/); gate-synced mirrors live under [references/templates/](references/templates/); hosts edit `openspec/schemas/tacos/templates/test-plan-pack/` after `tacos-doctor` set-schema.
9. Summary — Emit [post-run summary](#post-run-summary) in the human-visible turn output.

Invocation: explicit `/tacos-test-plans` user invoke only (`disable-model-invocation: true`). MUST NOT auto-invoke during orchestration **apply** or POST-ARTIFACT hooks.

Eng boundary: committed packs are read-only for eng `tacos-tdd`, orchestration **apply**, and `tacos-e2e-scenarios`. Requirement drift → QA re-invokes this skill with replace approval — eng MUST NOT edit `openspec/test-plans/**` during implementation.

## Post-run summary

Always include exactly one status line and bullets as applicable:

```text
Test plan: Created | Updated | Skipped (exists) | Cancelled
```

|Status|Required bullets|
|-|-|
|Created|Path: `openspec/test-plans/<slug>/<slug>-test-cases.md` · Counts: `N cases (P placeholders)` · Optional companions listed when written|
|Updated|Path · Counts: adds/updates/removals summary · Replace approval noted when slug existed|
|Skipped (exists)|Path to existing pack · Note: not modified without replace approval|
|Cancelled|Reason: user cancelled at preview gate or declined replace|

Optional bullets when applicable:

- `Scope grill: skipped` — when user chose `skip` at scope grill
- `Inline execution` — synthesis ran inline (no synthesis subagent)
- `Inline plan review` — plan review ran inline (no review subagent)
- `Plan review: Pass` or `Plan review: Blocked (waived)` — final review status before preview

## Done when

- Post-run summary emitted with exactly one status line and required bullets per [Post-run summary](#post-run-summary).
- Scope grill completed or skipped per [scope-grill.md](references/scope-grill.md).
- Plan review pass or waived BLOCKER/CRITICAL per [plan-review.md](references/plan-review.md).
- Created/Updated: `<slug>-test-cases.md` matches [test-case-format.md](references/test-case-format.md); preview gate honored per [preview-gate.md](references/preview-gate.md).
- Skipped (exists): existing pack left unchanged unless the user explicitly approved replace.
- Cancelled: no files written under `openspec/test-plans/<slug>/`.

## Distinction from sibling skills

|Skill|Relationship|
|-|-|
|[tacos-grill](../tacos-grill/SKILL.md)|Scope grill uses interview mechanics; not eng planning grill bundle|
|[tacos-tdd](../tacos-tdd/SKILL.md)|MAY read `openspec/test-plans/<slug>/**` for Red tests; MUST NOT write test plan files|
|[tacos-e2e-scenarios](../tacos-e2e-scenarios/SKILL.md)|Change-scoped journey closure only; MUST NOT write to `openspec/test-plans/`|
|Host gather skills|Inputs only — not part of the tacos test-plans contract|
|[tacos-host-skill](../tacos-host-skill/SKILL.md)|Hosts MAY add domain QA rubrics via review-oriented bootstrap; wire in `openspec/tacos.yaml` review arrays per host-skill refs|

## References

[scope-grill.md](references/scope-grill.md) · [plan-review.md](references/plan-review.md) · [test-case-format.md](references/test-case-format.md) · [format-and-workflow.md](references/format-and-workflow.md) · [source-scanning.md](references/source-scanning.md) · [preview-gate.md](references/preview-gate.md) · [samples.md](references/samples.md) · `tacos-orchestration` · [tacos-apply-review](../tacos-apply-review/SKILL.md)
