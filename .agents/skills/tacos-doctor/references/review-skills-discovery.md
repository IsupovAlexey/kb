# Review skills — agent discovery

Host review skill wiring is **agent-mediated**. `schema.cs` only performs additive yaml merge for empty or missing `review.spec_review_additional_skills` and `review.apply_review_additional_skills` arrays via `merge-review-skills`. Do **not** implement discovery heuristics, exclusion lists, keyword matching, or gap detection in C# doctor scripts — the install/update agent judges which skills qualify.

Run this workflow after **Implementation gates discovery** (when triggered) and before `openspec schema validate tacos` on **Install**. On **Update**, run when **one or both** `review.spec_review_additional_skills` or `review.apply_review_additional_skills` keys are absent or `[]`.

## Non-code and docs-only hosts

Hosts with no application code may still have review-oriented skills (policy, prose standards). When the repo is clearly non-code and no qualifying host skills exist, report gaps and suggest `/tacos-host-skill` — do **not** invent paths.

When non-code hosts already have review skills under a detected skills root, wire them the same as code hosts.

## Read order

1. README, CONTRIBUTING, root docs for declared host review skill paths or naming conventions
2. `AGENTS.md` **outside** `<!-- tacos-agents-* -->` and `<!-- tacos-implementation-gates-* -->` regions
3. `openspec/host/*.md` when activated (not `*.md.template`) for declared review skill paths
4. Scan detected host skills roots (`schema.cs diagnose` / `repo-skills.cs` candidates — prefer the project skills root where `tacos-orchestration/SKILL.md` exists) for non-`tacos-*` skills whose **name**, **directory**, or **description** implies review, conventions, or testing rubrics

Only wire paths that resolve to readable `SKILL.md` on disk. Repo-relative path strings only (same shape as `review.*_additional_skills` entries elsewhere).

## Exclusion list

Always exclude:

- `tacos-*` skills (bundle-owned)
- Scaffolding or meta skills (create-skill helpers and similar) unless human docs explicitly declare them as review rubrics
- Skills with frontmatter that **explicitly** opts out of review wiring — judge from frontmatter and description; if a skill is clearly not a review rubric despite naming overlap, do **not** wire it
- Paths that do not resolve to an existing skill directory or `SKILL.md`

## Wiring heuristics

Entries are repo-relative path strings. **Anchor rule:** match each array to what its review pass reviews — **tacos-spec-review** runs on planning artifacts only; **tacos-apply-review** runs on implementation diffs (code, skills, config, conventions, tests).

### Optional frontmatter override

When `SKILL.md` frontmatter includes `tacos-review-wiring: apply-only | spec-only | both`, use that value before the signal table below.

### Signal table

|Signal in name, directory, or `description`|Wire to|
|-|-|
|Conventions, testing, lint, build, commands, code/style, ship-time/PR checklist, implementation paths|`review.apply_review_additional_skills` **only**|
|Planning, proposal, design, spec rubric, architecture **at planning**, task-stage planning cues|`review.spec_review_additional_skills` **only**|
|**Explicit** frontmatter or description stating **both** planning-artifact review **and** implementation-diff review|**Both** arrays when each is absent or `[]`|
|Uncertain|`review.apply_review_additional_skills` **only**|

### Spec array MUST NOT receive

Do **not** pass `--spec` for skills that are implementation rubrics even when `review.spec_review_additional_skills` is empty:

- `*-conventions`, `*-testing`, and typical `backend-*` / `frontend-*` stack rubrics unless description explicitly targets planning artifacts
- Skills whose `description` WHEN clause is writing or reviewing code, paths, or tests only (for example `backend-conventions`)

**Preserve policy:** populate only when a key is missing or `[]`. Never append to or replace non-empty arrays. When arrays are already populated, skip yaml writes and WARN on diagnose (see [schema.md](schema.md)).

## Write `openspec/tacos.yaml`

For each qualifying skill path:

1. Validate the path resolves to readable `SKILL.md`.
2. Apply wiring heuristics above per array.
3. Invoke `dotnet scripts/schema.cs merge-review-skills` with `--spec` and/or `--apply` for each path (repeat flags per path). C# populates only empty arrays; preserves non-empty arrays.
4. Repo-relative path strings only — no per-entry model overrides (models use `review.additional_*_review_models` in bundle agents).

Do **not** hand-edit yaml arrays when merge-review-skills can apply discovered paths. Do **not** create or modify host skill files during install or update.

## Summarize for the user

|Outcome|Message|
|-|-|
|Arrays populated|OK — review skills wired from discovered host skills|
|Arrays preserved (non-empty)|WARN — existing review array entries preserved; verify paths match intended skills|
|No candidates found|OK — suggest `/tacos-host-skill` to bootstrap review-oriented host skills; no yaml writes|
|Gaps for detected stacks|WARN — repo layout implies missing conventions/testing skills; suggest `/tacos-host-skill`|

Install and update are **suggest-only** for gaps — never create host skill directories or bodies.

## Update

Run discovery when **one or both** review arrays are absent or `[]`. Populate **only** arrays that are missing or `[]`; never modify arrays that already contain path strings. When discovery would match candidates but an array is non-empty, skip writes to that array and WARN that wiring was preserved. When **both** arrays are non-empty, skip all yaml writes and WARN that maintainers should verify entries.
