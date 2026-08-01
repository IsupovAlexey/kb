# Rubric (ad hoc)

When change binding is declined, skipped, or unavailable.

## Presentation lenses

|Lens|Action|
|-|-|
|Reviewer-value grouping|Regroup hunks: core logic, wiring/integration, boilerplate (generated, lockfiles, formatting)|
|Tricky highlights|Call out surprising control flow, concurrency, security-sensitive paths|
|Cross-file|Note contracts spanning files (API + impl, schema + migration)|
|Conventions|Light pass per [tacos-implementation-conventions](../../tacos-implementation-conventions/SKILL.md) when in-repo code|
|Host standards|`AGENTS.md` managed blocks, `openspec/config.yaml` context when reviewing the host project|

## Ad hoc scope

Apply presentation lenses inside the single [walkthrough-document](walkthrough-document.md). Reserve spec traceability and planning alignment for bound mode ([rubric-bound](rubric-bound.md)) or gate skills (`/tacos-spec-review`, `/tacos-apply-review`).

## Weak-spot tags (advisory)

Fixed floor — use when genuinely warranted; phrase concerns as **worth checking**:

|Tag|Use when|
|-|-|
|Subtle|Easy-to-miss behavior change|
|Breaking|Compatibility or contract risk|
|Race|Concurrency / ordering|
|Perf|Hot path or allocation concern|
|Security|Authz, injection, secrets|
|Scope|Change wider than the hunk suggests|

Fixed floor per apply grill — same advisory tag set as [grill-prompts/apply-triggered.md](../../tacos-grill/references/grill-prompts/apply-triggered.md) weak-spot tags.

Agent MAY add a rare extra tag with one-word justification.

## When the user wants gate output

|Need|Point to|
|-|-|
|Implementation diff gate|`/tacos-apply-review`|
|Planning artifact gate|`/tacos-spec-review`|
|Diff navigation only|This walkthrough document|
