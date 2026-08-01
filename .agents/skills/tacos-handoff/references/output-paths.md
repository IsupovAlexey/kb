# Handoff output paths

Generated output under repository-root `artifacts/session-handoff/` (gitignored).

```text
artifacts/session-handoff/<slug>/<timestamp>-handoff.md
```

|Field|Rule|
|-|-|
|**slug**|Active OpenSpec change id (folder basename under `openspec/changes/`, non-archive), or `_no-change` when none resolved|
|**timestamp**|ISO 8601 **local** time, compact for filenames: `YYYY-MM-DDTHHMMSS` (no colons in time segment), suffix `-handoff.md`|
|**Example**|`artifacts/session-handoff/add-<feature>/2026-06-02T143052-handoff.md`|

Create parent directories as needed. One new file per invocation. Reviews under `artifacts/openspec-reviews/<slug>/` are pointer targets only.

Commit handoff files only when the host project opts in.
