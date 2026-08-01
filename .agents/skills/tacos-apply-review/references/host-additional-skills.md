# Host additional skills — apply review

Apply when running `tacos-apply-review` (including Task/subagent runs).

## Entry shape

`review.apply_review_additional_skills` entries are **repo-relative path strings** only. Model selection uses `review.additional_apply_review_models` in installed additional-apply-review agent frontmatter — not per-entry yaml overrides.

See [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md) (spawn names, models, parallel merge).

## Delegation modes

|Mode|Who runs it|Additional skills|
|-|-|-|
|**Parallel apply pass**|Apply orchestrator per stage **or manual `/tacos-apply-review` parent**|One **`tacos-additional-apply-review`** Task per **applicable** path (applicability-skipped entries recorded in parent merge); core child does **not** load this array|
|**Re-review after fixes**|Apply orchestrator **or manual `/tacos-apply-review` parent** when runtime supports concurrent spawn|Match initial pass — parallel core + **`tacos-additional-apply-review`** per **applicable** path; parent merge into `apply-review-<stage>-r2.md` (or increment)|
|**Re-review sequential fallback**|One fresh **`tacos-apply-review`** Task child when concurrent multi-spawn unavailable|Core rubric then each applicable entry **sequentially** in the **same** child|
|**Deep**|One fresh **`tacos-apply-review`** Task child per pass|Core dual-pass then each applicable entry **sequentially** when non-empty (mirror re-review sequential fallback); parent does **not** spawn parallel additional children|

Manual slash invoke and staged **Apply review:** use the **same** parallel pass — there is no manual-only single-subagent shortcut when the array is non-empty.

### Parallel apply pass (orchestrator or manual parent)

When the parent runs an **Apply review:** line **or** the user invokes `/tacos-apply-review` and the array is non-empty:

