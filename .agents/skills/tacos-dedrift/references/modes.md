# Dual modes

User **MUST** choose mode at explicit invoke — via slash argument or structured prompt when omitted. No implicit default mode on `/tacos-dedrift`.

## reconcile (specs ← code)

Update main spec **behavioral** obligations so they match shipped implementation.

- Read `openspec/specs/<capability>/spec.md` and relevant code, config, and skill bodies.
- Propose edits that capture new or changed SHALL-level behavior without adding presentation/inventory bloat.
- **conform** is not assumed when code changed — user chose reconcile explicitly.

## conform (code ← specs)

Update implementation so it matches main spec **behavioral** obligations.

- Treat main spec SHALL/MUST as source of truth for behavior.
- Propose code (and tests when applicable) edits; do not weaken spec obligations to match shortcuts in code.
- When spec and code both seem wrong, flag **needs human decision** in the drift report — do not pick silently.
- **deep** token is not valid with conform — reject before detect per [deep-mode.md](deep-mode.md).

## Comparison scope

|In scope|Out of scope|
|-|-|
|`openspec/specs/<capability>/spec.md` behavioral obligations|Change-folder deltas under `openspec/changes/**`|
|Acceptance-level behavior in scenarios|Presentation tokens, hex colors, Figma deferrals|
|Procedure only when specs state governing behavioral obligations|Wholesale legacy spec rewrite|

Layering matches apply-review: specs = invariants; design/tasks defer detail intentionally omitted from specs.

## Mode selection prompt

When mode is missing, call structured prompt per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) — **forbidden:** prose-only menu when structured tools are present.

**Prompt:** `Choose dedrift mode:`

|id|label|
|-|-|
|`reconcile`|Reconcile — update main specs to match code|
|`conform`|Conform — update code to match main specs|

Plain-text fallback (tools absent only):

```text
Choose dedrift mode:
1. reconcile — update main specs to match code (id: reconcile)
2. conform — update code to match main specs (id: conform)
```

Record choice before detect/preview.
