---
name: tacos-pr
description: >-
  Regenerates one GitHub PR description from OpenSpec planning artifacts and opens
  or syncs a single merge PR with preview-and-approval gates. Invoke via /tacos-pr
  only (not ambient). Not post-hoc review slices — use /tacos-slice-pr.
disable-model-invocation: true
user-invocable: true
---

# tacos pr

One markdown file per change ([output-paths](references/output-paths.md)). Regenerate, open, or sync — each GitHub write needs explicit approval. Config: `openspec/tacos.yaml` (`pr.*`, `pr.descriptions_root` → `{descriptions_root}`).

Out of scope: slicing, presentation branches, `slice-plan.md`, merge PR from `review/*` → `/tacos-slice-pr`. Inbound PR comment triage → `/tacos-pr-triage`. Never `gh` without preview + approval; never auto-sync on artifact save.

## Quick start

|User says|Workflow|
|-|-|
|Regenerate PR description / `/tacos-pr` regenerate|Regenerate → `{descriptions_root}/<change>/pr-descriptions/<change>.md`|
|Open a PR with that description|Open → preview → approval → `gh pr create`|
|Sync PR / push description to GitHub|Sync → preview → approval → `gh pr edit`|

Sample report line: `Wrote {descriptions_root}/<change>/pr-descriptions/<change>.md (regenerate only; run sync when ready).`

## Gating

`pr.enabled: true` for GitHub writes when key exists (missing `pr` → enabled). Regenerate OK when disabled. Warn if `gh` missing. Resolve change before workflow ([change-resolution](references/change-resolution.md) — active or `archive/`; branch diff to target when unnamed). Checkout preflight: [preflight](references/preflight.md).

## Approval

Intent ≠ approval — open/sync in the starting message is not `gh` authorization. Gates: [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync). Create/sync procedure: [gh-pr-create-single](references/gh-pr-create-single.md), [push-to-github](references/push-to-github.md).

## Workflows

|Trigger|Steps|
|-|-|
|Regenerate|[regenerate-descriptions](references/regenerate-descriptions.md)|
|Open|Regenerate if needed → preview → approval → [gh-pr-create-single](references/gh-pr-create-single.md)|
|Sync|Regenerate if `pr.regenerate_on_sync` (default) → preview → approval → [push-to-github](references/push-to-github.md)|
|Push only|Scoped file → preview → approval → `gh pr edit`|

Entry: read `pr.*` → [change-resolution](references/change-resolution.md) → intent (regen · open · sync · push) → run → report path/URL/warnings. Slice/`review/*` requests → `/tacos-slice-pr`.

Natural-language flags: regenerate · open/create · sync · push-only · `--no-draft` · base/trunk per [base-branch-resolution](references/base-branch-resolution.md). Ambiguous → ask once.

Cross-workflow: Existing PR for head → Sync, not Open. No `pr_number` on sync → offer Open first. Regenerate then sync in one turn → run Sync after regenerate completes.

## Hard rules

- No prior description bodies or live PR bodies as generation inputs.
- No `slice-plan.md` or Review passes (slice-pr).
- `preserve_why: true` → verbatim Why on regenerate.
- Host PR title: discover per [pr-title-conventions](references/pr-title-conventions.md) before regenerate `title:` or `gh pr create`; preview shows literal `--title`, not a paraphrase.
- No agent footers or tacos hints in GitHub-visible bodies.

## Done when

- Regenerate: `{descriptions_root}/<change>/pr-descriptions/<change>.md` exists; chat reports the path.
- Open / Sync / Push: user approved preview per [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync); `gh` completed or user cancelled after preview (no silent write).

## References

[change-resolution](references/change-resolution.md) · [base-branch-resolution](references/base-branch-resolution.md) · [pr-title-conventions](references/pr-title-conventions.md) · [preflight](references/preflight.md) · [output-paths](references/output-paths.md) · [regenerate-descriptions](references/regenerate-descriptions.md) · [gh-pr-create-single](references/gh-pr-create-single.md) · [push-to-github](references/push-to-github.md) · [approval-prompt](../tacos-orchestration/references/approval-prompt.md#github-pr-create-and-sync) · [description-template](references/description-template.md)
