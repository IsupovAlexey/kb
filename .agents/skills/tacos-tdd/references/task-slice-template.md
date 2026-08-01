# Task slice template (tasks.md)

For TDD-marked changes (`tdd.md` present) with `orchestration.staged_apply_enabled`, each **behavioral** slice in a `## N.` stage uses separate **Red**, **Green**, and **Refactor** checkboxes before **Apply review** and **Human review**.

One slice ≈ one `### Requirement:` title in change specs (or one numbered `design.md` decision cited in the task row).

## Behavioral slice example

```markdown
## 2. Export service

- [ ] Stage grill: …
- [ ] Red: `ExportService` returns CSV for empty filter — write test, run, paste failure in chat
- [ ] Green: minimal `ExportService` implementation until test passes
- [ ] Refactor: extract CSV row formatter without behavior change
- [ ] Apply review: …
- [ ] Human review: Pause before stage 3
```

**Red row done-when:** Failing test output pasted in chat (logs optional supplement).

**Green row done-when:** Test passes; Red done-when satisfied or waiver recorded per [red-green-refactor.md](red-green-refactor.md) **Waiver recording**.

**Refactor row done-when:** Tests green after cleanup; no new behavior.

## Multiple slices in one stage

When a stage delivers two requirements, use two Red/Green/Refactor triples (order: slice A Red → Green → Refactor, then slice B Red → Green → Refactor) unless tasks.md explicitly interleaves otherwise.

## Docs-only slice example

When the stage changes only markdown, skill prose, schema instructions, or other non-executable artifacts:

```markdown
- [ ] 4.1 Add README skill catalog row for `/tacos-tdd`
- [ ] Tests: N/A — documentation only; verify in stage validation tasks
```

Use a one-line reason on `Tests: N/A` — not an implicit skip of Red on behavioral work.

## Non-TDD changes

Changes without `tdd.md` keep the standard implement-first stage shape (implementation checkboxes, then stage **Tests** tail). Do not add Red/Green/Refactor rows unless the change becomes TDD-marked.
