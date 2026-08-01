# Grill gates

When `orchestration.grill_enabled` is true, tacos enforces grilling beyond OpenSpec file-existence checks.

## 1. Planning grill before `grill-summaries.md`

When `grill.planning` is `pending`:

1. Run **tacos-grill** with phase **`planning`** first (gather → parent interview → summarize).
2. **Then** create or replace `grill-summaries.md` with the summarized result (frontmatter `grill_mode`, `complete` or `skipped`, filled `## proposal` … `## tasks`).

### Interview completion (planning)

`grill.planning` MAY be `complete` only when:

- `grill_mode` is `full`, `short`, `defaults`, `assumptions`, or `skip` (see grill-summaries template), and
- For `full` / `short` / `defaults`: parent ran grill mode offer **and** topic interview per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Interview minimum (full / short / defaults) (mode offer alone is insufficient), and
- For `assumptions`: parent ran grill mode offer **and** assumption interview per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Interview minimum (assumptions) (mode offer alone is insufficient), and
- Each completed phase section has non-empty **User inputs** tracing to user replies (or documents skip), and
- **Forbidden:** `complete` from gather script `answeredBy`, **explore**, or **propose** message alone without step 2.
- **Forbidden:** marking `grill.planning` `complete`, checking off `Stage grill:`, or passing gated approvals when structured tools are unavailable without plain-text interview per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable (or explicit user skip/waiver in chat); using prose-only menus when structured tools **are** available — per [structured-gate-convention.md](structured-gate-convention.md).

### Sequencing forbidden (planning)

- Summarize Task (or inline summarize) **before** parent interview.
- `grill-summaries.md` or `proposal` … `tasks` while `grill.planning` is `pending`.
- Marking `grill.planning` `complete` with empty **User inputs** on all planning phases (unless `grill_mode: skip`).

**Forbidden:** Creating an empty `grill-summaries.md` from the schema template before the planning interview. **Forbidden:** “Prefer reasonable decisions” instead of running planning grill when `grill.planning` is `pending`. OpenSpec may mark `grill-summaries` `done` when the file exists — that does **not** satisfy `grill.planning`.

If `grill-summaries.md` exists but `grill.planning` is still `pending` (e.g. empty scaffold), run planning grill and **fill** the file; do not write `proposal` or later planning artifacts until `grill.planning` is `complete` or `skipped`.

## 2. `grill-summaries` in the status loop

Do **not** skip the planning grill when `grill.planning` is `pending`, even if OpenSpec marks `grill-summaries` `done` because a file exists.

When `grill.planning` is already `complete` or `skipped` and the file is missing, create from [tacos-doctor bundle grill-summaries template](../../tacos-doctor/schemas/tacos/templates/grill-summaries.md) only to recover structure — do not reset phase keys to `pending`.

## 3. Planning grill gates `proposal` … `tasks`

Before writing **any** of `proposal`, `specs`, `design`, `tasks`:

1. `grill.planning` MUST be `complete` or `skipped`.
2. `grill-summaries.md` MUST exist and reflect the planning grill (or explicit skip).
3. Then write planning artifacts in dependency order (host batch loop is fine).
4. Before each artifact write, apply [planning-artifact-loop.md](planning-artifact-loop.md) ### Generation-time contract row for that artifact (`proposal`, `specs`, `tasks`) — Modified Capabilities research, specs main-spec **STOP**, separate Verify Decision rows (FORBIDDEN embed), Apply review **medium** line (not one-line pointer).

## 4. Per-phase grill (`continue` only)

When **continue** adds the next planning artifact, `grill.planning` is already `complete` or `skipped`, and `grill.<phase>` is still `pending`:

1. Run **tacos-grill** for that phase (`proposal`, `specs`, `design`, or `tasks`): gather (Task) → **parent interview** (grill mode + topics for that phase) → summarize (Task).
2. Update `## <phase>` and set `grill.<phase>` to `complete` or `skipped`; non-empty **User inputs** required for `complete` (unless skip).
3. Write **only** that artifact.

**Forbidden (per-phase):** summarize before parent interview; `grill.<phase>: complete` with empty **User inputs**; prior chat or explore as substitute for interview.

## 5. Frontmatter

```yaml
grill_mode: null
grill:
  planning: pending | complete | skipped
  proposal: pending | complete | skipped
  specs: pending | complete | skipped
  design: pending | complete | skipped
  tasks: pending | complete | skipped
  update: pending | complete | skipped
```

**`planning`:** one interview before **propose** or **ff** artifact generation.

**`ff` vs `continue`:** **`ff`** runs planning grill once, then may batch-write `proposal` … `tasks` in one turn. **`continue`** writes one planning artifact at a time and may run per-phase grill (`grill.proposal`, `grill.specs`, …) when those keys are `pending`. **`ff` does not skip** planning grill, POST-ARTIFACT gates, or the rule that explore ≠ planning grill — see [planning-artifact-loop.md](planning-artifact-loop.md) § `ff` never skips.

