# Escape hatches

Host OpenSpec extension command aliases (`/opsx-*`) map to full tacos workflow — see [openspec-commands.md](../../tacos-orchestration/references/openspec-commands.md).

Stop the work workflow and recommend **full tacos** (`/opsx-propose` or `/opsx-ff`) when work-session constraints no longer fit.

## Stop before implementation

|Signal|Action|
|-|-|
|Planning grill reveals **multiple independent outcomes** needing separate review passes|Stop; propose full change folder with multi-stage tasks|
|Scope is **multi-day** or spans unrelated subsystems|Stop; full tacos for staged apply and planning review|
|Work needs **delta specs in git**, Jira sync, or PR description from change folder|Stop; use full tacos integrations|
|User needs **POST-ARTIFACT spec-review** or E2E scenarios artifact|Stop; enable via full tacos orchestration|
|Breakdown naturally needs **multiple implementation stages**|Stop; work allows single **## Work** only|

Tell the user why and suggest `/opsx-propose <name>` with a short scope summary from the grill.

## During implementation

|Signal|Action|
|-|-|
|Implementation reveals spec/design gap affecting behavioral obligations|Pause; update main specs and artifacts plan, or stop for full planning resync|
|Triggered apply grill (`grill.triggers.apply_on_spec_drift`) fires|Run short parent grill; if requirements change materially, stop for full tacos|
|Apply-review returns BLOCKERs requiring planning artifact changes|Fix in session when small; otherwise stop and open full change|

## Stay on work when

- Single bounded feature with one review pass
- Direct main-spec edits already planned
- Ephemeral checklist under `artifacts/tacos-work/<slug>/` is acceptable
- Progress tracked via `tasks.md` checkboxes (no `openspec status` required)

## Handoff and PR

**tacos-work** ships its skill bundle only. **tacos-handoff**, **tacos-pr**, and orchestration entry hooks are unchanged; invoke handoff or full tacos PR flows manually when needed. Optional session archive: `/tacos-work archive` after done — not a change-folder or **tacos-pr** integration.
