# Post-verify confirm

Run **after** [verify-gates](verify-gates.md) (`verify-tip` and `verify-slices` exit `0`) and **before** `git push` or PR-create preview.

## Summary (required)

Presentation branch, feature branch, trunk, per-slice SHA/message/file count, verify success lines; one squash merge PR — slice commits are review surfaces only.

## Prompt

`Verification passed. Push presentation branch and continue to PR create preview?`

|id|label|
|-|-|
|`approve`|Continue — may push and show PR-create preview|
|`defer`|Keep branch local — no push, no PR|
|`cancel`|Stop|

## Outcomes

|Response|Next step|
|-|-|
|`approve`|May `git push -u origin review/<change-slug>`; parent procedure step 10|
|`defer` / `cancel`|No push, no `gh pr create`|

Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): after verify summary, **next tool call MUST** be structured prompt when tools available — **forbidden:** prose-only menu.

Verify failure → fix and re-run verify; do not offer this gate.
