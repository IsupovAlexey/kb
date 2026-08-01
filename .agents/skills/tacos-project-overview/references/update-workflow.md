# Update workflow

Update `project_overview.path` — in-scope edits only; leave the rest of the file unchanged. Preview before write; apply only after approval.

## Gating

|`project_overview.enabled`|sync/archive orchestration|manual `/tacos-project-overview`|
|-|-|-|
|`true`|When matching `prompt_after_*` flag is true|Yes|
|`false`|Off|Yes (note sync/archive orchestration inactive)|

Defaults when block missing: `enabled: false`; `path` unset → `README.md`; both prompt flags `false`.

## Procedure

1. **Resolve trigger** — `sync` | `archive` | `manual` (manual: user-listed add / update / remove; ask once if scope is vague — skill **Manual scope** table).
2. **Resolve path** — `openspec/tacos.yaml` → `project_overview.path`; if unset, `README.md` at repo root. Do not assume `README.md` when config points elsewhere. **STOP** if the file is missing.
3. **Read overview** — full file at the resolved path.
4. **Scope**
   - **sync** / **archive:** Gates 1–2 in [sync-archive-prompts.md](sync-archive-prompts.md) — opt in, then verify/modify the scope plan before draft.
   - **manual:** user-listed add / update / remove; ask once if missing. Gate 2 when scope is vague.
5. **Draft** — per [overview-guidance.md](overview-guidance.md) using the **approved scope plan** only; prefer no edit when the plan says no change.
6. **Preview** — diff or explicit “no overview change needed”.
7. **Gate 3** — [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#project-overview); **pause** before write unless structured **approve**.
8. **Write** (after approval) — apply draft at the resolved path only, or skip if preview was no change.
9. **Report** — resolved path, trigger, sections touched (or none).

## Orchestration (sync / archive)

When `enabled` and the matching `prompt_after_*` flag are true, orchestration **MUST** run this workflow in the **same turn** after stock **sync** or **archive** — Gates 1–3 in order. See [../../tacos-orchestration/references/project-overview-hooks.md](../../tacos-orchestration/references/project-overview-hooks.md).
