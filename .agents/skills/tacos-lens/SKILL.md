---
name: tacos-lens
description: >-
  Launch the local tacos-lens viewer or CLI against a tacos-powered repository.
  Browse changes and specs, validate planning, export handoff/review packets, and
  operate limited repo config. Ships with tacos skills install; implementation is a
  shared checkout under the user common folder or the open tacos-lens dev workspace.
  Invoke via /tacos-lens.
user-invocable: true
disable-model-invocation: true
---

# tacos-lens

Local viewer and CLI for tacos planning repos. The skill ships with **tacos** (`npx skills add servicetitan/tacos`); the **tacos-lens** application repo is cloned separately (not a second marketplace plugin).

**When to use**

- Review change progress, artifacts, staged tasks, and apply gates in the browser
- Hand off to a fresh agent — Handoff tab or `export-handoff --brief`
- Validate specs — Spec Validation UI or `check`
- Export review context — `export-review-packet` or Handoff download
- Edit `openspec/tacos.yaml` with backup/restore in Repo config

Full capability list and contributor setup: [tacos-lens README](https://github.com/servicetitan/tacos-lens).

## Prerequisites

- .NET 10 SDK
- `openspec` CLI on PATH
- `git` on PATH (for first-time clone bootstrap)
- Target repository with an `openspec/` directory

## Resolve implementation root (call order)

Before `dotnet run`, resolve `<tacos-lens-install>`:

1. **Workspace-first (dev)** — walk up from cwd for `TacosLens.slnx`. When the IDE exposes workspace folders, check each root the same way. First match wins.
2. **Shared clone** — `<common-folder>/tacos-lens` where `<common-folder>` is:
   - `TACOS_COMMON_FOLDER` when set (expanded user path), else
   - `%USERPROFILE%\ServiceTitan` on Windows, else
   - `~/ServiceTitan` on Unix-like systems (same convention as consolidated-setup).
3. **Bootstrap when missing** — when step 1 did not match and step 2 path is absent, empty, or not a tacos-lens root, run lens-refresh then use step 2:

```bash
# from tacos-doctor skill root (…/tacos-doctor/)
dotnet scripts/lens-refresh.cs
```

Or clone manually:

```bash
git clone --branch master https://github.com/servicetitan/tacos-lens.git <common-folder>/tacos-lens
```

`/tacos-doctor update` clones or refreshes the shared checkout (`dotnet scripts/lens-refresh.cs` from tacos-doctor skill root).

## CLI

All commands run from `<tacos-lens-install>` via `TacosLens.Cli` (or published DLL). Default viewer URL after `serve`: `http://127.0.0.1:5726`.

|Command|When to use|
|-|-|
|`serve --repo <path> [--port <n>]`|Start the local viewer (default port 5726)|
|`check --repo <path>`|Index the repo and run `openspec validate --all --strict --no-interactive`|
|`export-comments --repo <path> [-o file.md]`|Export saved viewer text-selection comments as markdown|
|`export-handoff --repo <path> --change <id> [--brief] [-o file.md]`|Export live handoff context (`--brief` = pointers, tasks, gates, linked test-plan folder paths; no full planning or test-cases bodies)|
|`export-review-packet --repo <path> --change <id> [--no-bodies] [-o file.md]`|Assemble proposal, design, tasks, delta specs, apply-review, and comments (`--no-bodies` adds linked test-plan folder paths without inline bodies)|

### Launch viewer (`serve`)

From the target OpenSpec repository root, start in watch mode (`launchSettings.json` opens the browser):

```bash
dotnet watch run --launch-profile serve --project <tacos-lens-install>/src/TacosLens.Cli
```

When `--repo` is not the current directory:

```bash
dotnet watch run --launch-profile serve --project <tacos-lens-install>/src/TacosLens.Cli -- serve --repo <path>
```

One-shot (no hot reload):

```bash
dotnet run --launch-profile serve --project <tacos-lens-install>/src/TacosLens.Cli
```

After build, you can also run the published CLI:

```bash
dotnet <tacos-lens-install>/artifacts/bin/TacosLens.Cli/Debug/net10.0/TacosLens.Cli.dll serve --repo .
```

Alternative host entry (same viewer):

```bash
dotnet watch run --launch-profile http --project <tacos-lens-install>/src/Startup.Web
```

## Viewer surfaces

- **Overview** — in-progress changes with artifact/task meters, grill summary, staged task checklists, openspec/ and artifacts/ browse trees
- **Changes and archive** — planning artifacts, bound review outputs, Handoff tab
- **Handoff** — live index context, linked test-plan links when a change references `openspec/test-plans/<slug>/`, latest `session-handoff` link, copy-brief textarea, download review packet
- **Capabilities** — main spec browsing; search with FTS snippets; pin/unpin per repo
- **Spec Validation** — `openspec validate` results plus deep planning-consistency checks
- **Focus graph and spec diff** — relationship and delta surfaces per change
- **Repo config** — edit `openspec/tacos.yaml` (validated save, pre-save backup, restore); preview `config.yaml`; browse schema, host overlays, skills, backups
- **Agent outputs** nav — Handoffs (`session-handoff/`), Assisted review, Work sessions (`tacos-work/`), Test plans (`/test-plans`)
- **PR triages** — suggested change link when branch slug uniquely matches a change id
- **Recents switcher** — switch among up to 10 recently indexed repos (sidebar, when multiple repos recorded)

Fresh-agent recipe: open change → Handoff → Copy brief (or `export-handoff --brief`) → paste into new session; use `export-review-packet` for review handoffs.

## Errors

|Situation|Response|
|-|-|
|No `openspec/` in repo path|Report that the path is not a tacos-powered project; do not claim success|
|`openspec` missing on PATH|Document install from OpenSpec docs; suggest `openspec --version`|
|Shared clone path exists but is not tacos-lens|Report path conflict; do not overwrite — ask user to fix or set `TACOS_COMMON_FOLDER`|
|Clone or pull fails|Show git stderr; include clone URL and target path|
|Build fails|Show `dotnet build` output from tacos-lens solution root|
|Port already in use|Report that tacos-lens is already running, print the existing `http://127.0.0.1:<port>` URL, and exit successfully — do not start a duplicate server|
|Viewer unreachable|PWA shell may load offline, but repo content requires `serve` running|

## Write boundaries

Planning markdown under `openspec/changes/` and `openspec/specs/` stays agent-owned — lens does not edit those files. In-lens writes are limited to validated `openspec/tacos.yaml` save (with pre-save backup) and confirmed backup restore. Schema templates, `openspec/config.yaml`, host overlays, grill prompt bundles, and installed skills are browse-only in the viewer — use agents or `/tacos-doctor` for edits.

Other operator surfaces: text-selection comments, pins, and CLI exports. Local state persists in `~/.tacos-lens/lens.db`.
