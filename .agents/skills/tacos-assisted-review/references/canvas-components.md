# Canvas SDK (`cursor/canvas`)

Read host `canvas/SKILL.md` and `sdk/index.d.ts` before authoring. Import **only** from `cursor/canvas`. Colors from `useHostTheme()` — no hardcoded hex, gradients, box-shadows, or emoji decoration.

Assisted review SHOULD use the components below (not bare `Text` walls). Pair with [markdown-walkthrough](markdown-walkthrough.md) for the same content in `artifacts/assisted-review/`.

## Layout and structure

|Export|Use in assisted review|
|-|-|
|**`Stack`**|Top-level page wrapper; vertical sections with `gap`|
|**`Row`**|Overview metadata (`Stat` + `DiffStats` totals); inline pills in watch list|
|**`Grid`**|Two-column watch list or wiring table when many items|
|**`Divider`**|Between major `H2` sections|
|**`Spacer`**|Push metadata to the right in a header `Row`|
|**`H1` / `H2` / `H3`**|Section titles (match [walkthrough-document](walkthrough-document.md))|
|**`Text`**|Prose, pseudocode; **https** markdown links only — not repo file paths|
|**`Code`**|Repo-relative paths, `dotnet run dev/…`, identifiers|
|**`Link`**|**https** URLs only in canvas — repo files are not openable via `Link`|

## Surfaces

|Export|Use in assisted review|
|-|-|
|**`Card`** + **`CardHeader`** + **`CardBody`**|One card per P0/P1 file: header = `<Code>repo/relative/path</Code>` + **`DiffStats`** in `trailing`; body `padding: 0` + **`DiffView`**|
|**`Card` `collapsible`**|Long hunks or optional deep sections (lowest P1)|
|**`CollapsibleSection`**|**Also in this change** — one spoiler (`defaultOpen={false}`, `title="N more files"`, optional `count`) wrapping a `Stack` of `<Text><Code>path</Code> — reason</Text>` rows — not comma-separated inline lists|
|**`Table`**|**Wiring & integration** — **`headers` required** (non-empty array); e.g. `["Path", "Role", "Δ"]` — empty table causes **“Add at least one header”** runtime error|

## Diff (primary)

|Export|Use in assisted review|
|-|-|
|**`DiffView`**|Unified hunk per file; `path` for language; `lines` from patch|
|**`DiffStats`**|Per-file +/− in card header; optional scope totals in overview `Row`|

Compose per SDK docs:

```tsx
<Card>
  <CardHeader trailing={<DiffStats additions={n} deletions={m} />}>
    <Code>…/tacos-assisted-review/SKILL.md</Code>
  </CardHeader>
  <CardBody style={{ padding: 0 }}>
    <DiffView path="…/tacos-assisted-review/SKILL.md" lines={lines} />
  </CardBody>
</Card>
```

**Do not** batch P1 files as “Full hunks in markdown companion: a (+N), b (+N)…” — one card per P1 file in canvas.

## Feedback and tags

|Export|Use in assisted review|
|-|-|
|**`Callout`**|**Look outs** (triad step 3 per stop) — after each `DiffView`; optional `Pill` tags|
|**`Pill`**|Short tags next to paths (`Breaking`, `Drift`, `CRLF`)|

## Actions (sparse)

|Export|Use in assisted review|
|-|-|
|**`TodoList`**|**Do not use** — verify steps are for apply-review / PR author, not assisted walkthrough|
|**`Button`**|Rare; prefer chat paths for opening files|

## Metrics (sparse)

|Export|Use in assisted review|
|-|-|
|**`Stat`**|Scope totals only (e.g. files changed) — **not** per-file area breakdown|
|**`UsageBar`**|**Avoid** for diff reviews (misleading “area” charts); totals belong in `Stat`/`DiffStats`|

## Charts and DAG (rare)

|Export|Use in assisted review|
|-|-|
|**`BarChart` / `LineChart` / `PieChart`**|Only when the diff includes real metrics data — not for file-count vanity charts|
|**`computeDAGLayout`** + SVG|Optional **workflow** diagram when SKILL procedure has many branches (keep small)|
|**`Swatch`**|Optional leading icon on `CollapsibleSection` for P0 vs P1 groups|

## Hooks

|Export|Use in assisted review|
|-|-|
|**`useHostTheme()`**|All custom `style` overrides|
|**`useCanvasState` / `useCanvasAction`**|Optional interactivity — not required for walkthrough|

## Do not use for assisted review

- Decorative **`UsageBar`** file-area charts (stat-table noise)
- Empty **`Card`** / placeholder charts
- **`fetch`** or network calls (forbidden by canvas skill)
- Gate severity ladders styled as APPROVE/BLOCKER UI

## Cross-link companion markdown

End canvas with **`Callout` `tone="info"`** and plain text (no `file://` link):

“Markdown companion: `artifacts/assisted-review/<slug>/<timestamp>-assisted-review.md`” — user opens from chat absolute path.
