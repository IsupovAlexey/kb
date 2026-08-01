# Config notation (`openspec/tacos.yaml`)

All tacos skills read runtime switches from **`openspec/tacos.yaml`**. Use the patterns below so agents do not confuse yaml sections or repeat long gate expressions.

## Yaml sections

|Section|Examples|Skill entry line pattern|
|-|-|-|
|`orchestration`|`enabled`, `grill_enabled`, `staged_apply_enabled`, `planning_review_enabled`, `e2e_enabled`, `spec_grounding_models`|`Config: openspec/tacos.yaml` (`orchestration.*`, …)|
|`grill`|`per_task_stage`, `default_max_questions`, `triggers.*`, `prompts`, `*_models`|`grill.*`|
|`review`|`spec_review_models`, `apply_review_additional_skills`|`review.*`|
|`jira`, `pr`, `slice_pr`, `project_overview`, `dedrift`|section-scoped keys|`jira.*`, `pr.*`, `dedrift.*`, …|

## Qualified vs short keys

|Context|Notation|
|-|-|
|Cross-section gates, normative specs, host `AGENTS.md` read order, grill ↔ orchestration|**Fully qualified:** `orchestration.grill_enabled`, `grill.per_task_stage`|
|`post-artifact-gates.md` and `post-artifact-index.md` (entire file each)|**Short** orchestration toggle names (`grill_enabled`, `staged_apply_enabled`, …)|
|Markdown **table rows** in `tacos-orchestration/SKILL.md` and `orchestration-binding.md`|Short orchestration keys in table cells only|
|`grill.planning`, `grill.triggers.apply_on_ambiguous_task`, `grill.default_max_questions`|Always under **`grill:`** — never shorten to `planning`, `apply_on_ambiguous_task`, or `default_max_questions` alone|
|Command tables in **tacos-orchestration** `SKILL.md` (Before/During/After)|`grill.triggers.<key>` for trigger toggles; orchestration prose outside tables uses `orchestration.*`|

Outside allowlisted table rows, use `orchestration.<name>` or **stage grill gate** prose (`grill.*`, `jira.enabled`, … unchanged).

## Stage grill gate (canonical phrase)

Mandatory `Stage grill:` lines and stage-start apply grill apply only when **all** are true:

- `orchestration.grill_enabled`
- `orchestration.staged_apply_enabled`
- `grill.per_task_stage` (absent key → **true**)

Normative detail: [task-stage-contract.md](task-stage-contract.md) ## Stage grill gate. In operational prose, prefer **stage grill gate** with that link instead of repeating the three keys.

## Absent-key defaults (skills MUST honor)

|Key|When absent|
|-|-|
|`grill.per_task_stage`|**true** (tacos-doctor **update** should add the key explicitly)|
|`pr` section|GitHub writes enabled; see `tacos-pr`|
|`slice_pr.enabled`|per template / `post-artifact-gates.md`|

## Host snippets

`AGENTS.md` managed blocks and `tacos-doctor` templates: use **`orchestration.grill_enabled`** (and siblings) in read-order bullets; POST-ARTIFACT summary lines may use orchestration short toggles with an explicit `orchestration:` scope in the same paragraph.
