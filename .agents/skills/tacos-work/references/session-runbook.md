# Session runbook

Phase index for `/tacos-work`. Load **one** graph per [SKILL.md](../SKILL.md) Entry — do not read every phase file on each turn. Report: `tacos-work [<slug>]: <phase> — <next>`.

## Phase index

|Phase|Graph|When|
|-|-|-|
|0 Session|[read-graphs/phase-0-session.md](read-graphs/phase-0-session.md)|New invoke; write `tasks.md`|
|1 Grill|[read-graphs/phase-1-grill.md](read-graphs/phase-1-grill.md)|`orchestration.grill_enabled` true|
|2 Work|[read-graphs/phase-2-work.md](read-graphs/phase-2-work.md)|After grill or when grill disabled|
|2.5 Confirm|[read-graphs/phase-2.5-confirm.md](read-graphs/phase-2.5-confirm.md)|Before any implementation edit|
|3 Implement|[read-graphs/phase-3-implement.md](read-graphs/phase-3-implement.md)|After execute confirm|
|4 Review|[read-graphs/phase-4-review.md](read-graphs/phase-4-review.md)|After **## Work** complete|
|5 Done|[read-graphs/phase-5-done.md](read-graphs/phase-5-done.md)|After apply-review pass|
|Archive|[read-graphs/archive-mode.md](read-graphs/archive-mode.md)|`/tacos-work archive` after done|

## Shared gates

|Before|Required|
|-|-|
|Planning grill (when `orchestration.grill_enabled`)|`tasks.md` with **Intent**; [planning-grill.md](planning-grill.md) done or skip — **Planning** in same file|
|Any implementation edit|`tasks.md` with **## Work** + `**Testable outcome:**` + terminal review lines|
|Implementation start (Phase 3)|Phase 2.5 **Proceed**|
|Done|Apply review (includes MINOR polish when needed) + human gate (or waive)|

Do not: use invoke text as the task list; skip grill for README-only asks; set `grill_mode: skip` or fill **Planning** in Phase 0 when `orchestration.grill_enabled`; mark `grill.planning: complete` without parent interview; bypass artifacts after `/tacos-work`; add full-tacos `Stage grill:` lines; start Phase 3 without Phase 2.5 **Proceed**.

Scope escape → [escape-hatches.md](escape-hatches.md).
