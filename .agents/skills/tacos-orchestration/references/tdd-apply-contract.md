# TDD apply contract

When `openspec/changes/<name>/tdd.md` exists, orchestration and schema `apply.instruction` select this contract instead of implement-first + stage **Tests** tail for **behavioral** slices.

Procedure detail: `../../tacos-tdd/SKILL.md` and `../../tacos-tdd/references/red-green-refactor.md`. Task row shapes: `../../tacos-tdd/references/task-slice-template.md`.

## Detection

Branch when **`tdd.md` is present** in the change folder. Do not use a global `orchestration.tdd_enabled` toggle. Absence of `tdd.md` keeps the stock staged apply contract unchanged.

## Per-stage order (TDD-marked)

For each `## N.` stage when `orchestration.staged_apply_enabled`:

1. **Stage grill** (when stage grill gate) — mandatory parent interview per `task-stage-contract.md`
2. **Per-slice Red** — write/run failing test; paste failure output in chat (done-when)
3. **Per-slice Green** — minimal implementation until test passes
4. **Per-slice Refactor** — improve structure; tests stay green
5. **Apply review** — `tacos-apply-review` (+ additional paths when configured)
6. **Human review** — pause before next stage

**Verify Decision N** and **Project overview:** rows are optional at apply time for TDD-marked behavioral stages — complete when present in `tasks.md`; do not block apply when absent (per tacos schema `apply` instruction and [task-stage-contract.md](task-stage-contract.md) ## Per-stage checklist order).

Do **not** use implementation-first checkboxes followed by a stage **Tests** tail for behavioral work in TDD-marked changes.

## Red paste gate

- MUST NOT check **Green** until paired **Red** is complete and failing output is pasted in chat visible to the user.
- Log files under `artifacts/outputs/` may supplement; they do not replace chat paste.
- User waiver for a slice → record under `grill-summaries.md` ## apply before Green.

Apply-review treats Green without visible red paste as **BLOCKER** unless waiver is recorded.

## Docs-only waiver

When a stage changes only markdown, skill prose, schema instructions, or other non-executable artifacts:

- Use `Tests: N/A` with a one-line reason instead of Red/Green/Refactor triples for that slice.
- Do not use docs-only waiver to skip tests on behavioral changes.

## Slice granularity

- One behavioral slice ≈ one `### Requirement:` title in change specs (or one cited `design.md` decision in the task row).
- Multiple requirements in one stage → multiple Red/Green/Refactor triples (or explicit docs-only waiver lines).

## Orchestration Entry

On **apply** when `tdd.md` exists: read `../../tacos-tdd/SKILL.md` and this file before processing `tasks.md` checkboxes (in addition to existing apply hooks).

## Binding

- Source of truth for ritual: `tacos-tdd` skill references
- Source of truth for checklist generation: schema `tasks` instruction (TDD-marked branch)
- Runtime gate order: read-graphs/apply.md MUST read + `task-stage-contract.md` (schema `apply.instruction` is gate header only)
