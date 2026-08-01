# Mode: propose (user invoke)

Load when user invokes `/tacos-tdd <change-name>`, `/tacos-tdd`, or natural-language TDD + change name.

## Flow

1. Resolve or create `openspec/changes/<name>/`.
2. Run planning grill when `orchestration.grill_enabled` (same as propose / ff) — [tacos-grill planning read graph](../../tacos-grill/references/read-graphs/planning.md).
3. Write the `tdd` schema artifact (`tdd.md`) when missing — [tdd-marker-template.md](tdd-marker-template.md).
4. Continue planning artifacts (`proposal`, specs, design, tasks) per tacos orchestration.
5. Stamp Development Approach in `proposal.md`:

```markdown
## Development Approach

This change is implemented with test-driven development (Red → Green → Refactor). See `tdd.md` and the `tacos-tdd` skill.
```

Adjust only when the user requests different wording in chat.

Do not batch planning artifacts before planning grill completes.

## Done when

- `tdd.md` exists per [tdd-marker-template.md](tdd-marker-template.md).
- Development Approach stamped in `proposal.md`.
- Planning grill complete before batched artifacts.
- Remaining planning artifacts written per orchestration.
