# schema.cs

```bash
dotnet scripts/schema.cs [diagnose|set-schema|update|config|skill-refresh|workspace-init|merge-review-skills] [options]
```

|Command|Purpose|
|-|-|
|`diagnose`|Schema, layout mode, `AGENTS.md`, `artifacts/` gitignore|
|`set-schema`|Install tacos schema, config hook, managed `AGENTS.md` blocks|
|`update`|Replace `openspec/schemas/tacos/`; merge `openspec/tacos.yaml`|
|`config`|Install/sync host subagent `model:` from `*_models`|
|`skill-refresh`|Print `npx skills add` commands; cwd is layout root for workspace mode, git root for single|
|`workspace-init`|Scaffold workspace layout (`--island-id` for team OR `--layout-root` for personal, plus `--folders-json`)|

See [workspace-install.md](workspace-install.md) for layout modes. For `update` flags and backups see [update.md](update.md). For host subagent model sync see [config.md](config.md).
