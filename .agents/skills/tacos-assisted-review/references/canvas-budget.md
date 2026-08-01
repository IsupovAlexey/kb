# Canvas budget (capacity)

Cursor canvas is a **compiled React file** with **all data inlined** (`cursor/canvas` — no `fetch`). Every `DiffView` line is a JS object plus Shiki highlighting at render time. There is **no published hard limit**, but large canvases **slow compile**, **blank/error** in the IDE, or feel like an unusable wall of cards.

**Rule:** The **markdown companion** carries **full** walkthrough fidelity ([walkthrough-document](walkthrough-document.md) triads for every important path). The **canvas** is a **budgeted highlight reel** — same story and order, **fewer/smaller** `DiffView` payloads. Never stuff the entire branch into one `.canvas.tsx`.

Read host `canvas/SKILL.md` and check `<name>.canvas.status.json` beside the file after write (`status`, `error`, `diagnostics`) — simplify and rewrite if build failed.

## What costs capacity

|Cost driver|Mitigation|
|-|-|
|Many `DiffView` `lines={…}` entries|Cap stops and lines per stop ([Targets](#targets))|
|Full-file diffs|Hunk only: changed lines + ≤5 lines context per side|
|One `Card` per file with identical chrome|`Card collapsible` for lower P1; narrative-only stops|
|Charts, `UsageBar`, large `Table`s|Avoid ([canvas-components](canvas-components.md))|
|Huge inline string arrays in `.canvas.tsx`|Trim canvas; full text stays in markdown|

## Pre-flight (before writing canvas)

Run `git diff --stat` and `git diff --numstat` (or PR equivalent). Estimate:

|Signal|Label|
|-|-|
|≤10 files and ≤250 changed lines|**Small**|
|≤18 files and ≤500 changed lines|**Medium**|
|Above medium|**Large**|

Record: `files_changed`, `lines_added`, `lines_deleted`, `inventory_count`, planned **full triad** count (P0+P1).

## Targets (stay under — do not “fill to”)

Soft caps for **canvas only** (markdown companion is not capped the same way):

|Budget|Small|Medium|Large|
|-|-|-|-|
|Full triad stops (`Text` + `DiffView` + look outs)|all P0+P1|all P0 + top P1 by rank|**all P0** + **≤5** P1|
|Max `DiffView` **line objects** per file|120|80|60|
|Max **total** `DiffView` line objects in file|800|600|450|
|`.canvas.tsx` source lines (approx.)|≤450|≤400|≤350|

**P0** (review-flagged) always gets a canvas triad within per-file line cap — trim hunk width, not omission.

**P1** beyond the cap: on canvas use **short narrative + `<Code>` path + `DiffStats`** and point to markdown for full code; still **full triad in markdown**.

**P2–P4:** canvas **Also in this change** only (`<Code>` path + reason). Full triad in markdown when class A/B warrants.

## Trimming hunks for `DiffView`

1. Parse unified diff per file; keep **changed** lines plus small context.
2. Drop unchanged runs longer than 5 lines (replace with `{ type: "unchanged", content: "…" }` **only** if the SDK needs continuity — prefer omitting middle context entirely).
3. For files with >60 lines in one hunk, split canvas presentation: **first hunk only** + narrative “see markdown companion for remaining hunks in this file.”

## Canvas presentation tiers

|Tier|Canvas|Markdown|
|-|-|-|
|**A**|Full triad|Full triad|
|**B**|Narrative + `DiffStats` + look outs; no `DiffView`|Full triad|
|**C**|`CollapsibleSection` or bullet in **Also in this change**|Full or brief triad per class|

Assign tier from [Targets](#targets). Default important paths to **A** in markdown; downgrade canvas tier when pre-flight is **Medium** or **Large**.

Use **`Card collapsible`** with `defaultOpen={true}` for first 2–3 P1 stops, `defaultOpen={false}` for the rest when still showing `DiffView`.

## pr-review-canvas alignment

Match the floor from marketplace **pr-review-canvas**: core logic full, wiring condensed, boilerplate **listed not diffed**. Assisted review maps that to: **Walkthrough** (budgeted diffs) → brief wiring → **Also in this change** (mechanical list).

## Very large PRs (cannot fit one canvas)

When pre-flight is **Large** or inventory count would exceed **Large** targets, **expect** a multi-part delivery. Offer this package in chat:

|Piece|What the reviewer gets|
|-|-|
|**Markdown companion**|**Complete** — all P0/P1 triads ([walkthrough-document](walkthrough-document.md))|
|**`_full.patch`**|Grep the whole diff without re-running git|
|**Canvas part 1**|`canvases/<slug>-<timestamp>.canvas.tsx` — Overview + P0 + top-ranked P1 (within budget)|
|**Canvas part 2…N** _(optional)_|`canvases/<slug>-<timestamp>-walkthrough-2.canvas.tsx`, `-3`, … — next P1 chunks, same triad shape|
|**Deep dive**|Same-session follow-up for paths only listed on canvas part 1 ([follow-up-deep-dive](follow-up-deep-dive.md))|

Record in `_session.md`:

```yaml
canvas_parts:
  - path: canvases/<slug>-<timestamp>.canvas.tsx
    covers: P0 + P1 paths …
  - path: canvases/<slug>-<timestamp>-walkthrough-2.canvas.tsx
    covers: …
size_class: large
```

**Do not** cram part 2 into part 1 when `.canvas.status.json` fails or line totals exceed **Large** caps — spawn the next canvas file instead.

Initial chat lists **all** canvas part links + markdown + deep-dive invite. Part 2+ may be generated on first delivery (if time permits) or on user request (“show walkthrough part 2”).

## After write

1. If `<slug>-<timestamp>.canvas.status.json` exists and `status` is not ok — reduce payload, split to `-walkthrough-2`, or rewrite once.
2. Chat pointer: markdown is **complete**; canvas part(s) are **highlights**; name remaining paths in **Also in this change** or part 2.

## Agent failures (canvas)

- Embedding every file’s full diff in canvas while markdown also has everything (double bulk + compile risk).
- Exceeding **Large** targets “because completeness.”
- Ignoring `.canvas.status.json` build errors.
- Using `UsageBar` / charts to substitute for diffs ([canvas-components](canvas-components.md)).
