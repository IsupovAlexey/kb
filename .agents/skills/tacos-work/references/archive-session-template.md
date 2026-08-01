# Archive session template (`session.md`)

Distilled historical record written by archive mode. Not a live session checklist — no grill frontmatter, no unchecked boxes.

## Distill from `tasks.md`

|Section|Source|
|-|-|
|Meta header|Archive date (ISO), session slug, source path|
|Intent|**## Intent** — summary when long; verbatim when short|
|Planning|**Planning → Summary**, **Decisions**, **User inputs** — condensed; omit `(pending grill)`|
|Outcome|**## Work** `**Testable outcome:**` line|
|Spec touch|**Plan** / **Spec touch** rows when present|
|Work summary|Checked implementation rows only (no Apply review / Human review lines)|

## Deterministic distill

`scripts/archive-session.cs` (and Lens when the script is installed) MUST follow these mechanical rules. Agents MUST NOT paraphrase `tasks.md` when the script is available — run [archive-script.md](archive-script.md) instead.

|Section|Rule|
|-|-|
|Intent|When **## Intent** body length exceeds 600 characters and **Planning → Summary** is non-empty, use Summary; otherwise use Intent verbatim|
|Planning|Copy **Summary**, **Decisions**, and **User inputs** as-is; omit **Open questions**; remove lines containing `(pending grill)`|
|Outcome|Extract first `**Testable outcome:**` line from **## Work**|
|Spec touch|From checked `Spec touch:` row, else `spec_touch:` / `Spec touch:` in User inputs or Decisions, else `N/A — not recorded in session checklist`|
|Work completed|`- [x]` rows under **## Work** excluding rows whose text starts with `Plan:`, `Apply review:`, `Human review:`, `Tests:`, or `Verify Decision` (case-insensitive)|

## Template

Replace `<date>`, `<slug>`, and body placeholders.

```markdown
# tacos-work session archive

Historical planning record from <date>. Verify against `openspec/specs/**` and the codebase — not current contract.

- Source: `artifacts/tacos-work/<slug>/tasks.md`
- Archived: <date>

## Intent

<intent summary or verbatim>

## Planning

### Summary

### Decisions

### User inputs

## Outcome

**Testable outcome:** <from ## Work>

## Spec touch

<paths or N/A with reason>

## Work completed

- <checked implementation rows only>
```

## Write rules

- One file per archive folder: `session.md` only
- Folder: `openspec/changes/archive/<YYYY-MM-DD>-<slug>/` per [output-paths.md](output-paths.md)
- Same calendar day + slug collision: `<YYYY-MM-DD>-<slug>-2/`, then `-3`, …
