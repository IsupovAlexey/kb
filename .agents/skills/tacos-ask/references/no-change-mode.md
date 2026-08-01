# No-change mode

Enter when change resolution yields `_no-change` (no slug binds).

## Sources

Answer from, as needed:

|Source|Use|
|-|-|
|`openspec/specs/**`|Main capability requirements|
|Host onboarding docs|`README.md`, `AGENTS.md` when present|
|Codebase|Implementation detail when specs are insufficient|

Cite paths + short quotes the same as bound-change answers.

## Tracked change needed

When the user's need implies a git-tracked change folder (`openspec/changes/<id>/`), offer full tacos planning — `/opsx-propose` for a new change, or `/opsx-continue` / `/opsx-apply` when an active change already exists — rather than inventing change artifacts.

## Small edits

When the user requests a small edit with no bound change, assess per [small-edits](small-edits.md) — typically `openspec/specs/**`, README, or docs. Confirm before write. Route larger work per [escape-hatches](escape-hatches.md).
