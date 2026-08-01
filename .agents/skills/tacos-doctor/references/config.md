# tacos config (host subagents)

Apply `openspec/tacos.yaml` **`*_models`** to installed host subagent files. Does **not** bump bundle `version` in `openspec/tacos.yaml`, refresh distribution skills, or replace `openspec/schemas/tacos/` — use `/tacos-doctor update` for that.

## When to run

|Situation|Command|
|-|-|
|Changed `*_models` in `openspec/tacos.yaml`|`/tacos-doctor config`|
|After `/tacos-doctor update` when the bundle added new `agent-tacos-*.md` templates|`/tacos-doctor config`|
|First-time tacos install|`/tacos-doctor install` (includes host agents via `set-schema`)|

## Flow (`/tacos-doctor config`)

1. Optional: `dotnet scripts/schema.cs diagnose` — host subagent drift WARNs.
2. `dotnet scripts/schema.cs config` — missing agents copied from bundle; existing agents get frontmatter `model:` synced from yaml (body preserved). New copies expand `{{SKILLS_PREFIX}}` to the detected tacos skills install root (same probe as host `AGENTS.md` / `openspec/config.yaml`). Existing agents that still contain `{{SKILLS_PREFIX}}` get the same substitution on `config` without replacing the rest of the body.

   `config` does **not** rewrite literal hardcoded host skills paths in existing agent bodies (for example `.agents/skills/` when the install root is `.cursor/skills/`). For those diagnose WARNs, edit paths manually or delete the agent file and run `/tacos-doctor config` to copy a fresh template with expanded `{{SKILLS_PREFIX}}`.

Requires a host with subagent support detected by `schema.cs diagnose`.

## Done when

- `schema.cs config` exit `0`; diagnose shows host subagents OK or only expected WARNs (hardcoded path repair is manual per above).
- Does not change `openspec/tacos.yaml` `version` or refresh distribution skills — use `/tacos-doctor update` for that.

## `*_models` and valid `model:` values

Each delegated step uses a `*_models` map with **`cursor`** and **`claude`** keys (for example `review.spec_review_models`). Values are written into installed host subagent frontmatter when tacos-doctor installs templates.

|Host|Where to find valid `model` values|
|-|-|
|**Cursor**|[Subagents — model configuration](https://cursor.com/docs/subagents#model-configuration); model picker in chat; often `inherit`, `fast`, or a host-specific slug|
|**Claude Code**|[Subagents — choose a model](https://code.claude.com/docs/en/sub-agents#choose-a-model); aliases `sonnet`, `opus`, `haiku`, or full ids such as `claude-sonnet-4-6`|

Editors that load Claude workspace agents can use the `claude` key only. After you change `*_models`, run `/tacos-doctor config` so installed `agent-tacos-*.md` host subagents get an updated `model:` line (paths shown by `schema.cs diagnose`).
