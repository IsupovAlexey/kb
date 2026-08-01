# POST-ARTIFACT hub (toggle table)

When apply-ready and `orchestration.enabled` is true, continue the **same turn** via [post-artifact-index.md](../post-artifact-index.md) — load step bundles on demand; not stock opsx end state.

Step router: [post-artifact-index.md](../post-artifact-index.md). Hub router: [orchestration-binding.md](../orchestration-binding.md).

|Step|Ref|
|-|-|
|OpenSpec validate|After full apply-ready bundle and after E2E / spec review / fixes / delta re-review (r2) when those ran — `openspec validate <change-id> --strict --no-interactive` last automated POST-ARTIFACT step — [post-artifact-planning-review.md](../post-artifact-planning-review.md) ### OpenSpec validate|

## Toggle table

|Toggle|Skill / ref|
|-|-|
|`orchestration.planning_review_enabled`|`tacos-spec-review` (Task only); initial pass parallel core + per-additional when configured; parent merge; delta re-review (r2) parallel when additional skills configured and concurrent spawn supported, sequential fallback when not|
|`orchestration.e2e_enabled`|`tacos-e2e-scenarios`|
|`orchestration.staged_apply_enabled`|`task-stage-contract.md` on `tasks.md` and apply; parallel apply review + parent merge per stage when additional skills configured|
|`grill.per_task_stage`|When the **stage grill gate** is true ([task-stage-contract.md](../task-stage-contract.md) ## Stage grill gate; absent key: treat as **true**; tacos-doctor **update** should add `per_task_stage` explicitly), generated `tasks.md` MUST lead each `## N` stage with a `Stage grill:` line before implementation checkboxes (canonical template: `task-stage-contract.md` ## Stage grill line); apply MUST run mandatory stage-start tacos-grill **apply** phase before that stage's implementation work. When `false`, omit `Stage grill:` lines; triggered apply grill unchanged.|
