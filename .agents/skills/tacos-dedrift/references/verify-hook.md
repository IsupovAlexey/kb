# Verify hook

Main-spec dedrift detect after stock **verify**. Shared contract: [orchestration-dedrift-pass.md](orchestration-dedrift-pass.md). Orchestration binding: `../../tacos-orchestration/references/orchestration-binding.md` § Main-spec dedrift pass; `../../tacos-orchestration/SKILL.md` **verify** row.

## Procedure

1. User runs **verify** on a change (`/opsx-verify` or equivalent).
2. `openspec validate <change-id> --strict --no-interactive` completes (or user waives validate failures per orchestration binding).
3. Stock verify heuristics complete for the change folder.
4. Run [Shared contract](orchestration-dedrift-pass.md#shared-contract) detect for implicated capabilities — **MUST** run detect every verify turn.
5. Record detect outcome in the **verify** summary (**no-drift** or user choice).

## Split from apply-time drift

|Surface|When|
|-|-|
|`grill.triggers.apply_on_spec_drift` during change-folder **apply**|Change-folder planning artifacts|
|This verify hook|Main `openspec/specs/**` vs codebase after stock verify|
