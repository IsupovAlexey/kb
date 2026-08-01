# Triggered grill (apply, sync, explore)

When `orchestration.grill_enabled` is true, orchestration **MUST** run this checklist on **apply**, **sync**, and **explore** before guessing. Triggered grills do **not** gate planning artifacts (`proposal` … `tasks`). They **do** gate guessing during implementation, delta merge, and decision-oriented explore.

**Stock opsx** apply/explore steps are incomplete for tacos hosts — inline clarification when a signal below fires is **forbidden**; run **tacos-grill** instead.

## Mandatory stage grill vs triggered apply

When the **stage grill gate** is true ([task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate), orchestration runs **mandatory** tacos-grill **apply** at each `tasks.md` `## N` stage start **before** that stage's implementation checkboxes. Prompts: [grill-prompts/apply-mandatory.md](grill-prompts/apply-mandatory.md).

**This file** governs **triggered** apply only. Stage grill does **not** satisfy or replace triggered checks. In the same stage, both MAY run: stage grill once at the boundary; triggered grill **before each** implementation checkbox when a signal below matches. Orchestration gates: [grill-gates.md](../../tacos-orchestration/references/grill-gates.md) §7–§8; apply order: [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Apply (apply).

## When to evaluate

|Command|Evaluate|
|-|-|
|`apply`|**Before each** pending `tasks.md` implementation checkbox (not the `Stage grill:` line); **again** mid-task if drift appears. Stage-start mandatory grill is separate — see above|
|`sync`|Before each delta→main step that needs human semantics|
|`explore`|When explore signals fire (not a substitute for `grill.planning`)|

## Signals (`grill.triggers` in `openspec/tacos.yaml`)

When a key is `true`, run triggered grill if **any** signal in that group matches.

|Key|Command|STOP when|
|-|-|-|
|`grill.triggers.apply_on_ambiguous_task`|apply|Task lacks done-ness; multiple valid implementations; task vs artifacts conflict; you would pick defaults without user input|
|`grill.triggers.apply_on_spec_drift`|apply|Implementation violates a **behavioral** obligation in specs (SHALL/MUST) or acceptance in tasks/design; edits contradict planning or `grill-summaries.md`; user overrides planning without artifact updates. **Not** presentation or inventory detail intentionally omitted from specs when design defers and tasks cover acceptance (e.g. Figma color match while specs require visibility without prescribing hex values). **Not** procedure (paths, pipeline order, templates) intentionally omitted from specs when design/tasks or named skill references cover it|
|`grill.triggers.sync_on_ambiguous_delta`|sync|Conflicting SHALL/MUST vs main; unclear merge/replace/split; archive/migration unclear|
|`grill.triggers.explore_on_decision`|explore|User compares options or tradeoffs; decisions worth a future **propose**; spike conclusions should persist|

**Explore is not grilling:** `## explore` does **not** replace `grill.planning` or phase grills on **propose**, **ff**, or **continue**.

## Procedure

1. **STOP** implementation, merge, or explore conclusions.
2. Run **tacos-grill** phase `apply`, `sync`, or `explore` (no gather).
3. Parent interview: 1–3 questions per [interview-prompt.md](interview-prompt.md) (**Triggered grills**).
4. Append **User inputs** (and **Decisions** when resolved) under `## <phase>` in `grill-summaries.md` (create section if missing).
5. On spec drift: propose artifact updates; do not continue with stale planning.
6. Resume after user answers or explicit waiver (record waiver in chat and **User inputs**).

Prompts: `grill.prompts.<phase>` or the matching bundle in [grill-prompts.md](grill-prompts.md) (`apply` triggered → `grill-prompts/apply-triggered.md`; `explore` → `grill-prompts/explore.md`; `sync` → `grill-prompts/sync.md`). `/tacos-grill` with phase `apply`, `sync`, or `explore` follows the same procedure.