**Phase keys:** set with planning grill; on continue, tacos-grill may update one phase.

OpenSpec marks `grill-summaries` **done** when the file exists; **`grill.planning`** is the **propose** and **ff** quality gate.

## 6. Explore is not grilling

**`explore`** and `## explore` do not satisfy `grill.planning` or phase completion.

**Forbidden:** Skipping planning grill on **propose** or **ff** while `grill.planning` is `pending`; copying explore into phase sections without **User inputs**; backfilling `grill-summaries.md` frontmatter without an interview.

**Forbidden prefilled scaffold (common skip pattern):** `grill_mode` set and `grill.planning: complete` while planning-phase **User inputs** trace to explore or propose thread text (e.g. `## explore` cites "explore + propose thread"; specs/design/tasks say "Same as proposal"); cross-phase identical **User inputs** without per-phase interview. Recovery: reset `grill.planning` to `pending`, clear synthetic **User inputs**, re-run planning grill. Template anti-patterns: [tacos-doctor bundle grill-summaries template](../../tacos-doctor/schemas/tacos/templates/grill-summaries.md) ## Anti-patterns.

**Same-turn explore → propose/ff:** **Forbidden** when `grill.planning` is still `pending` — end explore turn; planning grill runs on the next **propose** / **ff** turn ([read-graphs/explore.md](read-graphs/explore.md) ## After explore / before propose).

## 7. Triggered grill (apply, sync, explore)

When `orchestration.grill_enabled` is true, **apply**, **sync**, and **explore** MUST follow [tacos-grill/references/triggered-grill.md](../../tacos-grill/references/triggered-grill.md) before guessing. Triggered grills do not gate planning artifacts; they gate inline defaults during implementation, delta merge, and decision-oriented explore.

## 8. Mandatory stage apply grill (`grill.per_task_stage`)

When the **stage grill gate** is true ([task-stage-contract.md](task-stage-contract.md) ## Stage grill gate; [config-notation.md](config-notation.md)):

1. At each `tasks.md` `## N` stage, the **first** checklist line MUST be `Stage grill:` (see [task-stage-contract.md](task-stage-contract.md)).
2. Orchestration MUST **STOP** at each unchecked `Stage grill:` line and run mandatory parent `tacos-grill` **apply** for **that stage only** (`AskQuestion` / grill mode per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) **Mandatory stage apply**, then topic interview per ## Interview minimum (full / short / defaults)) before checking off **any** implementation work in that stage — prompts in [tacos-grill/references/grill-prompts/apply-mandatory.md](../../tacos-grill/references/grill-prompts/apply-mandatory.md). **Forbidden:** treating grill mode selection as completing stage grill; treating the checkbox as metadata; skipping stage grill unless the user chooses **skip**/waive.
3. Append **User inputs** under `## apply` in `grill-summaries.md`; on requirement changes, **full-resync** affected planning artifacts and change delta specs before implementation checkboxes.
4. **Triggered** apply grill (section 7) still runs before **each** pending implementation checkbox when signals match — stage grill does not replace it. Orchestration enforcement: [task-stage-contract.md](task-stage-contract.md) ## Apply (apply); [triggered-grill.md](../../tacos-grill/references/triggered-grill.md) **Mandatory stage grill vs triggered apply**.

When `grill.per_task_stage` is **false**, omit `Stage grill:` lines and skip section 8; section 7 unchanged.

## 9. Update grill (`update` only)

When **update** revises existing planning artifacts and `orchestration.grill_enabled` is true:

1. On a **fresh** revision turn, when `grill.update` is `pending`, run **tacos-grill** with phase **`update`** first (gather → parent interview → summarize) per [tacos-grill/references/read-graphs/update.md](../../tacos-grill/references/read-graphs/update.md). Interview topics are revision-oriented (anchor edit, bidirectional ripple, minimal delta, redirect) — not greenfield planning motivation.
2. On **re-entry** when `grill.update` is already `complete` or `skipped`, skip update grill; still run stock reconcile and POST-ARTIFACT as toggles require.
3. Set `grill.update` to `complete` or `skipped` in change frontmatter (or `grill-summaries.md` when present); non-empty **User inputs** required for `complete` (unless skip).

**Forbidden:** treating stock **update** coherence pass as substitute for update grill when `grill.update` is `pending` and `orchestration.grill_enabled` is true; skipping per-artifact user confirm before writes; creating missing artifacts or advancing build frontier.

### Update frontmatter

```yaml
grill:
  update: pending | complete | skipped
```

**`update`:** one interview before reconcile on a fresh revision turn. Distinct from `grill.planning` (greenfield **propose** / **ff**).
