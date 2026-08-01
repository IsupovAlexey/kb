# tacos-work phase 2.5 — Execute confirm

Runs after Phase 2 for all `/tacos-work` invokes (grill on or off).

## MUST read

- [../../../tacos-orchestration/references/structured-gate-convention.md](../../../tacos-orchestration/references/structured-gate-convention.md) — `proceed` / `edit` / `cancel`
- [../../../tacos-grill/references/interview-prompt.md](../../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable

## During phase

|Step|Action|
|-|-|
|2.5.1|`tacos-work [<slug>]: confirm — awaiting execute approval`|
|2.5.2|**Playback summary** in chat before the gate: **Do** from **In scope** (+ **Planning** **Decisions** when relevant); **Not do** from **Out of scope**; **Complexity note** from Intent; unresolved **Planning** **Open questions** when present|
|2.5.3|Structured gate: **next tool call MUST** be `AskQuestion` / `AskUserQuestion` when available — option ids `proceed`, `edit`, `cancel`|
|2.5.4|**Proceed** → Phase 3. **Edit** → adjust `tasks.md` (Intent subsections and **## Work**); re-offer playback when ready. **Cancel** → stop|

## Gate

No Phase 3 edits until **Proceed** or explicit execute-after-corrections in chat.

## Done when

- User chose **Proceed** (or execute-after-corrections)
- Next: [phase-3-implement.md](phase-3-implement.md)
