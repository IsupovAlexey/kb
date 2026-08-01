# tacos install

Run from the **tacos-doctor skill root** (`SKILL.md` / `scripts/`). Two-phase: **skills** via `npx skills add` first (see [update.md — Two-phase install](update.md#two-phase-install-first-time)), then **doctor schema** below.

## Layout mode

|Request|Flow|
|-|-|
|Default|**Single install** below|
|`install workspace` + island id or layout path, folders|[workspace-install.md](workspace-install.md) → skills at layout root → **Single install** at layout root|

## Single install

1. Run **Diagnose** step 1 in `SKILL.md` (`dotnet --version` when unsure).
2. `dotnet scripts/check-prereqs.cs` — host tools and tacos skills. Bootstrap and sync-host: [check-prereqs.md — OpenSpec bootstrap](check-prereqs.md#openspec-bootstrap).
3. `dotnet scripts/schema.cs set-schema` — config hook, `AGENTS.md` tacos-agents managed block, **implementation-gates** block (when absent: insert shell after agents block; when present: preserve body; `--force` does not refresh gates), `tacos.yaml` sync, default single `workspace` at git root, `schemas/tacos/` copy.
4. **Implementation gates discovery (agent)** — when step 3 inserted the gates block or `## Commands` is still the placeholder, follow [implementation-gates-discovery.md](implementation-gates-discovery.md): read repo docs, write Commands when applicable, set `<!-- tacos-doctor-discovery: documented|inferred|empty -->` (`empty` is OK for non-code hosts), WARN on inferred only, optional `config.yaml` pointer when non-empty. **No** discovery in `schema.cs`.
5. **Review skills discovery (agent)** — follow [review-skills-discovery.md](review-skills-discovery.md): discover qualifying host review skills, populate empty `review.spec_review_additional_skills` and `review.apply_review_additional_skills` only, suggest `/tacos-host-skill` when gaps remain. **No** discovery in `schema.cs`.
6. `openspec schema validate tacos`

## Done when

- `check-prereqs.cs` exit `0`; OpenSpec bootstrapped when needed per [check-prereqs.md](check-prereqs.md).
- `set-schema` applied; `openspec schema validate tacos` exit `0`.
- Implementation gates and review skills discovery completed when step 3 or yaml state triggers them above.
