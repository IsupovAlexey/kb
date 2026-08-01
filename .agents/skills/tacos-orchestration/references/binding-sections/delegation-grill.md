# Delegation (grill)

|Step|Agent|Requirement|
|-|-|-|
|Gather|`tacos-grill-gather` Task when supported|**MUST** (pre-artifact only)|
|Interview|Parent|**MUST** — grill mode + topics before summarize (structured prompt per [structured-gate-convention.md](../structured-gate-convention.md); plain-text only when tools absent)|
|Summarize|`tacos-grill-summarize` Task when supported|**MUST** — **forbidden** until interview completes or user chose **skip**|
|Write artifact|Parent|After `grill.planning` `complete` or `skipped` + filled `grill-summaries.md`|

**Forbidden:** One Task for gather + summarize with no parent interview; `grill.planning: complete` without `grill_mode` and **User inputs** (unless skip); explore / propose text as interview substitute.
