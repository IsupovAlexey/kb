# Follow-up and deep dive (same session)

After initial delivery, free-form questions are **expected**. Same triad: **narrative → code → look outs** ([walkthrough-document](walkthrough-document.md)).

## After initial delivery (offer once)

One line inviting deep dive — file, topic, or paths from **Also in this change**.

## Session bundle (reuse)

|File|Purpose|
|-|-|
|`<timestamp>-assisted-review.md`|Primary walkthrough — **append** deep dives here|
|`_full.patch`|Grep without re-running git|
|`_session.md`|Index: compare, `gate_artifacts`, `canvas_parts`, markdown path|

Reuse `gate_artifacts` from `_session.md` — no re-delegate unless diff changed or user asks to refresh.

## Follow-up procedure (mandatory Write)

1. Parse the user’s focus (file, topic, “part 2”, compare A vs B).
2. Load `_session.md`, `_full.patch`, markdown companion, gate files listed in `gate_artifacts:`.
3. **Write artifacts before chat:**
   - **Markdown:** append `## Deep dive — <topic>` to the session `<timestamp>-assisted-review.md` (full triad), or add `<timestamp>-deep-dive-<n>.md` and link from `_session.md`.
   - **Canvas (Cursor):** append stops to the session `.canvas.tsx` within [canvas-budget](canvas-budget.md), or Write `canvases/<slug>-<timestamp>-deep-dive-<n>.canvas.tsx`; update `_session.md` `canvas_parts`.
4. **Chat:** one line — what was appended and absolute paths. **No** walkthrough body in chat.

Skipping step 3 is a contract violation — same as dumping the initial walkthrough in chat.

## New invoke vs same chat

|Case|Behavior|
|-|-|
|Same chat|Follow-up — reuse bundle, Write append|
|New invoke, same slug, same diff|Offer continue prior session or fresh timestamp|
|New diff / PR|New `_full.patch`, new timestamp|
