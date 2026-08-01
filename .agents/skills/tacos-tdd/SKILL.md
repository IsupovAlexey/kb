---
name: tacos-tdd
description: >-
  Propose path: invoke via /tacos-tdd to create or mark a change with tdd.md and
  TDD planning. Apply path: orchestration loads this skill when tdd.md exists —
  Red/Green/Refactor per stage before implementation checkboxes.
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Change name (e.g. add-billing-export) — creates or updates the change with TDD marker
---

# tacos tdd

Opt-in TDD for full tacos OpenSpec changes. Marker: `tdd.md` in the change folder only — no global `orchestration.tdd_enabled`.

## Entry

1. Resolve caller path:

|Path|When|
|-|-|
|propose|User invoke (`/tacos-tdd`, natural-language TDD + change name)|
|apply|Orchestration apply when `tdd.md` exists — [tdd-apply-contract.md](../tacos-orchestration/references/tdd-apply-contract.md)|

2. Load path bundle (MUST gates ≤1 hop):

|Path|Bundle|
|-|-|
|propose|[mode-propose.md](references/mode-propose.md)|
|apply|[mode-apply.md](references/mode-apply.md) → [red-green-refactor.md](references/red-green-refactor.md)|

## Quick start

|User says|Path|
|-|-|
|`/tacos-tdd <change-name>`|propose|
|`/tacos-tdd` (no name)|propose — ask once for change name|
|Apply with `tdd.md` present|apply|

Non-TDD changes: no `tdd.md`, standard implement-first apply.

## Done when

- Propose: per [mode-propose.md](references/mode-propose.md).
- Apply: per [mode-apply.md](references/mode-apply.md) and [red-green-refactor.md](references/red-green-refactor.md).

## References

[red-green-refactor.md](references/red-green-refactor.md) · [task-slice-template.md](references/task-slice-template.md) · [tdd-marker-template.md](references/tdd-marker-template.md) · `tacos-orchestration` · `tacos-apply-review` · `tacos-test-plans` (read `openspec/test-plans/<slug>/` for Red input; MUST NOT write test plan files)
