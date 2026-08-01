# Artifact-informed grouping

OpenSpec artifacts plus the real diff → **review slices** on one presentation branch. **Diff cohesion** wins over `tasks.md` stages when they disagree. **Do not** map stages 1:1 to slices by default.

## Read (change folder)

|Source|Use for|
|-|-|
|`design.md`|Dependencies, goals/non-goals, risky areas|
|`tasks.md`|Stage boundaries (advisory); note mismatch in plan|
|`specs/**/*.md`|Capability boundaries|
|`grill-summaries.md`|User constraints|
|`proposal.md`|Scope|

## Diff analysis

1. `git diff <trunk>...HEAD`
2. Per file needing partial splits:

   ```bash
   git diff <base>...HEAD -- <path> | dotnet run ../../tacos-split-diff/scripts/split-diff.cs analyze
   ```

3. Map: `path` → `hunks[].index` (0-based)

## Grouping rules

- One slice = one cohesive review unit; bottom slice = foundations first.
- **Whole-file** from feature tip only when all hunks for that file are in that slice.
- **Partial file:** disjoint hunk sets; cumulative hunks 1..k per `execute-slice` ref.
- **Binary / `hunks: []`:** whole-file checkout; note in plan.
- **Deleted / renames:** one slice owns the pair.

**Review order = commit order** (bottom → top). One merge PR; **Review passes** in merge description (`descriptions` ref).

When the change folder is its own slice, put full `openspec/changes/<change>/` in **slice 1** when cohesive, then implementation in dependency order.

**Titles:** host rules (`pr-title-conventions` ref); optional `[n/N]`. Rebalance by churn (~2× median cap) moving **whole groups** only when needed.

## Plan summary must include

Feature branch, trunk, `review/<change-slug>`, slice count (bottom → top), size table, files/hunk indices, single merge PR strategy, note when `tasks.md` ≠ diff grouping.
