# Apply mandatory stage-start grill

Run on the **parent** at each `tasks.md` `## N` stage boundary while `Stage grill:` is unchecked. **STOP** all implementation work in that stage until stage grill finishes or the user chooses **skip**/waive. Open with grill mode per [interview-prompt.md](../interview-prompt.md) **Mandatory stage apply** (includes **skip**). Scope to **this stage only**: stage title, unchecked implementation tasks below, and relevant planning context.

Challenge whether this stage's **planned work** still matches artifacts — not a substitute for [implementation-quality-lenses.md](../implementation-quality-lenses.md) during planning. If reliability, security, scale, edge cases, or observability are missing from specs/design/tasks but this stage needs them, **STOP** and full-resync planning before implementation.

## Interview minimum

After grill mode offer, parent MUST satisfy [interview-prompt.md](../interview-prompt.md) ## Interview minimum (full / short / defaults) before checking off `Stage grill:` or editing implementation files in the stage. Mode offer is step 0 only — **forbidden:** proceeding to implementation after `short` / `full` / `defaults` without at least one user-facing topic prompt (or documented **`skip`** / waive).

Typical focus (pick what applies for topic questions — ask only what the stage needs; MUST NOT pad to a question count):

- Scope and done-ness for the stage outcome
- Assumptions that would be expensive to unwind after implementation
- Conflicts with planning artifacts or prior **User inputs** under `## apply`
- Gaps vs [implementation-quality-lenses.md](../implementation-quality-lenses.md) already captured in planning (if gap → full-resync, do not invent requirements only in chat)
- Call order when wiring a control into an existing entrypoint (record in `grill-summaries.md` ## apply **User inputs** if needed)
- Skip/waiver, ordering with Human review, trigger overlap with mandatory vs triggered apply

Append **User inputs** (and **Decisions** when resolved) under `## apply` in `grill-summaries.md`. Check off the `Stage grill:` line only after interview (or explicit skip/waiver recorded in chat and **User inputs**).

**Forbidden:** Treating grill mode selection as completing stage grill; treating `Stage grill:` as documentation and proceeding to implementation; recording **User inputs** from planning artifacts without structured topic replies; treating stage grill as complete without parent topic interview; skipping triggered apply grill later in the same stage because stage grill already ran.

During **apply** when `orchestration.grill_enabled` is true: use **Triggered** when a signal matches ([triggered-grill.md](../triggered-grill.md)); use this bundle only when the stage grill gate is true ([task-stage-contract.md](../../../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate). No gather step for either.
