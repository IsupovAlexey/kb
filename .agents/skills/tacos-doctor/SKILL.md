---
name: tacos-doctor
description: >-
  Diagnose tacos OpenSpec prerequisites; install or update the tacos schema; sync
  host subagent models from openspec/tacos.yaml. Invoke via /tacos-doctor.
user-invocable: true
disable-model-invocation: true
---

# tacos doctor

Check host prerequisites and set up the `tacos` OpenSpec schema in the host repo.

Run from this skill root (`SKILL.md` / `scripts/`).

## Entry

1. Resolve mode from user message:

|Mode|Trigger|
|-|-|
|diagnose|`/tacos-doctor` (no args) or diagnose intent|
|install|`/tacos-doctor install`, install request, or workspace install|
|update-config|`/tacos-doctor update`, `/tacos-doctor config`, update or config request|

2. Load one mode bundle (MUST gates and exit codes ≤1 hop):

|Mode|Bundle|
|-|-|
|diagnose|[mode-diagnose.md](references/mode-diagnose.md)|
|install|[mode-install.md](references/mode-install.md)|
|update-config|[mode-update-config.md](references/mode-update-config.md)|

**Update:** Phase A may use this session’s loaded skill. After skills refresh, MUST re-read tacos-doctor from disk before Phase B — [update.md — Phase B re-load gate](references/update.md#phase-b--re-load-gate-mandatory).

3. Follow bundle flow and Done when. Prerequisites: [.NET SDK](https://dotnet.microsoft.com/download), [Node.js](https://nodejs.org/) 18+, git, optional [OpenSpec](https://github.com/Fission-AI/OpenSpec) — [check-prereqs.md](references/check-prereqs.md).

## Invocation quick map

|User says|Mode|
|-|-|
|`/tacos-doctor`|diagnose|
|`/tacos-doctor install`|install|
|`/tacos-doctor update`|update-config → [update.md](references/update.md)|
|`/tacos-doctor config`|update-config → [config.md](references/config.md)|

Shared refs: [schema.md](references/schema.md) · [workspace-install.md](references/workspace-install.md)
