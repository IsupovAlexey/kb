# Test plan preview gate

Before writing or replacing files under `openspec/test-plans/<slug>/`, show the full plan summary in chat and collect approval. QA-only write path — eng workflows MUST NOT bypass this gate.

## Preview (required)

Include in chat before any structured gate:

|Section|Content|
|-|-|
|Slug|`openspec/test-plans/<slug>/`|
|Mode|Create · Update (replace) · Skipped (exists)|
|Intended files|`<slug>-test-cases.md` plus optional companions|
|Case summary|New `TC-*` ids; updates to existing ids; removals on replace|
|Placeholder count|Cases marked pending design or known inconsistency|
|Open questions|Unresolved conflicts, missing specs, unreachable URLs|
|Diagrams|Whether intro or `diagrams.md` will be written|
|Plan review|Pass · Pass with MAJOR · Blocked (waived) — per [plan-review.md](plan-review.md)|
|Scope grill|`full` / `short` / `defaults` / `skip` — per [scope-grill.md](scope-grill.md)|

When `openspec/test-plans/<slug>/` already exists:

- State **replace** explicitly.
- List files that would be overwritten.
- Do not write until the user approves replace (separate from Proceed on new slugs when the gate combines both — see below).

## Gate

Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): when structured tools are available, the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` after the full preview — **forbidden:** prose-only `Proceed / Edit / Cancel?`. Plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools are absent.

**Prompt:** `Write this test plan pack?`

|id|label|
|-|-|
|`proceed`|Proceed — write previewed pack|
|`edit`|Edit — revise plan; re-preview|
|`cancel`|Cancel — no writes|

### Existing slug

When the folder exists and the run would overwrite files:

**Prompt:** `Replace existing test plan pack for <slug>?`

|id|label|
|-|-|
|`proceed`|Proceed — replace approved files|
|`edit`|Edit — revise plan; re-preview|
|`cancel`|Cancel — no writes|

Run replace approval before or as part of Proceed when both previews apply; **forbidden:** silent overwrite.

## On outcomes

|Selection|Action|
|-|-|
|`proceed`|Write only previewed files; emit post-run summary|
|`edit`|Incorporate corrections; show revised preview; call structured gate again|
|`cancel`|Status line `Test plan: Cancelled`; no writes|

**Never counts as approval:** silence; ambiguous "looks good"; starting synthesis without preview selection.

## Delegation note

When `agent-tacos-test-plans` runs synthesis, the **parent** still owns the preview gate and write approval before any file under `openspec/test-plans/`.
