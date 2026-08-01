# Rubric (bound)

When change binding is confirmed.

## All ad hoc lenses

Apply every row in [rubric-adhoc](rubric-adhoc.md) inside one [walkthrough-document](walkthrough-document.md).

## Plus: light planning alignment

|Source|Use for|
|-|-|
|`proposal.md`|Intent in **Overview** prose when bound|
|`design.md`|Decision cross-check during core logic|
|`tasks.md`|Expected outcomes per stage in watch list|
|`specs/**`|Prose intent only|
|`grill-summaries.md`|Resolved decisions; stay consistent with recorded choices|

Phrasing examples: "Planning expected …", "Design chose …".

## Context sources

Load planning files under `openspec/changes/<id>/` only. Keep alignment advisory prose; merge authority stays with the human. When [review-delegation](review-delegation.md) runs, gate subagents produce `*-assisted.md` inputs for the walkthrough — that is not the same as the user running a separate `/tacos-apply-review` session after merge.

## Self-review vs foreign PR

Both use the same rubric. Bound foreign PR with accidental branch match → confirm step prevents wrong load.
