# Plan, approve, and slice-plan.md

Group diff → present plan → **slice-plan approval** → write `slice-plan.md` → **branch-execution approval**. Parent procedure continues with execute, verify, [post-verify-confirm](post-verify-confirm.md), and PR create ([post-pr-create](post-pr-create.md); preview per [gh-pr-create-single](../../tacos-pr/references/gh-pr-create-single.md)).

## Present the plan

Bottom-to-top **slices** (one merge PR). Per slice:

- **Slice** (`1`, `2`, …)
- Presentation branch: `review/<change-slug>` (override only with slice-plan approval)
- **Commit message** — host rules first (`pr-title-conventions` ref), optional `[n/N]`
- **Size** — `+lines`, `-lines`, churn, file count
- Files; partial files: **this slice’s** hunk indices only (disjoint per slice; **cumulative** union through slice k at execute — `execute-slice` ref)

Optional Mermaid when three+ slices. Revisions require re-ask for slice-plan approval before write.

## Slice-plan approval gate

**MUST NOT** write `slice-plan.md` or create the presentation branch until explicit approval.

**Prompt:** `Approve this slice plan and write slice-plan.md?`

|id|label|
|-|-|
|`approve`|Approve plan — write slice-plan.md|
|`revise`|Revise grouping — no file write|
|`cancel`|Cancel|

On `approve`: write plan, then branch-execution gate.

Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): after showing the plan, **next tool call MUST** be `AskQuestion` / `AskUserQuestion` when tools are available — **forbidden:** prose-only approval. Plain-text fallback only when tools absent: show plan → end turn → unambiguous yes.

**Never counts as approval:** `/tacos-slice-pr` alone, silence, ambiguous “looks good” (see parent `approval-prompt` for shared rules).

## Write slice-plan.md

**Path** (`pr.descriptions_root` from `openspec/tacos.yaml`, default `artifacts/openspec-artifacts`):

```text
{repo-root}/{descriptions_root}/<change-name>/slice-plan.md
{repo-root}/{descriptions_root}/<change-name>/pr-descriptions/01-<merge-short-name>.md
{repo-root}/{descriptions_root}/<change-name>/pr-descriptions/02-<slice-short-name>.md
```

- `<change-name>` — `openspec/changes/<change-name>/`
- `01-*.md` — merge PR description; `02+` — optional slice notes (same `pr_number`, not separate PRs)
- **Do not** commit `{descriptions_root}/**` unless the host opts in

Write **once** after slice-plan approval, **before** branch-execution approval. Immutable during execution.

Hunk indices **MUST** match split-diff `analyze`. Headers **MUST** use `### Slice N —` (parser for `verify-slices`).

### Template

```yaml
---
approved_at:
feature_branch:
base_branch:
presentation_branch: review/<change-slug>
source_change:
immutable: true
---
```

```markdown
# Slice plan

## Context

- Feature branch: ``
- Base branch: ``(e.g.`origin/main`)
- Presentation branch: `review/<change-slug>`
- Original feature branch preserved: yes (read-only)

## Slices (review order: bottom → top)

### Slice 1 — <short title>

- **PR title / commit message:** ``(host prefix first; optional`[1/N]` after prefix)
- **Depends on:** trunk tip
- **Files:**
  - `path/to/file.ts` (all hunks)
  - `path/other.ts` hunks `0,2`

### Slice 2 — <short title>

- **PR title / commit message:** ``
- **Depends on:** slice 1 commit on presentation branch
- **Files:**
  - `path/other.ts` hunks `1`

## Notes

<!-- Tasks stage mismatch, binary/whole-file, squash-merge review-only, etc. -->
```

## Branch-execution approval gate

**MUST NOT** `git checkout -b review/...` or commit until explicit approval **after** `slice-plan.md` exists.

**Prompt:** `Create presentation branch and commits from slice-plan.md?`

|id|label|
|-|-|
|`approve`|Create `review/<change-slug>` and slice commits|
|`defer`|Keep plan only — no branch|
|`cancel`|Cancel|

On `approve`: parent procedure step 7 (`execute-slice` ref). On `defer`: stop with plan file only.

Per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md): **next tool call MUST** be structured prompt when tools available — **forbidden:** prose-only menu.

## Immutability

After slice-plan approval and write: **MUST NOT** edit `slice-plan.md` during execution. Failure + new grouping → void plan, new approval, overwrite only after fresh approval.
