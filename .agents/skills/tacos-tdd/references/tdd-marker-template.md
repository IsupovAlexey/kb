# tdd.md marker template

The **`tdd`** schema artifact generates `openspec/changes/<name>/tdd.md`. Canonical template (installed with the tacos schema bundle):

`openspec/schemas/tacos/templates/tdd.md` (installed with the tacos schema bundle via tacos-doctor).

## Inline stub

Same content as the schema template when the host install is not yet refreshed:

```markdown
# TDD

This change uses test-driven development. Apply follows Red → Green → Refactor per slice.

<!-- Optional: Test plan: openspec/test-plans/<slug>/ when eng work follows QA-committed scenarios -->

<!-- Optional: host test command hints, e.g. dotnet test path/to/tests.csproj --filter FullyQualifiedName~SliceName -->
```

The `Test plan:` line is optional — include when eng work follows QA-committed scenarios under `openspec/test-plans/<slug>/`. Omit when Red input comes only from change specs, Jira, or Figma. Eng tacos skills read the linked pack read-only; mutation is QA-only via `/tacos-test-plans`.

Behavioral SHALL/MUST obligations live in change delta specs and main specs — not in this marker file.

## When to create

- `/tacos-tdd <change-name>` — create when missing on the TDD path.
- Standard **propose** / **ff** without `/tacos-tdd` — do **not** create `tdd.md` unless the user explicitly requests TDD.

## openspec instructions

When the schema row exists: `openspec instructions tdd --change <name>` for generation rules. The artifact requires `grill-summaries` and is not in `apply.requires`.
