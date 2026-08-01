# tacos prerequisite checks

Run from the **tacos-doctor skill root** (`SKILL.md` / `scripts/`). Walks up from cwd to find the host repo (`openspec/`, `AGENTS.md`, or installed host skills roots).

```bash
dotnet scripts/check-prereqs.cs
```

Schema install/diagnose runs only after this script exits `0` (see **Diagnose** in `SKILL.md`). When only OpenSpec setup is missing and `node` plus the detected package manager (`npm` or `pnpm`) are on PATH, exit `2` or `3` signals bootstrap-needed — **Install** and **Update** run bootstrap below (not bare **Diagnose**). When OpenSpec is missing and `node`/package manager are not available, exit `1`.

**Package manager detection:** Scan from git/layout root for any `pnpm-lock.yaml` under the host repo (skipping `.git`, `node_modules`, `artifacts`). When found → require `pnpm` and bootstrap via `pnpm add -g`; otherwise require `npm` and bootstrap via `npm install -g`.

## Exit codes

|Code|Meaning|
|-|-|
|`0`|All required checks passed (warnings allowed)|
|`1`|One or more required checks failed|
|`2`|OpenSpec CLI missing — bootstrap needed (`ensure-openspec.cs --install-cli` or `--install`)|
|`3`|OpenSpec CLI present but `openspec/config.yaml` missing — init needed (`ensure-openspec.cs --init` or `--install`)|

Invalid arguments exit `1`.

## Output sections

**Host tools:** `dotnet`, `openspec` (required — `NEEDS bootstrap` or `NEEDS init` instead of `FAIL` when only OpenSpec setup is missing); `git` (required); `gh` (warn when `slice_pr`, `pr.enabled`, or the `tacos-pr-triage` skill is installed — covers `/tacos-slice-pr`, `/tacos-pr`, and `/tacos-pr-triage`); `jira` (skip until `jira:` in `openspec/tacos.yaml`; when `jira.enabled: true`, **WARN** only if neither Atlassian MCP nor `acli`/`atlassian` CLI is detected).

**tacos skills:** `tacos-orchestration`, `tacos-grill` (required); `tacos-jira-sync` (required when `jira.enabled: true`); `tacos-implementation-conventions`, `tacos-direct-output`, `tacos-spec-review`, `tacos-apply-review`, `tacos-e2e-scenarios`, `tacos-test-plans`, `tacos-split-diff`, `tacos-slice-pr`, `tacos-pr`, `tacos-pr-triage`, `tacos-handoff`, `tacos-lens`, `tacos-host-skill` (SKIP when missing; OK when installed).

## OpenSpec bootstrap

During `/tacos-doctor install` or **Update** (not bare **Diagnose**):

|`check-prereqs.cs` exit|Action|
|-|-|
|`1`|Abort and report failures|
|`2` or `3`|`dotnet scripts/ensure-openspec.cs --install` from this skill root (script resolves host repo root). On non-zero exit, report stderr and stop. Re-run `check-prereqs.cs`; abort if not exit `0`.|
|`0` on **Install**|`dotnet scripts/ensure-openspec.cs --sync-host` before schema steps (restores OpenSpec host skills/commands after workspace scaffold or clone)|
|`0` on **Update**|Defer `--sync-host` until after skills refresh and [re-load from disk](update.md#phase-b--re-load-gate-mandatory) ([update.md](update.md) Phase B)|

Partial `openspec/` trees without `openspec/config.yaml` still require init (`--install` covers CLI + init). Re-run `check-prereqs.cs` after bootstrap; expect exit `0` before `set-schema`, `update`, or `schema.cs diagnose` on install/update paths.

## When .NET SDK is missing

Run `dotnet --version` in shell first (see SKILL **Diagnose** step 1); do not invoke this script without `dotnet`.

## Tracked-workspace island SDK pin mismatch

When `dotnet run` from a workspace team island layout root fails because a **parent** `global.json` pins an SDK this machine lacks (`Requested SDK version` in stderr), create a local `global.json` at the island layout root per [workspace-install.md](workspace-install.md) **`global.json` when parent SDK pin mismatches**. Do not edit the product repo git-root pin for tacos-only work.

## openspec on Windows

npm or pnpm global installs expose `openspec` as `openspec.cmd`. The checker resolves `PATHEXT` (`.CMD`, `.EXE`, …) because `Process.Start` with `UseShellExecute = false` does not apply the shell’s PATH lookup. Version is read with `openspec --version` (OpenSpec 1.3+ removed the `version` subcommand). After global package-manager install, `ensure-openspec.cs` re-probes PATH with the same PATHEXT logic.

## Jira transport

When `jira.enabled: true`, the checker prints **two lines**:

|Line|Meaning|
|-|-|
|`jira MCP:`|OK when Atlassian MCP is detected; SKIP when not|
|`jira CLI:`|OK when `acli` or `atlassian` is on PATH; SKIP when not|

MCP detection (best-effort): Cursor project cache `~/.cursor/projects/<workspace-slug>/mcps/plugin-atlassian-atlassian/`, repo or `~/.cursor/mcp.json`, or installed Cursor **Atlassian** plugin under `~/.cursor/plugins/cache/cursor-public/atlassian/`.

**WARN** `jira: enabled but no transport` only when **both** lines are SKIP. MCP OK + CLI SKIP is healthy (MCP-first per tacos-jira-sync).
