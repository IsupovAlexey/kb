# OpenSpec command ids

tacos uses **canonical command ids** as the only vocabulary in binding docs (`AGENTS.md`, `openspec/config.yaml`, orchestration, grill). Hosts expose different slash syntax; **normalize the user's invocation to a canonical id**, then apply rules. This file is the **abbreviation glossary** for workflow command ids and host alias normalization.

## Canonical ids

|Id|Purpose|
|-|-|
|`propose`|Create change + planning artifacts in one step|
|`new`|Scaffold a new change|
|`continue`|Create next artifact by dependencies|
|`ff`|Fast-forward all planning artifacts|
|`explore`|Think without implementing|
|`apply`|Implement from tasks|
|`verify`|Check implementation vs artifacts|
|`update`|Revise existing planning artifacts in place (no code edits)|
|`sync`|Merge delta specs to main specs|
|`archive`|Archive completed change|
|`bulk-archive`|Archive multiple changes|
|`onboard`|Guided workflow tutorial|

## Normalization

Map any host invocation to one canonical id **before** orchestration or grill rules:

1. Strip a leading `/`.
2. Strip an `opsx:` or `opsx-` prefix (and legacy `openspec:` / `openspec-` if present).
3. The remainder is the canonical id (e.g. `propose`, `ff`, `bulk-archive`).

|Example input|Canonical id|
|-|-|
|`/opsx-propose`, `opsx-propose`|`propose`|
|`/opsx:propose`|`propose`|
|Command file `id: opsx-apply`|`apply`|
|`.github/prompts/opsx-ff.prompt.md` (Copilot IDE)|`ff`|
|`/opsx-update`, `/opsx:update`|`update`|

Rule of thumb: if it ends with the same slug as the id column above, normalize to that id.

## Host aliases (reference only)

Do **not** copy into `AGENTS.md` or binding sections.

|Alias style|Typical hosts|Example|
|-|-|-|
|Colon|Claude Code|`/opsx:propose`|
|Hyphen|Cursor, Windsurf, GitHub Copilot (IDE), many others|`/opsx-propose`|

Some hosts use different prefixes (e.g. Trae `/openspec-propose`); still normalize to the same canonical id when the slug matches.

Authoritative per-host list: [OpenSpec commands.md](https://github.com/Fission-AI/OpenSpec/blob/main/docs/commands.md).

## Binding rule

|Use canonical ids|Do not use in binding prose|
|-|-|
|`propose`, `explore`, `apply`, …|`/opsx-propose`, `/opsx:explore`, `opsx-ff`|
|`tacos-grill` skill paths|Host-specific slash forms for **opsx** commands|

When prose says "run **apply**", the agent uses whichever slash form the host exposes; orchestration tables and grill gates always refer to the **canonical id** `apply`.
