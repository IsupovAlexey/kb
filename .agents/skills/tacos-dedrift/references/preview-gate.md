# Dedrift preview gate

After detect completes on explicit `/tacos-dedrift` invoke, show all proposed spec and code changes in chat, then collect approval before any write. When `deep` token is present, show **one** combined preview after the re-detect loop and verify re-detect stabilize — not per iteration ([deep-mode.md](deep-mode.md)).

## Preview (required)

Include per capability in scope:

- Classification: **aligned**, **stale spec**, **code violation**, **needs human decision**
- Proposed edits (spec paths + summary; code paths when **conform**)
- Items flagged **needs human decision** (no write for those)

Multi-capability: one **combined** preview per [delegation.md](delegation.md) before serial writes.

## Gate

Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): when structured tools are available, the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` after preview — **forbidden:** prose-only `Proceed / Edit / Cancel?`. Plain-text only per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable when tools are absent.

**Prompt:** `Apply these dedrift changes to main specs (and code if conform)?`

|id|label|
|-|-|
|`proceed`|Proceed — apply previewed changes|
|`edit`|Edit — revise scope or edits; re-preview|
|`cancel`|Cancel — no writes|

On **`proceed`:** parent serializes writes per [delegation.md](delegation.md); report per [output-format.md](output-format.md).

On **`edit`:** incorporate user corrections; show revised preview; call structured gate again.

On **`cancel`:** state cancelled; no pending preview; no writes.

**Never counts as approval:** silence; ambiguous “looks good”; starting message reconcile/conform intent without preview selection.
