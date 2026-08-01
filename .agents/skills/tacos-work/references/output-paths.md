# Work output paths

Work artifacts are gitignored: session checklist at `artifacts/tacos-work/<slug>/tasks.md`; apply-review at `artifacts/openspec-reviews/<slug>/apply-review.md`. Optional archive (explicit `/tacos-work archive` only) writes git-tracked `session.md` under `openspec/changes/archive/`.

## Layout

```text
artifacts/tacos-work/<slug>/
  tasks.md              # Intent + Planning (grill) + ## Work

artifacts/openspec-reviews/<slug>/
  apply-review.md

openspec/changes/archive/<YYYY-MM-DD>-<slug>/
  session.md            # optional — distilled historical record ([archive-session-template.md](archive-session-template.md))
```

|Path|Contents|
|-|-|
|`artifacts/tacos-work/<slug>/tasks.md`|**Intent** (verbatim), **Planning** (grill), **## Work** (checkboxes) — [tasks-template.md](tasks-template.md)|
|`artifacts/openspec-reviews/<slug>/apply-review.md`|End-of-run **tacos-apply-review**|
|`openspec/changes/archive/<date>-<slug>/session.md`|Optional git archive — [archive-mode.md](read-graphs/archive-mode.md)|

`<slug>` from [slug-resolution.md](slug-resolution.md).

## Write order

Create `tasks.md` (Intent + scaffolds) → planning grill updates **Planning** in same file → fill **## Work** → execute confirm → implement. [session-runbook.md](session-runbook.md).

## Examples

|Description|Slug|Session file|
|-|-|-|
|"Add CSV export for billing admins"|`add-csv-export-billing-admins`|`artifacts/tacos-work/add-csv-export-billing-admins/tasks.md`|

Resume same slug only when user confirms. Commit code and main specs; `artifacts/` stays gitignored. Archive `session.md` when the user runs `/tacos-work archive` — preview + approve per [archive-mode.md](read-graphs/archive-mode.md).

## Related (not work-owned)

|Path|Notes|
|-|-|
|`openspec/changes/<name>/`|Full tacos|
|`artifacts/session-handoff/`|**tacos-handoff**|
