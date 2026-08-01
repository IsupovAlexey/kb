# Canvas integration (Cursor)

One `.canvas.tsx` plus a **markdown companion** per [output-delivery](output-delivery.md). Prerequisite: [review-delegation](review-delegation.md).

## Procedure

1. Read host canvas `SKILL.md`, `sdk/index.d.ts`, [canvas-components](canvas-components.md), and [canvas-budget](canvas-budget.md).
2. Build path inventory, classify, and **rank** P0→P4 per [walkthrough-document](walkthrough-document.md); read `*-assisted.md`.
3. **Pre-flight** diff size; plan canvas tier A/B/C per [canvas-budget](canvas-budget.md) (markdown stays full fidelity).
4. **Write** [markdown-walkthrough](markdown-walkthrough.md) first — full P0/P1 triads per [walkthrough-document](walkthrough-document.md).
5. **Write** `canvases/<slug>-<timestamp>.canvas.tsx` within [canvas-budget](canvas-budget.md) — tier A/B/C presentation per budget table (triad shape: [Triad per stop](walkthrough-document.md#triad-per-stop) **Canvas pattern** row).
6. Read `<slug>-<timestamp>.canvas.status.json` if present; simplify and rewrite once on build error.

## Layout (target density)

Use **`Stack`** as the page root. Top-level sections match [walkthrough-document](walkthrough-document.md#top-level-sections). Per-stop triad component mapping: [Triad per stop](walkthrough-document.md#triad-per-stop) **Canvas pattern** row. SDK map: [canvas-components](canvas-components.md).

```
Stack
  H1 — change title
  Row — Stat (files changed) + DiffStats (scope +/− totals only)
  H2 Overview — Text
  H2 Walkthrough — P0→P1 stops (triad per walkthrough-document)
  H2 Wiring — optional brief stop or Table with headers
  H2 Also in this change — CollapsibleSection (defaultOpen false)
  Callout info — link to markdown companion (same timestamp)
```

## DiffView and links

- **Tier A** stops: `Card` (+ `collapsible` for lower P1) with trimmed **`DiffView`** per [canvas-budget](canvas-budget.md).
- **Tier B** stops: narrative + `<Code>` path + **`DiffStats`**; full code only in markdown.
- **No** `c:/` / `file://` links in canvas ([walkthrough-document](walkthrough-document.md)).
- **`Table`** in Wiring: non-empty `headers` array required.

## Advisory

No BLOCKER/APPROVE ladder UI in the canvas.