1. **Core child** — `subagent_type` **`agent-tacos-apply-review`** only (`references/review-format.md`, `references/checklist-pass1.md` for initial pass; `checklist-pass2.md` when pass-1 clean per SKILL). Do **not** read `review.apply_review_additional_skills` in the core child.
2. **Applicability** — for each additional entry, evaluate [Apply review applicability](#apply-review-applicability-host-skills) against the review diff. **Skip spawn** when scope is confidently inferred and zero diff paths match; record skip in parent merge. **Spawn** when scope is not confidently inferable, diff is empty or ambiguous, or skill file is unreadable.
3. **Per applicable entry** — one fresh **`tacos-additional-apply-review`** child scoped to **that** host skill plus shared diff + change context.
4. **Parent** — merges into `artifacts/openspec-reviews/<change>/apply-review-<stage>.md`; `## Skipped additional skills` when any entry was applicability-skipped; `## Parallel delegation warnings` on failed children. See [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md).

### Re-review after fixes

**Mandatory** after orchestrator fixes from a failing apply review — parent self-report does not pass the gate ([review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) ## Anti-short-circuit). Append **Re-review after fixes** per [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) **Dynamic re-review checkbox** before fixes. Record [Turn-summary delegation record](../../tacos-orchestration/references/review-gate-pass.md#turn-summary-delegation-record). **Forbidden:** parent-authored `apply-review-*-rN.md`.

When `review.apply_review_additional_skills` is empty → one fresh core **`agent-tacos-apply-review`** subagent; write `apply-review-<stage>-r2.md` or increment.

When the array is non-empty and the runtime supports concurrent Task spawns, **match initial apply review parallel launch** ([post-artifact-signoff.md](../../tacos-orchestration/references/post-artifact-signoff.md) **Re-review after fixes**):

1. **Applicability** — same rules as [Parallel apply pass](#parallel-apply-pass-orchestrator-or-manual-parent) step 2
2. Launch **parallel** subagents in **one parent turn** when the host allows:
   - **Core:** fresh Task with `subagent_type` **`agent-tacos-apply-review`**
   - **Per applicable entry:** one fresh Task with **`agent-tacos-additional-apply-review`** per repo-relative path
3. **Parent merge** into `apply-review-<stage>-r2.md` (or increment) with **`## Skipped additional skills`** and **`## Parallel delegation warnings`** when needed

**Sequential fallback:** When Task is supported but concurrent multi-spawn is not, one fresh **`tacos-apply-review`** subagent: tacos apply dimensions first, then each applicable additional entry in array order (steps 2–5 below) in the **same** child, applying the same [applicability](#apply-review-applicability-host-skills) rules. The **parent** does not merge parallel children for this fallback path. Note sequential fallback in turn summary.

### Deep (manual invoke)

One fresh **`tacos-apply-review`** subagent per deep pass: complete core dual-pass (pass 1 → pass 2 when pass 1 and main-spec drift are clean) first, then when `review.apply_review_additional_skills` is non-empty run each applicable entry in array order (steps 2–5) in the **same** child — same applicability rules as re-review. Record applicability skips under **`## Skipped additional skills`** in the deep artifact. The **parent** does **not** spawn parallel `tacos-additional-apply-review` children during deep. **Forbidden:** parallel additional swarm per deep pass; parent-authored `apply-review-deep-N.md`. Procedure: [deep-mode.md](deep-mode.md).

## Apply review applicability (host skills)

Parent orchestrators evaluate whether each entry in `review.apply_review_additional_skills` applies to the review diff before spawning **`tacos-additional-apply-review`**. Yaml entries remain repo-relative path strings only.

### Inferring scope (parent)

For each additional path:

1. Resolve the review diff path list (staged stage diff, tacos-work session diff, or explicit scope from `/tacos-apply-review`).
2. Resolve `SKILL.md` (or file path) and infer implementation scope from the skill **name**, **description** (frontmatter and opening prose), directory name, and path segments — stack, subsystem, and file-type cues implied by that identity (e.g. a frontend review skill → client paths and `.tsx`; a backend API skill → `Api/` and `.cs`).
3. Match diff paths against the inferred scope; path separators normalized to `/`.
4. **Skip spawn** when inference is **confident** and **zero** diff paths fall in scope — record `path | reason` (e.g. `no matching diff paths`).
5. **Spawn** when scope cannot be inferred confidently, diff is empty or ambiguous, or skill file is missing (missing file still WARNs in merge per step 2 below).

Record applicability-skipped entries in the merged artifact under **`## Skipped additional skills`** (omit section when empty).

### Child quick-exit (after spawn)

When **`tacos-additional-apply-review`** runs but re-check against inferred scope shows zero matching paths, return minimal tacos-format output: one **INFO** line naming the skill path and `N/A — no matching diff paths`; do **not** run the full host rubric.

## 1. Load the list

Read `openspec/tacos.yaml` → `review.apply_review_additional_skills`.

- Missing key or `[]` → skip this file; use only `references/review-format.md`, `references/checklist-pass1.md` / `checklist-pass2.md`, and `tacos-implementation-conventions`.
- Non-empty in **re-review** or **deep** mode (inside one fresh core child) → complete steps 2–5 for each entry.
- Non-empty when you are the **parallel core child** → do **not** load this array (parent spawns **`tacos-additional-apply-review`** per applicable path).
- Non-empty when you are the **manual or orchestrator parent** → spawn parallel children per **Parallel apply pass**; do **not** load host skills inline on the parent.

## 2. Resolve each path

For every path string in order:

1. If the path is a directory, read `<path>/SKILL.md`.
2. If the path is a file, read that file.
3. If neither exists, record a **WARN** in the review (path + “not found”) and continue with remaining paths.

Do not guess alternate locations.

## 3. Apply host rubrics

Treat each loaded skill as **additive** to the tacos core checklist:

- Run tacos dimensions first (spec compliance, conventions rubric, host `AGENTS.md` / `openspec/config.yaml` `context`).
- Then apply every requirement and checklist item from host skills that applies to the diff scope.
- Pull linked `references/` only when needed for the files under review — do not load entire skill trees upfront.

When a host skill conflicts with tacos core on file layout, co-location, module structure, style, or naming, **host skill wins** for repo-specific rules. Core DRY findings are satisfied by inline consolidation on the owner when host placement requires it — not by a new top-level file. When it conflicts with OpenSpec proposal/specs/design for the change, flag **BLOCKER** or **CRITICAL** (spec wins).

**Apply fix loop:** when addressing core DRY or duplication findings on a diff with configured host skills, read host placement rules before adding files or top-level modules.

## 4. Cite findings

Findings driven by a host skill **MUST** name the skill path (or skill `name` from frontmatter) in the finding line, e.g. `backend-testing | tests/... | …`.

## 5. Orchestrator launching Task

Pass scoped prompts per delegation mode above. Do not paraphrase host rules — children must read the skills. For parallel passes, do **not** pass the full additional array to the core child. Do **not** pass `model` from yaml on Task.

**Same-turn rule:** When launching parallel apply review (staged **Apply review:** or manual `/tacos-apply-review`), issue **all** Task spawns (core + each **applicable** additional path) before waiting on any child. A completed core-only run does **not** satisfy apply review when the yaml array has entries that were applicable (not applicability-skipped). Full contract: [post-artifact-signoff.md](../../tacos-orchestration/references/post-artifact-signoff.md) **Apply review — parallel launch**.

## 6. Forbidden

- Loading all additional skills inside the **core** parallel child (parent owns per-skill children + merge).
- Satisfying manual `/tacos-apply-review` by running core-only or by loading additional skills inline on the parent when the array is non-empty and Task is supported (initial pass only — deep and re-review load additional skills inside one child).
- Parallel `tacos-additional-apply-review` swarm during deep (one child, sequential additional skills only).
- Inventing host rules when the array is empty.
- Skipping listed paths without applicability evaluation per [Apply review applicability](#apply-review-applicability-host-skills) (skip only on confident inference with zero matching paths).
- Replacing tacos review output format or severity guide with a host-only template.
- Per-entry `{ path, model? }` objects or Task-time yaml model resolution.
