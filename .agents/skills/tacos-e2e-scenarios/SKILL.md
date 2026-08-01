---
name: tacos-e2e-scenarios
description: >-
  Writes openspec/changes/<name>/e2e-scenarios.md (2–8 journey scenarios) from
  planning artifacts. Use when POST-ARTIFACT e2e runs, user invokes
  /tacos-e2e-scenarios, or they ask for e2e closure.
user-invocable: true
---

# tacos e2e scenarios

Produces **`openspec/changes/<name>/e2e-scenarios.md`**. Format and journey rules: [references/format-and-workflow.md](references/format-and-workflow.md).

Delegation: Run via Task as `agent-tacos-e2e-scenarios` when supported — [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md).

## Quick start

|||
|-|-|
|Inputs|`openspec/changes/<name>/` — `proposal.md`, `specs/**`, `design.md`|
|Output|`openspec/changes/<name>/e2e-scenarios.md`|
|Sample summary|`E2E scenarios: Created — openspec/changes/foo/e2e-scenarios.md — 4 scenarios (1 Automated, 3 Manual)`|

## Workflow

1. Resolve change — Active change from parent prompt, user message, or `openspec/changes/<name>/` context. If ambiguous, ask once.
2. Check existing file — If `e2e-scenarios.md` exists → Skipped (exists); do not replace. Offer append or user-directed edits only after explicit approval.
3. Testability — Apply [Testability](#testability). If not end-to-end testable → Omitted with a one-line reason; do not create the file.
4. Synthesize — Read planning artifacts; apply [Level of detail](references/format-and-workflow.md#level-of-detail).
5. Write — Create `e2e-scenarios.md` using the template in [format-and-workflow.md](references/format-and-workflow.md).
6. Summary — Emit the [post-run summary](#post-run-summary) in the human-visible turn output (parent orchestrator or slash invoke).

Toggle behavior: [post-artifact-planning-review.md](../tacos-orchestration/references/post-artifact-planning-review.md) ### E2E closure; master switch `orchestration.enabled`.

## Testability

Create when the change introduces or changes behavior a user, operator, or QA can exercise as a coherent journey (UI flow, CLI workflow, API sequence with observable outcome, integration path).

Omit (not end-to-end testable) — do not create the file; state which bucket applies:

|Bucket|Examples|
|-|-|
|Docs / prose only|Documentation and comments only; no executable behavior change|
|Internal refactor|Rename, extract module, test-only moves; no user-visible delta|
|Developer tooling|CI, build, or packaging tooling; no end-user journey|
|Config / flags only|Toggle defaults with no new journey to verify|

When borderline, prefer Create with a narrow Scope section that states what is out of scope for e2e.

Never overwrite — existing `e2e-scenarios.md` is immutable unless the human explicitly requests append or replacement.

## Post-run summary

Always include exactly one status line and bullets as applicable:

```text
E2E scenarios: Created | Omitted | Skipped (exists)
```

|Status|Required bullets|
|-|-|
|Created|Path: `openspec/changes/<name>/e2e-scenarios.md` · Counts: `N scenarios (A Automated, M Manual)`|
|Omitted|Reason: one line (name the testability bucket or specific rationale)|
|Skipped (exists)|Path to existing file · Note: not modified|

Optional: `Inline execution` when Task/subagent delegation was unavailable (per [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md)).

## Done when

- Post-run summary emitted with exactly one status line and required bullets per [Post-run summary](#post-run-summary).
- Created: `e2e-scenarios.md` matches the template and [Level of detail](references/format-and-workflow.md#level-of-detail) in [format-and-workflow.md](references/format-and-workflow.md).
- Omitted: file not created; reason names a testability bucket or specific rationale per [Testability](#testability).
- Skipped (exists): existing file left unchanged unless the human explicitly approved append or replacement.

## When to run

- After planning — when `orchestration.enabled` and `orchestration.e2e_enabled` are true (toggle and timing in [Workflow](#workflow)).
- On request — `/tacos-e2e-scenarios` or user asks for end-to-end scenario closure.
- Schema — artifact id `e2e-scenarios` → `e2e-scenarios.md`; optional; not in `apply.requires`.

## References

- `tacos-orchestration` — POST-ARTIFACT E2E closure toggle (link in [Workflow](#workflow))
- `tacos-spec-review` — may review `e2e-scenarios.md` when present
- `tacos-test-plans` — QA acceptance packs at `openspec/test-plans/<slug>/`; this skill MUST NOT write there
