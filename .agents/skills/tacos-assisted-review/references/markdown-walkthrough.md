# Markdown walkthrough (companion + no-canvas hosts)

Structure, triads, inventory, and sections: [walkthrough-document](walkthrough-document.md). This ref covers **markdown-only** formatting and build order.

## When to write

|Host|Path|
|-|-|
|**Cursor + canvas**|Always `artifacts/assisted-review/<slug>/<timestamp>-assisted-review.md` **and** budgeted canvas|
|**No canvas**|Markdown only|

## Build order

1. Inventory + rank P0→P4 per [walkthrough-document](walkthrough-document.md).
2. **Write markdown** — full P0/P1 triads per [Triad per stop](walkthrough-document.md#triad-per-stop) (**Markdown pattern** row); optional wiring; **Also in this change** bullet list.
3. **Write canvas** per [canvas-integration](canvas-integration.md) when host supports canvas.

## File header (when canvas exists)

```markdown
# Assisted review — <title>

**Canvas:** [open walkthrough](file:///…/canvases/<slug>-<timestamp>.canvas.tsx)
```

## Markdown-only rules

- Post-change fenced code — **not** ` ```diff ` fences; fence language matches file type.
- One `###` + `file://` heading per P0/P1 stop (same order as canvas).
- **Also in this change:** bullet list per [walkthrough-document](walkthrough-document.md#also-in-this-change-collapsible) (canvas uses `CollapsibleSection` — [canvas-components](canvas-components.md)).

## Chat pointer

Absolute paths to markdown and canvas part(s); offer deep dive once ([output-delivery](output-delivery.md)).
