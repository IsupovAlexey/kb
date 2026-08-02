<!-- tacos-agents-begin -->

## OpenSpec (tacos)

When `openspec/tacos.yaml` has `orchestration.enabled: true`, OpenSpec workflow commands are governed by tacos skills, not only stock host `opsx` command/prompt files.

Artifact removal (any turn): When editing `openspec/changes/**` (including outside `/opsx:*`) and the user asks to remove content, delete it outright—no tombstones, “removed”/“N/A”/“not done” placeholders, or strikethrough. Read `.agents/skills/tacos-orchestration/references/artifact-editing.md` before the edit; do not run full orchestration entry for ad-hoc edits.

Normative hub: `.agents/skills/tacos-orchestration/references/orchestration-binding.md`

Command read graphs (load per command id before stock opsx steps — see `tacos-orchestration/SKILL.md` Entry):

- explore — `references/read-graphs/explore.md`
- propose, ff, continue — `references/read-graphs/propose.md`
- update — `references/read-graphs/update.md`
- apply — `references/read-graphs/apply.md`
- sync — `references/read-graphs/sync.md`
- verify — `references/read-graphs/verify.md`
- archive — `references/read-graphs/archive.md`
- `/tacos-work` — `references/read-graphs/tacos-work.md`

Stock override binding (mandatory): When `orchestration.enabled` is true: stock OpenSpec CLI and host opsx steps are incomplete for tacos. MUST NOT follow stock opsx when it conflicts with tacos-orchestration or tacos-grill. MUST NOT use stock "prefer reasonable decisions" instead of tacos-grill when `orchestration.grill_enabled` is true. MUST load `orchestration-binding.md` § Stock overrides before inferring workflow from opsx alone. Canonical text: `references/stock-override-binding.md`.

Six override invariants (compact — full matrix on demand via hub): planning grill sequence; explore ≠ planning grill; POST-ARTIFACT when apply-ready; MUST-delegate subagent matrix; OpenSpec validate hard stops on sync/archive/verify; stock-override rules (explore: mini-table + link; propose/apply: full matrix on demand).

Read order (same turn, only before stock opsx workflow steps): `openspec/config.yaml` (`<!-- tacos-config-begin -->` kernel — content ownership, generation-time contract, direct-output; not duplicated below) → `tacos-orchestration/SKILL.md` Entry (includes read graph; loads `tacos-direct-output` Entry when orchestration enabled) → command-specific refs the graph lists.

Command ids: Canonical `propose`, `ff`, `explore`, … — normalize host `opsx-*` aliases per `openspec-commands.md`.

- `propose` — Create change + planning artifacts
- `ff` — Fast-forward all planning artifacts
- `continue` — Next artifact by dependencies
- `update` — Revise existing planning artifacts in place (no code edits)
- `apply` — Implement from tasks
- `explore` — Think; triggered grill when decisions crystallize (not planning grill)
- `sync` — Validate change, merge delta specs to main, validate main specs; project overview per `project-overview-hooks.md` when flags on
- `archive` — Validate change, archive; project overview per `project-overview-hooks.md` when flags on
- `verify` — Validate change, then implementation vs artifacts report

<!-- tacos-agents-end -->

<!-- tacos-implementation-gates-begin -->

## Implementation gates (local dev)

When **Commands** lists local dev checks, **apply** and **apply-review** treat them as **hard stops** on failure. Repos with no code (docs-only, specs-only) may leave **Commands** empty — tacos orchestration still applies; gates are optional.

- Redirect **build** and **test** output to `artifacts/outputs/` (or the host's documented artifacts output path). Read and search saved logs for failures; do **not** re-run a command solely to re-display output.
- Prefer **scoped** checks (changed paths, filtered test subsets) over whole-repo runs when the repo supports them.
- Run checks in order: **formatters and linters** → **build** → **tests**; when one build suffices, reuse its output for tests.

## Commands

After editing markdown under `wiki/`:

```bash
npm ci
npm run kb:lint
npm run format:check
```

Optional auto-fix for formatting only:

```bash
npm run format:write
```

<!-- tacos-doctor-discovery: npm run kb:lint; npm run format:check -->

<!-- tacos-implementation-gates-end -->
# AGENTS

