# Host additional skills — spec review

Apply when running `tacos-spec-review` (including Task/subagent runs).

## Entry shape

`review.spec_review_additional_skills` entries are **repo-relative path strings** only (from repository root to a host skill directory or `SKILL.md`). Skills install root: **`/tacos-doctor diagnose`**. Model selection uses `review.additional_spec_review_models` in installed additional-spec-review agent frontmatter — not per-entry yaml overrides.

See [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md) (spawn names, models, parallel merge).

## Delegation modes

|Mode|Who runs it|Additional skills|
|-|-|-|
|**Parallel initial pass**|POST-ARTIFACT parent **or manual `/tacos-spec-review` parent**|One **`tacos-additional-spec-review`** Task per path; core child does **not** load this array|
|**Delta r2**|Parent when concurrent spawn supported; else one fresh **`tacos-spec-review`** Task child|Match initial pass — parallel core + per-entry additional with parent merge; sequential core-then-additional in one child when multi-spawn unavailable|
|**Deep**|One fresh **`tacos-spec-review`** Task child per pass|Full core dimensions then each applicable entry **sequentially** when non-empty (mirror delta r2); parent does **not** spawn parallel additional children|

Manual slash invoke and POST-ARTIFACT use the **same** parallel initial pass — there is no manual-only single-subagent shortcut when the array is non-empty.

### Parallel initial pass (orchestrator or manual parent)

When the parent launches parallel review (POST-ARTIFACT or `/tacos-spec-review`):

1. **Core child** — `subagent_type` **`agent-tacos-spec-review`**; scope: tacos rubric only (per `SKILL.md` ## Workflow). Do **not** read `review.spec_review_additional_skills` in the core child.
2. **Per additional entry** — one fresh **`tacos-additional-spec-review`** child scoped to **that** host skill path (read that path’s `SKILL.md` or file; shared inputs: change folder, artifact scope, `grill-summaries.md` when present).
3. **Parent** — merges child outputs into one tacos-format artifact; adds `## Parallel delegation warnings` for failed children. See [post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) **Planning spec review — parallel launch**.

### Delta r2 (orchestrator)

When `review.spec_review_additional_skills` is empty → one fresh core **`agent-tacos-spec-review`** subagent only.

When the array is non-empty and the runtime supports concurrent Task spawns, **match initial POST-ARTIFACT parallel launch** ([post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) **Delta re-review**): parallel core + per-entry additional children; **parent** merges into `*-review-r2.md`.

**Sequential fallback:** one fresh **`tacos-spec-review`** subagent runs full [dimensions.md](dimensions.md) first, then for each additional entry in array order completes steps 2–5 below inside **that same** child.

Append **Delta re-review after fixes** to `openspec/changes/<name>/tasks.md` per [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) **Dynamic re-review checkbox**. Record [Turn-summary delegation record](../../tacos-orchestration/references/review-gate-pass.md#turn-summary-delegation-record). **Forbidden:** parent-authored `*-review-rN.md`.

### Deep (manual invoke)

One fresh **`tacos-spec-review`** subagent per deep pass: run full [dimensions.md](dimensions.md) first, then when `review.spec_review_additional_skills` is non-empty complete steps 2–5 for each applicable entry in array order inside **that same** child — same sequential pattern as delta r2. Record applicability skips under **`## Skipped additional skills`** in the deep artifact. The **parent** does **not** spawn parallel `tacos-additional-spec-review` children during deep. **Forbidden:** parallel additional swarm per deep pass; parent-authored `*-deep-N.md`. Procedure: [deep-mode.md](deep-mode.md).

## 1. Load the list

Read `openspec/tacos.yaml` → `review.spec_review_additional_skills`.

- Missing key or `[]` → skip host skills; use only tacos core rubric and inputs per `SKILL.md`.
- Non-empty in **delta r2** or **deep** mode (inside one fresh core child) → complete steps 2–5 for each entry.
- Non-empty when you are the **parallel core child** → do **not** load this array (parent spawns **`tacos-additional-spec-review`** per path).
- Non-empty when you are the **manual or POST-ARTIFACT parent** → spawn parallel children per **Parallel initial pass**; do **not** load host skills inline on the parent.

## 2. Resolve each path

For every path string in order:

1. If the path is a directory, read `<path>/SKILL.md`.
2. If the path is a file, read that file.
3. If neither exists, record a **WARN** in the review (path + “not found”) and continue with remaining paths.

Do not guess alternate locations.

## 3. Apply host rubrics

Treat each loaded skill as **additive** to the tacos core rubric (per `SKILL.md` ## Workflow step 3):

- Run full [dimensions.md](dimensions.md) first (delta-r2): completeness, grill alignment, task-stage contract, Intent fidelity, Implicit branch coverage, traceability, etc.
- Then apply every requirement and checklist item from host skills that applies to the artifact(s) under review.
- Pull linked `references/` only when needed for that scope — do not load entire skill trees upfront.

When a host skill adds stricter planning rules (e.g. diagram required, naming), enforce them in **Must address**. When a host skill contradicts resolved `grill-summaries.md` **User inputs**, flag **BLOCKER** unless the human waived in chat.

## 4. Cite findings

Findings driven by a host skill **MUST** name the skill path (or skill `name` from frontmatter) in the finding line.

## 5. Orchestrator launching Task

Pass scoped prompts per delegation mode above. Do not paraphrase host rules — children must read the skills. For parallel passes, do **not** pass the full additional array to the core child. Do **not** pass `model` from yaml on Task.

**Same-turn rule:** When launching parallel planning spec review (POST-ARTIFACT or manual `/tacos-spec-review`), issue **all** Task spawns (core + each additional path) before waiting on any child. A completed core-only run does **not** satisfy planning spec review when the yaml array is non-empty. Full contract: [post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) **Planning spec review — parallel launch**.

## 6. Forbidden

- Loading all additional skills inside the **core** parallel child (parent owns per-skill children + merge).
- Satisfying manual `/tacos-spec-review` by running core-only or by loading additional skills inline on the parent when the array is non-empty and Task is supported.
- Inventing host rules when the array is empty, or skipping listed paths to save tokens.
- Replacing tacos output format (**Grill alignment**, **Complexity & split**) with a host-only template.
- Inline-only spec review in the authoring thread when orchestration requires Task delegation.
- Delta re-review in the same thread that applied review fixes (use a fresh Task subagent).
- Parallel delta r2 swarm (one child, sequential additional skills only).
- Parallel deep swarm (one child per pass, sequential additional skills only).
- Per-entry `{ path, model? }` objects or Task-time yaml model resolution.
