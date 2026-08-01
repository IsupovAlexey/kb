# tacos-work archive mode

Load from `tacos-work/SKILL.md` Entry when invoke is `/tacos-work archive` or natural-language archive intent with no active phase-0–5 session in progress.

## MUST read

- [archive-script.md](../archive-script.md)
- [archive-session-template.md](../archive-session-template.md)
- [slug-resolution.md](../slug-resolution.md) ## Archive mode
- [output-paths.md](../output-paths.md)
- [../../../tacos-orchestration/references/structured-gate-convention.md](../../../tacos-orchestration/references/structured-gate-convention.md)

## Forbidden

- MUST NOT run `openspec validate`
- MUST NOT paraphrase or hand-distill `tasks.md` when `scripts/archive-session.cs` is available — use script output only
- MUST NOT create active `openspec/changes/<name>/` folders (non-archive)
- MUST NOT write `.openspec.yaml`, delta specs, or faux proposal bundles under the archive folder
- MUST NOT commit unless the user explicitly asks
- MUST NOT write under `openspec/changes/archive/` when source `artifacts/tacos-work/<slug>/tasks.md` is missing

## During mode

|Step|Action|
|-|-|
|A.1|`tacos-work [<slug>]: archive — resolving session`|
|A.2|Resolve `<slug>` per [slug-resolution.md](../slug-resolution.md) ## Archive mode|
|A.3|From tacos-work skill root, run `dotnet run scripts/archive-session.cs --repo <repo-root> --slug <slug> --preview --format json`; if script missing, report install error and stop|
|A.4|Parse JSON `targetPath` and `sessionMarkdown` for preview gate|
|A.5|Structured preview gate — **next tool call MUST** be `AskQuestion` / `AskUserQuestion` when available: show target path + `sessionMarkdown`; options `approve` / `decline`|
|A.6|On **approve**: run `--write` with same `--repo`, `--slug`, and optional `--date`|
|A.7|On **decline**: no git write; report declined|
|A.8|`tacos-work [<slug>]: archived → <path>` or `tacos-work [<slug>]: archive declined`|

## Done when

- User chose approve or decline after preview
- On approve: `openspec/changes/archive/<date>-<slug>/session.md` exists per template
- No `openspec validate` ran during this mode
