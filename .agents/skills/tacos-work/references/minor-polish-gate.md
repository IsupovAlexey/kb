# MINOR polish gate (tacos-work Phase 4)

Procedural step **4.5** after apply-review gate pass. When latest apply-review satisfies [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) (`APPROVE` + `Ready`) but **Optional (MINOR)** lists open rows, the orchestrator runs this gate **before** checking off **Apply review:** and opening **Human review:**. Staged apply mirrors this binding via [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Mechanical MINOR sweep.

## When to run

|Condition|Action|
|-|-|
|Zero open MINOR rows|Skip gate; proceed to check off **Apply review:**|
|Open MINOR rows|Run auto-fix pass, then human gate when complicated rows remain|

**Forbidden:** reporting "ready for Human review" while open MINOR rows are neither addressed, auto-fixed, nor waived.

## Simple auto-fix (conservative)

Auto-fix without human gate when **all** of the following hold for a MINOR row:

- Single file path cited in the row
- ≤3 net lines changed in that file
- Edit type is one of: cross-link to canonical reference, one-line carve-out, explicit `MUST NOT` / `SHALL NOT` line, or missing done-when citation line when behavior is already specified elsewhere
- No product decision, refactor, or multi-file coordination required

When confident, apply the edit, re-run host implementation gates when non-empty per [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) **Re-runs checks**, and record under **Polish outcome**.

When uncertain whether a row qualifies as simple, treat as **complicated** — do not auto-fix.

## Complicated remainder — structured multi-select

When any MINOR row remains after the auto-fix pass (or was classified complicated from the start):

1. Show a short chat summary: which rows were auto-fixed (if any) and which remain.
2. **Next tool call MUST** be structured multi-select per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) — **forbidden:** prose-only "which MINORs should I fix?"
3. One option per remaining MINOR row (`id` = stable slug from file + theme, `label` = `path — summary`).
4. `allow_multiple: true` — user selections = fix now in this session.
5. Unselected options = **waived** for this session; record each waived row in turn summary and **Polish outcome**.

On zero selections: all remaining rows waived unless the user explicitly asks to fix specific rows in a follow-up message (re-open gate).

Plain-text fallback when structured tools absent: list MINOR rows; user replies with row ids to fix; unmentioned rows waived; **end turn**.

## Polish outcome

After auto-fix and/or human-selected fixes, append to latest `artifacts/openspec-reviews/<slug>/apply-review.md`:

```markdown
## Polish outcome (parent)

- <path or row id> — auto-fixed
- <path or row id> — fixed (human selected)
- <path or row id> — waived
```

When appending to the artifact is impractical, equivalent bullets in the turn summary are acceptable — cite apply-review path.

No `apply-review-r2.md` for polish-only edits.

## Phase binding

Complete minor polish (or skip when zero MINOR) before checking off **Apply review:**. No separate **## Work** checkbox — this gate is part of Phase 4 procedure.
