# tacos-work phase 5 — Human gate

## During phase

|Step|Action|
|-|-|
|5.1|Pause for sign-off or waive|
|5.2|`tacos-work [<slug>]: done`|
|5.3|One-line archive nudge: run `/tacos-work archive` (or `/tacos-work archive <slug>`) to persist planning git|
|5.4|When **Project overview:** line is present and unchecked: one-line warn-nudge to complete overview via Phase 3 workflow before closing|

Phase 5 MUST NOT embed archive preview or write — archive is [archive-mode.md](archive-mode.md) only.

Scope escape → [escape-hatches.md](../escape-hatches.md).

## Done when

- Human gate signed off or waived; chat reports `tacos-work [<slug>]: done` and step 5.3 nudge when `tasks.md` still exists locally
