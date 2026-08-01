# Output delivery

## Deliverables

After diff resolution, optional binding, and [review-delegation](review-delegation.md) (when applicable), produce **one** logical walkthrough per [walkthrough-document](walkthrough-document.md).

|Deliverable|Path|When|
|-|-|-|
|**Markdown companion**|`artifacts/assisted-review/<slug>/<timestamp>-assisted-review.md`|**Always** (revisitable, diff-friendly, comparable to canvas)|
|**Canvas**|`canvases/<slug>-<timestamp>.canvas.tsx` (**exact** pattern; [budgeted](canvas-budget.md))|Cursor when host canvas skill is installed|

Use the **same** `<slug>` and `<timestamp>` for the pair. Cross-link in the markdown header and in a canvas `Callout` (see [markdown-walkthrough](markdown-walkthrough.md), [canvas-components](canvas-components.md)).

## Host matrix

|Host|Write|Chat|
|-|-|-|
|**Cursor + canvas**|Markdown **then** canvas (both before chat)|Short pointer + **both** absolute links|
|**No canvas**|Markdown only|Pointer + markdown path|

**MUST NOT** put the full walkthrough body in chat when artifacts exist.

**MUST NOT** split sections across multiple agent turns unless binding confirm required a prior turn.

## Markdown artifact

```text
artifacts/assisted-review/<slug>/<timestamp>-assisted-review.md
```

|Field|Rule|
|-|-|
|**slug**|PR number when PR review; else bound change id; else `_adhoc`|
|**timestamp**|Local ISO compact: `YYYY-MM-DDTHHMMSS`|
|**Body**|[markdown-walkthrough](markdown-walkthrough.md) — same sections as canvas|
|**Parent dirs**|Create as needed; gitignored `artifacts/`|

Do **not** write under `openspec/changes/`.

On Cursor without canvas: markdown only; optional one-line reminder that canvas skill enables `.canvas.tsx`.

## Cursor + canvas (dual write)

1. Read host canvas `SKILL.md`, `sdk/index.d.ts`, [canvas-integration](canvas-integration.md), [canvas-components](canvas-components.md).
2. **Write** `artifacts/assisted-review/<slug>/<timestamp>-assisted-review.md` with full content ([markdown-walkthrough](markdown-walkthrough.md)).
3. **Write** `canvases/<slug>-<timestamp>.canvas.tsx` per [canvas-budget](canvas-budget.md) (highlight reel; markdown is complete).
4. Check `<slug>-<timestamp>.canvas.status.json` beside the canvas file; rewrite slimmer if build failed.
5. Chat: markdown link; canvas link(s) from `_session.md` `canvas_parts`; note **large PR** multi-canvas package when applicable ([canvas-budget](canvas-budget.md)). Offer deep dive ([follow-up-deep-dive](follow-up-deep-dive.md)).

Forbidden on Cursor + canvas: “continued in chat above (no canvas)”; canvas without markdown companion.

## Session artifacts (first delivery)

Under `artifacts/assisted-review/<slug>/` also write:

- `_full.patch` — full unified diff ([diff-input](diff-input.md))
- `_session.md` — index (timestamp, range, bound change, artifact paths)

See [follow-up-deep-dive](follow-up-deep-dive.md).

## After delivery

Report both paths when canvas exists; markdown-only path otherwise. **Offer deep dive once** ([follow-up-deep-dive](follow-up-deep-dive.md)).
