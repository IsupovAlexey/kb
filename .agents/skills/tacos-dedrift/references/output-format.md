# Dedrift review output

Explicit `/tacos-dedrift` runs **MUST** produce a review artifact unless the user waives artifact write in chat.

## Path

```
artifacts/openspec-reviews/dedrift-<slug>.md
```

|Slug source|Example|
|-|-|
|Explicit capabilities joined|`dedrift-apply-review-post-artifact-orchestration.md`|
|`all`|`dedrift-all.md`|
|Bound change context (optional)|`artifacts/openspec-reviews/<change-id>/dedrift-<slug>.md` when parent prompt names a change|

Prefer flat `artifacts/openspec-reviews/dedrift-*.md` unless orchestration bound the run to a change folder.

## Frontmatter

```yaml
---
# Set dedrift_mode to exactly one value: reconcile or conform
dedrift_mode: reconcile
# run_mode: scheduled  # optional — set only for scheduled-job-prompt.md runs
# depth: deep  # optional artifact frontmatter — deep tier only (## Run metadata depth: standard|deep is separate)
capabilities: [apply-review, post-artifact-orchestration]
run_at: <ISO-8601>
---
```

## Body template

```markdown
# Dedrift review — <slug>

**Mode:** reconcile _(one of: reconcile, conform)_  
**Scope:** <capability list or all>

## Summary

|Status|Count|
|-|-|
|aligned|N|
|stale spec|N|
|code violation|N|
|needs human decision|N|

## Per capability

### <capability>

- **Status:** aligned | stale spec | code violation | needs human decision
- **Spec:** `openspec/specs/<capability>/spec.md`
- **Finding:** …
- **Proposed action:** … (none if aligned)

## Writes applied

- (list spec and code paths written after Proceed, or "none — cancelled")
```

### Scheduled runs (`run_mode: scheduled`)

Per-run source of truth for the operator; mirror the same sections in the PR body when create/edit succeeds ([scheduled-job-prompt.md](scheduled-job-prompt.md) § 7).

**Outcomes:** `no-drift` — aligned only or empty scope; artifact only, no branch/PR. `decision-signal` — needs human decision items with empty reconcile write set; baseline-only commit, PR open/update, no main spec writes. Reconcile runs — spec writes plus PR when applicable.

```markdown
## Capabilities reconciled

- <capability>: <finding>

## Needs human decision

- <capability>: <reason>

## Overview reconciled

- <section>: <finding>

## Overview needs human decision

- <section or path>: <reason>

## Host overlay updated

- <path_map | skill_map | governed scope | exclude_path_prefixes>: <change> — <rationale>

## Unmapped paths (skipped)

- <path>

## Run metadata

- baseline: <ref or window>
- format commits: <count> (all paths excluded)
- dedrift commits: <count> (all paths excluded)
- depth: standard | deep
- deep_stopped: stable | cap | n/a
- label: applied | skipped (<reason>)
- run mode: first open | subsequent
```

## Deep pass artifacts

When depth is `deep`, write loop pass artifacts per [deep-mode.md](deep-mode.md):

```
artifacts/openspec-reviews/dedrift-deep-N.md
```

Append **## Deep pass outcome** to the final `dedrift-<slug>.md` when deep ran:

```markdown
## Deep pass outcome

- mode: deep
- depth: scheduled | interactive
- pass_number: <final pass>
- max_iterations: <cap>
- stopped: stable | cap
- prior_deep_artifacts: <paths to dedrift-deep-*.md>
```

## Chat summary

After Proceed or Cancel, parent posts a short chat summary:

- Mode and scope
- Counts by status
- Path to review artifact (when written)
- List of spec/code files changed (Proceed) or cancellation note (Cancel)

Scheduled runs: include artifact path in the chat summary only — not in the PR body ([scheduled-job-prompt.md](scheduled-job-prompt.md) § 7).

## Status definitions

|Status|Meaning|
|-|-|
|**aligned**|Main spec behavioral obligations match implementation|
|**stale spec**|Code reflects shipped behavior; spec missing or outdated (reconcile candidate)|
|**code violation**|Spec behavioral SHALL/MUST not met by code (conform candidate)|
|**needs human decision**|Reconcile vs conform unclear, or both spec and code appear wrong|
