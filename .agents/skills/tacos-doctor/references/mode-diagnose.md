# Mode: diagnose

Load when `/tacos-doctor` (no args) or user asks to diagnose prerequisites/schema.

Run from this skill root (`SKILL.md` / `scripts/`).

## Flow

1. If unsure `dotnet` is available, run `dotnet --version`. On failure: report [.NET SDK install](https://dotnet.microsoft.com/download) and stop.
2. `dotnet scripts/check-prereqs.cs` — [check-prereqs.md](check-prereqs.md)
3. If step 2 exit `0`: `dotnet scripts/schema.cs diagnose` — [schema.md](schema.md)
4. Summarize both sections using exact `check-prereqs.cs` / `schema.cs diagnose` lines (OK/FAIL/WARN/SKIP). For Jira: `jira MCP: OK` satisfies transport — do not WARN because `jira CLI` is SKIP. For stale hardcoded skills prefix on host subagents: `config` only substitutes `{{SKILLS_PREFIX}}` — repair per [config.md](config.md).

If step 2 exit `1`, skip step 3 and report what failed.

## Exit codes

|Step|Code|Action|
|-|-|-|
|`check-prereqs.cs`|`0`|Continue to `schema.cs diagnose`|
|`check-prereqs.cs`|`1`|Stop; report failures; skip schema diagnose|
|`check-prereqs.cs`|`2` or `3`|Bootstrap only on **Install** or **Update** — not bare diagnose — [check-prereqs.md — OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap)|

Full exit-code table: [check-prereqs.md — Exit codes](check-prereqs.md#exit-codes).

## Done when

- Chat reports Host tools and Schema (or tacos skills) using exact checker/diagnose output lines.
- FAIL/NEEDS items include the next command (`install`, `update`, or fix a host tool).
- Step 3 skipped when `check-prereqs.cs` exits `1`.

## Example summary

```text
--- Host tools ---
OK   dotnet: 10.0.100
FAIL openspec: not on PATH
--- Tacos skills ---
OK   tacos-orchestration: …
SKIP tacos-jira-sync: jira.enabled is false
SKIP tacos-lens: not installed
OK  openspec/schemas/tacos has schema.yaml
WARN openspec/tacos.yaml version … is behind installed bundle … — run /tacos-doctor update
```
