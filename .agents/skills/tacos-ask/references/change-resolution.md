# Change resolution (tacos-ask)

Resolve the OpenSpec **change id** (slug) before loading artifacts. When no slug resolves, use `_no-change` and enter [no-change-mode](no-change-mode.md).

## Resolution order

1. User names the change — folder basename or unambiguous slug under `openspec/changes/` (not `archive/` unless user names archive folder).
2. Single active change — exactly one non-archive dir under `openspec/changes/` with `proposal.md` → use it.
3. `openspec list` — when available, one in-progress change → use it.
4. Chat context — if the thread clearly references one change id, use it.
5. Ask once — list active candidates; user picks.

Prefer ask-once over branch-diff scoring. Use branch-diff only when the user is mid-apply without naming a change and multiple actives exist.

## tacos-ask specifics

|Field|Rule|
|-|-|
|**Slug**|Basename under `openspec/changes/` (non-archive) with planning artifacts|
|**No change**|`_no-change` — do not write handoff files; switch to no-change mode|
|**After resolve**|Load per [artifact-load](artifact-load.md) when slug ≠ `_no-change`|

When ambiguous after one ask, stop and request clarification before answering.
