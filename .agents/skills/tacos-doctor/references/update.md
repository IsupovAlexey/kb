# tacos update (distribution refresh)

Run from the **tacos-doctor skill root** (`SKILL.md` / `scripts/`). After skills refresh, **re-read this file from disk** — see [Phase B — re-load gate](#phase-b--re-load-gate-mandatory).

## Two-phase install (first time)

1. **Skills** — from the **host project root** (plain git repo or existing OpenSpec project; project-local trees per [OpenSpec supported tools](https://github.com/Fission-AI/OpenSpec/blob/main/docs/supported-tools.md)):
   - **Cursor:** `npx skills add servicetitan/tacos --agent cursor -y`
   - **Claude Code:** `npx skills add servicetitan/tacos --agent claude-code -y`
   - Installs under the host’s project skills root; `schema.cs diagnose` reports the detected prefix.
2. **Schema** — `/tacos-doctor install` ([install.md](install.md); bootstrap per [check-prereqs.md — OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap) when needed) → `dotnet scripts/schema.cs set-schema` → `openspec schema validate tacos`

Doctor does **not** run `npx skills` on first install.

## Update flow (`/tacos-doctor update`)

### Phase A — before skills refresh

Steps safe with the tacos-doctor version loaded at chat start:

1. `dotnet scripts/check-prereqs.cs` — bootstrap only when exit `2` or `3`: [check-prereqs.md — OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap). Do **not** run `ensure-openspec --sync-host` yet.
2. Optional: `dotnet scripts/schema.cs diagnose`
3. **Refresh every detected project skills root** (when the agent can run host CLIs; otherwise give the user the plan):
   - `dotnet scripts/schema.cs skill-refresh` — lists one `npx skills add` per install target where `tacos-orchestration/SKILL.md` exists (see **Skills refresh mapping**). Each `RUN` line shows `cwd=<OpenSpec project root>` — **that** is the working directory; the path in `# updates …` is where files land, not where to `cd` first.
   - When `skill-refresh` exits `1` for nested installs, delete the listed trees (including `skills-lock.json` there) and re-run diagnose before retrying.
   - When `schema.cs diagnose` WARNs about redundant Cursor copies, treat the duplicate tree as stale; refresh only the canonical root from **skill-refresh** output.
   - When diagnose WARNs about **accidental nested** installs (e.g. `.agents/skills/.agents/skills/`), delete the nested tree before any `npx skills add`.
   - Run each `RUN` `npx` command with cwd at the OpenSpec project root (use `--distribution-source /path/to/tacos` for unpublished checkouts).
   - When **skill-refresh** lists more than one install target, you MUST run every `RUN` command so copies stay in sync.

### Phase B — re-load gate (mandatory)

This chat loaded tacos-doctor at session start. **Skills refresh overwrote skill files on disk** — including `tacos-doctor` scripts and this procedure. Session memory of Phase A is stale for everything below.

Before post-refresh steps, **read from disk** (MUST NOT continue from cached procedure):

1. `<skills-prefix>/tacos-doctor/SKILL.md`
2. This file — **Phase B steps** below
3. `<skills-prefix>/tacos-doctor/references/check-prereqs.md` — [OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap) (`--sync-host` on exit `0`)

Resolve `<skills-prefix>` from `dotnet scripts/schema.cs diagnose` (or host layout). Run every `dotnet scripts/…` below from the **refreshed** `<skills-prefix>/tacos-doctor/` skill root.

### Phase B steps (after re-load)

4. `dotnet scripts/ensure-openspec.cs --sync-host` — [check-prereqs.md — OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap)
5. `dotnet scripts/lens-refresh.cs` — clone or refresh shared tacos-lens at `<common-folder>/tacos-lens` (clone when missing or empty; pull when present). `<common-folder>` is `%USERPROFILE%\ServiceTitan` on Windows, `~/ServiceTitan` on Unix-like systems unless overridden by `TACOS_COMMON_FOLDER`. See [lens-refresh.cs](../scripts/lens-refresh.cs).
6. `dotnet scripts/schema.cs update` — see [schema.md — Commands](schema.md#commands) (`update` row) and [schema.md — Flags](schema.md#flags).
7. When update **inserted** a missing implementation-gates block, run **Implementation gates discovery (agent)** per [implementation-gates-discovery.md](implementation-gates-discovery.md) (same as [install.md](install.md) step 4).
8. **Review skills discovery (agent)** — when one or both review arrays are absent or `[]`, follow [review-skills-discovery.md](review-skills-discovery.md) (populate only empty arrays; preserve non-empty); when both arrays are non-empty, skip yaml writes and WARN only.
9. When you changed `*_models` or the bundle added host agent templates, run `/tacos-doctor config`.
10. `openspec schema validate tacos`

### Skills refresh mapping

|Project path|`--agent` for `npx skills add`|
|-|-|
|`.agents/skills/`|`cursor` (canonical for Cursor; use `claude-code` when that host owns this tree)|
|`.cursor/skills/`|`cursor` (only when `.agents/skills/` has no tacos install)|
|`.claude/skills/`|`claude-code`|
|Other `<tool>/skills/`|`<tool>` (best-effort; confirm with [npx skills](https://www.npmjs.com/package/skills) / host docs)|

`<source>` defaults to `servicetitan/tacos`; override with `schema.cs skill-refresh --distribution-source …`.

**Record-and-latest:** `openspec/tacos.yaml` `version` records what was applied. Default update target is the bundle template version. Override with `--target-version <semver>` when pinning (see [schema.md — Flags](schema.md#flags)).

**`--force` on update:** Overwrites `openspec/tacos.yaml` and refreshes managed `config.yaml` / `tacos-agents` `AGENTS.md` blocks — does **not** refresh `tacos-implementation-gates` inner body (same as `set-schema --force`). Use only after preview and approval. Schema tree is always replaced on `update` (bundle-owned). Only `openspec/config.yaml` is backed up when it would change.

Backup root: `artifacts/tacos-backup/` (ignored via repo `artifacts/` gitignore).

## Done when

- Phase A complete: every `skill-refresh` `RUN` command executed; nested-install WARNs cleared first.
- Phase B re-load gate: refreshed `SKILL.md`, this file (Phase B), and `check-prereqs.md` read from disk before post-refresh steps.
- `ensure-openspec --sync-host` run after re-load (not in Phase A).
- `dotnet scripts/lens-refresh.cs` executed (clone when missing or empty; pull when present).
- `schema.cs update` applied; `openspec/tacos.yaml` `version` matches bundle target.
- `openspec schema validate tacos` exit `0`.
- **Config** run when `*_models` or bundle agent templates changed ([config.md](config.md)).
- Implementation gates and review skills discovery completed when update inserted missing blocks or empty review arrays.
