# Change resolution (active or archived)

Resolve the OpenSpec **planning artifact directory** and stable **change id** before regenerate, open, or sync. Works when the change lives under `openspec/changes/<name>/` or `openspec/changes/archive/<dated-name>/` (already archived).

## Change id vs artifact directory

|Field|Rule|
|-|-|
|**Artifact directory**|Resolved folder with `proposal.md` (active or `archive/`)|
|**Change id**|Basename of that folder (e.g. `add-<feature>`, `2026-05-31-add-<feature>`)|
|**Output**|`{descriptions_root}/<change id>/pr-descriptions/<change id>.md` per [output-paths.md](output-paths.md)|

If an existing description file uses a different change id (e.g. undated slug while archive folder is dated), prefer the id in that file’s path and frontmatter.

## Resolution order

1. **User names the change** — folder basename or unambiguous slug; search active then `archive/` (prefix match on dated folders allowed).
2. **Single active change** — exactly one non-archive dir under `openspec/changes/` with planning artifacts → use it.
3. **`openspec list`** — when available, one active change → use it.
4. **Branch diff to target (default when on a feature branch)** — [base-branch-resolution.md](base-branch-resolution.md) for trunk/base, then:

```bash
git fetch origin
git diff <base-ref>...HEAD --name-only
```

Score every candidate under `openspec/changes/*/` and `openspec/changes/archive/*/` that contains `proposal.md`:

|Signal|Weight|
|-|-|
|Diff path appears in `tasks.md` (paths, backticks, apply targets)|+3|
|Diff path appears in `design.md` or `proposal.md`|+2|
|Diff path under `openspec/specs/` matches a delta in that change’s `specs/**/*.md`|+2|
|Branch name contains change id or dated folder suffix (after `YYYY-MM-DD-`)|+2|
|Candidate is **active** (not under `archive/`)|+1|

Highest score wins when margin over second place is **≥2**. If top two differ by **≤1**, ask once (show branch, base, top candidates, diff file count).

5. **Existing description** — `pr-descriptions/*.md` with `head_branch` matching current branch and clearest `pr_number` / change id.
6. **Ask** — list active + recent archive candidates; user picks.

## After resolve

- Read planning artifacts only from the resolved **artifact directory** (not from `openspec/specs/` alone unless the change has no local copies).
- Record resolved `artifact_dir` in session; use **change id** for output paths and frontmatter `head_branch` / `base_branch` updates.
- **Head** = current branch; **base** = trunk from [base-branch-resolution.md](base-branch-resolution.md) (same ref used for diff).

## Ambiguity / failure

- No candidate scores above zero and user did not name a change → stop; ask for change name or confirm base/head branches.
- Detached `HEAD` without branch name → ask user for feature branch before open/sync.
- Diff empty (branch equals base) → warn; resolve change by name or description file only.
