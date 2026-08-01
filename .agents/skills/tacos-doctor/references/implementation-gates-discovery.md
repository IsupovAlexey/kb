# Implementation gates — agent discovery

Command discovery is **agent-mediated**. `schema.cs` only inserts or preserves the managed block shell (`templates/host/AGENTS-implementation-gates-snippet.md`). Do **not** implement discovery in C# doctor scripts.

Run this workflow after `set-schema` / `update` when the implementation-gates block was **inserted** or `## Commands` still has the install placeholder.

## Non-code and docs-only hosts

Many tacos hosts have **no application code** (specs, playbooks, policy, knowledge bases) but still use OpenSpec planning and apply. They **benefit from tacos** without local format/build/test commands.

- When the repo is clearly non-code (no `.sln`, `.csproj`, `package.json`, `go.mod`, `Cargo.toml`, `pom.xml`, or similar; content is mostly markdown/openspec), set `<!-- tacos-doctor-discovery: empty -->`, leave **Commands** empty or a one-line note (e.g. “No local dev gates — prose/planning only”), and **do not infer** commands from layout.
- Empty gates are **valid**: apply and apply-review use tacos skills; hard stops apply only when Commands lists a command.
- Optional: add a short note under **Commands** so future agents know empty is intentional.

## Read order

1. README, CONTRIBUTING, root docs for explicit local dev commands
2. `AGENTS.md` **outside** `<!-- tacos-agents-* -->` and `<!-- tacos-implementation-gates-* -->` regions
3. Makefile / justfile targets named like `fmt`, `lint`, `test`, `build`
4. _(skip)_ Host skill path pointers — do **not** add yet; spec allows later

Only when the above lack a category **and** the host is not non-code/docs-only (see above), infer from repo layout (`.sln`, `.csproj`, `package.json` scripts) using **allowlisted** categories only:

|Category|Examples|
|-|-|
|format|csharpier, prettier, `npm run format`|
|lint|analyzers, eslint, `npm run lint`|
|build|`dotnet build`, `npm run build`|
|test|scoped/fast unit test commands — prefer README over whole-solution defaults|

Do **not** parse CI workflows. Do **not** add arbitrary scripts.

## Write `AGENTS.md`

Inside `<!-- tacos-implementation-gates-begin -->` … `<!-- tacos-implementation-gates-end -->`:

1. Keep bundle MUST prose (hard stops, `artifacts/outputs/`, scoped checks, format → build → test).
2. Replace the `## Commands` placeholder with concise fenced or bullet command lines per category present.
3. Set exactly one metadata line (inside the block, typically after `## Commands`):

   - `<!-- tacos-doctor-discovery: documented -->` — every listed command traced to human docs
   - `<!-- tacos-doctor-discovery: inferred -->` — one or more commands from layout inference only
   - `<!-- tacos-doctor-discovery: empty -->` — no commands; shell only

## Summarize for the user

|Metadata|Message|
|-|-|
|`documented`|OK — gates populated from repo docs|
|`inferred`|WARN — gates used inference; verify or document in README/CONTRIBUTING|
|`empty`|OK — no local dev gates (expected for non-code hosts or when none documented); optional one-line note under **Commands**|

## Optional `openspec/config.yaml`

When Commands are non-empty, you **may** add one line to `tacos-config` context:

`Implementation gates (local dev): AGENTS.md between <!-- tacos-implementation-gates-begin --> and <!-- tacos-implementation-gates-end -->.`

Do not add a check registry to `openspec/tacos.yaml`.

## Update

Same discovery when update **inserts** a missing block. When markers already exist, **do not** overwrite inner body; remind maintainers to verify gates after stack changes.
