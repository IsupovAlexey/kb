# Pipeline trace and decision verification (apply-review)

Use with `tacos-apply-review` `SKILL.md` workflow step 3. Orchestrator gate pass: [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md).

## When this section applies

Run when **any** planning input documents:

- Ordered **pipeline** or call sequence (validate → admit → read → mutate → publish)
- **Gate before I/O** or reject-before-external-call
- **Verify Decision N** rows under tacos-work **## Work** or full tacos `tasks.md`
- Named **negative/error paths** that must not consume capacity or must fail before side effects

When none apply, write `## Pipeline trace` as **N/A** in the review artifact.

## Pipeline trace (reviewer procedure)

1. Identify the **entrypoint** the diff wires or modifies (service method, handler, middleware).
2. List **ordered steps** in the implementation hot path — include **loop bodies** and indirect reads (helper calls, per-item fetches), not only top-level statements.
3. Compare order to planning: `design.md` **Decision N**, grill **User inputs**, tacos-work **Planning → Decisions**, stage `tasks.md` acceptance, and **Verify Decision N** rows.
4. **BLOCKER** when any dependency **read or write** (external API, DB, cache, message bus) runs **before** the documented gate/admission point — including reads inside loops started before the gate.
5. Record findings in review artifact `## Pipeline trace`:

```markdown
## Pipeline trace

- Entrypoint: `Class.Method`
- Planned order: validate → admit → read → mutate → publish
- Actual order: …
- Result: pass | fail — <one line>
```

When fail, add **Must address** row: `BLOCKER | <location> | Move <gate> before <first violating I/O> per Decision N / Verify row`.

## Decision verification (reviewer procedure)

For each open **Verify Decision N** row (or stage task citing **Decision N** with done-when):

|Verify done-when type|Reviewer checks|
|-|-|
|Named test|Test exists; asserts the decision; would fail if implementation regressed|
|Trace step|Hot-path trace confirms order/behavior|
|Command|Evidence in diff or host norms that command was run|

**BLOCKER** when a **Verify** row names a negative/error path or behavioral obligation and no matching automated test or cited existing coverage exists.

**MAJOR** when done-when is vague (“add tests”, “verify behavior”) without a named test or trace criterion — recommend concrete rewrite in review; spec-review should have caught at planning.

## Work binding

For **tacos-work**, treat **Verify Decision N** rows under **## Work** as **primary acceptance scope** alongside touched specs and implementation checkboxes — parallel to staged `tasks.md` for full tacos.

## Summary alignment

Pipeline or verification **BLOCKER**s require `Status: NEEDS REVISION` (or `APPROVE WITH CHANGES` when only **MAJOR** verification gaps remain) with matching **Must address** rows — [review-format.md](review-format.md) ## Summary ↔ severity alignment.
