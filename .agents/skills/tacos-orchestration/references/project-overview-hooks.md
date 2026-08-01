# Project overview hooks (sync, archive)

Skill: `../../tacos-project-overview/SKILL.md`.

When `project_overview.enabled` is true and the matching `prompt_after_*` flag is true, orchestration **MUST** run **tacos-project-overview** in the **same turn** after stock **sync** or **archive** succeeds — three gated steps (opt in → scope plan → preview approval). Hooks **MUST NOT** write `project_overview.path` without explicit approval.

## Skip when overview task complete

Before Gate 1, read the active change's `tasks.md` (OpenSpec change folder) or, for **tacos-work**-only sessions, `artifacts/tacos-work/<slug>/tasks.md`.

When a **checked** `- [x] Project overview:` line exists covering the shipped user-visible surface for this change or session:

1. **MUST NOT** run Gates 1–3.
2. Record in the **sync** or **archive** summary: `project overview: skipped — checked task in tasks.md`.

Unchecked or absent `Project overview:` lines do not skip hooks.

## Config (`project_overview` in `openspec/tacos.yaml`)

|Flag|When|
|-|-|
|`prompt_after_sync`|After successful **sync** for a change|
|`prompt_after_archive`|After successful **archive** for a change|

Both default `false`. Require `enabled: true`.

## After sync

When `project_overview.enabled` and `prompt_after_sync` are true and stock **sync** completed for change `<name>`:

1. Read `../../tacos-project-overview/SKILL.md` (that skill’s **Entry** refs load workflow and prompts).
2. **Gate 1** — Ask whether to update `{path}` from change `<name>`; **STOP** if user declines.
3. **Gate 2** — Propose a scope plan (sections/topics add/update/remove or no change); user MUST confirm (`approve_plan`) or refine before draft.
4. **Draft + preview** — only the approved plan; then **Gate 3** per [approval-prompt.md](approval-prompt.md#project-overview).
5. **MUST NOT** skip Gates 1–2 and jump straight to draft or write.
6. **MUST NOT** substitute a “run `/tacos-project-overview` later” paragraph for this workflow.
7. **MUST NOT** write without Gate 3 **approve**.

## After archive

When `project_overview.enabled` and `prompt_after_archive` are true and stock **archive** completed for change `<name>`:

Same as **After sync**, with trigger **archive** and the archived change name.

## Manual

`/tacos-project-overview` — user supplies add/update/remove scope; read `../../tacos-project-overview/SKILL.md` (**Manual scope** + **Entry** procedure).

## Binding

- Command ids: **sync**, **archive** per [openspec-commands.md](openspec-commands.md).
- Load this file from [read-graphs/sync.md](read-graphs/sync.md) or [read-graphs/archive.md](read-graphs/archive.md) After command hooks when `orchestration.enabled` is true.
