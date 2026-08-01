# Workspace and layout install

Two install modes: **single** (git-root OpenSpec) and **workspace** (scoped folders + entry artifacts). Single install writes a default `workspace` block for apply-review scope only — no `.code-workspace` or `.claude/settings.json`.

## Install verb

|Pattern|`/tacos-doctor install`|`schema.cs` scaffold|
|-|-|-|
|Team workspace|`/tacos-doctor install workspace` (+ island id, folders)|`workspace-init --island-id` then `set-schema`|
|Personal workspace|`/tacos-doctor install workspace` (+ `--layout-root`, folders)|`workspace-init --layout-root` then `set-schema`|

## `workspace-init`

Team island — run from the **tacos-doctor** skill root (where `scripts/schema.cs` lives); folder paths in `--folders-json` are relative to that directory:

```bash
dotnet scripts/schema.cs workspace-init \
  --island-id <name> \
  --folders-json '[{"name":"specs","path":"../../specs"}]'
```

Personal workspace (off-repo or in-repo; git optional):

```bash
dotnet scripts/schema.cs workspace-init \
  --layout-root /path/to/workspace \
  --folders-json '[{"name":"work","path":"."}]'
```

Exactly one of `--island-id` or `--layout-root` is required.

Generated `<layout-dir>.code-workspace` always includes the layout root folder (`path: .`, name = layout directory) even when `workspace.folders` omits it — so OpenSpec and entry files at the layout root are in the IDE workspace.

## Team workspace gitignored local files

Doctor appends these lines at `tacos-workspaces/<island>/` `.gitignore`:

```
*/skills/tacos-*
*/agents/agent-tacos-*
*/skills/openspec-*
*/commands/opsx-*
*/commands/opsx/*
*/prompts/opsx-*
*/workflows/opsx-*
skills-lock.json
global.json
openspec/host/*.template
openspec/host/README.md
```

## Optional island `global.json`

Team workspace islands often sit under a product repo whose **git root** has `global.json` pinning an exact SDK. `dotnet run` from the island layout root walks up the tree and applies that pin.

## Skills placement

|Mode|`skill-refresh` cwd|
|-|-|
|single|git root|
|workspace|layout root|

## Single default `workspace`

After `/tacos-doctor install` or update at git root:

```yaml
workspace:
  folders:
    - name: repository
      path: .
```

## Promote personal workspace → team island

Move `openspec/changes/<name>/` to `tacos-workspaces/<island>/openspec/changes/`, run `/tacos-doctor update` at the team island.
