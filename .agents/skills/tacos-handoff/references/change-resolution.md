# Change resolution (handoff slug)

Resolve the OpenSpec **change id** (slug) before building **Pointers**. Output path uses this slug or `_no-change`.

## Change id

|Field|Rule|
|-|-|
|**Slug**|Basename of folder under `openspec/changes/` (non-archive) with planning artifacts, e.g. `add-<feature>`|
|**No change**|`_no-change` when no slug resolves|
|**Output**|`artifacts/session-handoff/<slug>/<timestamp>-handoff.md` per [output-paths.md](output-paths.md)|

## Resolution order

1. **User names the change** — folder basename or unambiguous slug under `openspec/changes/` (not `archive/` unless user names archive folder).
2. **Single active change** — exactly one non-archive dir under `openspec/changes/` with `proposal.md` → use it.
3. **`openspec list`** — when available, one in-progress change → use it.
4. **Chat context** — if the thread clearly references one change id, use it.
5. **Ask once** — list active candidates; user picks.

Prefer ask-once over branch-diff scoring. Use branch-diff only when the user is mid-apply without naming a change and multiple actives exist.

## After resolve

- Record slug in handoff **Meta**.
- Build pointers from `openspec/changes/<slug>/` and `artifacts/openspec-reviews/<slug>/` when slug ≠ `_no-change`.
- When ambiguous after one ask, stop and request clarification before writing.
