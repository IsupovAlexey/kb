# Deep dedrift (reconcile)

Opt-in depth tier for explicit `/tacos-dedrift reconcile` and scheduled runs when `dedrift.scheduled_depth` is `deep`. Reconcile-only — `conform … deep` MUST be rejected before detect. Not orchestration-hook auto-upgrade; not ambient.

## Invoke

|Form|Example|
|-|-|
|Deep|`/tacos-dedrift reconcile apply-review deep`|
|Deep + cap|`/tacos-dedrift reconcile all deep 10`|
|Multi-cap|`/tacos-dedrift reconcile apply-review post-artifact-orchestration deep`|

Parse tokens: `deep`; optional positive integer immediately after `deep` overrides `dedrift.deep_max_iterations` for this invoke. Invalid, zero, or negative cap → yaml default.

Config: `openspec/tacos.yaml` — `dedrift.deep_max_iterations` (default **5**), `dedrift.scheduled_depth` (`standard` | `deep`, default **standard**). Host overlay `scheduled_depth` in `openspec/host/dedrift-job.md` overrides yaml for scheduled runs when set.

## Reject conform + deep

When mode is **conform** and `deep` token is present, stop before detect with a clear error: deep is reconcile-only. Do not run detect or preview.

## Parent orchestrator (mandatory)

**Never detect inline** during the deep loop. Every pass MUST delegate detect via Task per [delegation.md](delegation.md) ## Deep detect loop.

If Task is unavailable:

- **Interactive:** report in chat (`deep dedrift failed — Task unavailable`) and stop — do not parent-author `dedrift-deep-N.md` without a child detect pass.
- **Scheduled:** write failure to the review artifact (§ 8) and exit without spec writes or PR open/update.

### Deep loop procedure (parent)

The **parent** invoker owns iteration — not detect subagents.

1. Resolve mode, scope, depth, cap; load all prior `dedrift-deep-*.md` for this sweep when `pass_number > 1`.
2. **Pass 1:** Fresh Task detect child(ren) per [delegation.md](delegation.md) ## Deep detect loop — full scoped detect against on-disk specs + codebase.
3. **Pass N>1:** Merge accumulated reconcile proposals into the in-memory spec baseline ([Between passes](#interactive-explicit-invoke)); launch **fresh** Task detect child(ren) again — **forbidden:** reuse pass 1 reports without re-detect.
4. Parent merges batch reports; writes `dedrift-deep-N.md` from child output only (**forbidden:** parent-authored loop artifacts without a detect pass).
5. Read **## Deep pass outcome**; compare per-capability status map and classification counts vs the prior pass.
6. **Stop** per [Stop semantics](#stop-semantics); **verify re-detect** is one additional fresh Task round after the loop stabilizes.

### New findings rule

Each pass classifies against the **in-memory baseline** (merged partial reconciles), not only on-disk specs. Treat as **new this pass** any capability whose status or proposed reconcile differs from cumulative prior `dedrift-deep-*.md` for that capability (match by capability id + finding essence). The loop continues while the per-capability status map or classification counts change.

### Scheduled idempotency (within one run)

|Signal|Detect scope|
|-|-|
|Pass 1, no prior `dedrift-deep-*.md` this run|Full scoped detect per [scheduled-job-prompt.md](scheduled-job-prompt.md) § 5|
|Pass N>1|Re-detect after in-memory baseline merge — inputs include all prior `dedrift-deep-*.md`; scope unchanged unless § 4a overlay re-map in the same run|

**Subsequent scheduled run** (open PR, new default-branch commits): scope from git baseline trailers per scheduled-job-prompt § 2–3 — not from prior PR body. Deep loop restarts at pass 1 for that run.

## Parent loop (mandatory)

### Classification tally

Per pass, count capabilities in each status: **stale spec**, **code violation**, **needs human decision**. **Aligned** is excluded from the stop tally.

### Interactive explicit invoke

1. Parse mode, scope, depth, cap.
2. **Pass 1..N detect** — per [Parent orchestrator](#parent-orchestrator-mandatory) ## Deep loop procedure; accumulate reconcile write set across passes; do **not** write files during the loop.
3. **Between passes** — merge accumulated reconcile proposals into the in-memory spec view used for the next detect pass (no file writes). Without this step, pass 2+ would compare the same on-disk specs and the loop cannot surface drift that appears only after partial reconciles.
4. Write `artifacts/openspec-reviews/dedrift-deep-N.md` each pass with **## Deep pass outcome** (below).
5. **Stop** when per-capability status assignments match the prior pass **and** the three classification counts are unchanged → `stopped: stable`. Count-only comparison is insufficient — capability churn across buckets with stable totals (e.g. stale 2→1, code violation 1→2) is not stable.
6. **Stop** when `pass_number >= max_iterations` with status map or counts still changing → `stopped: cap`.
7. **Verify re-detect** — one final detect pass on the same scope after loop stabilizes; merge any new **stale spec** into the write set; record in the final artifact.
8. **Preview** — one combined preview for all proposed writes per [preview-gate.md](preview-gate.md). No writes until `proceed`.
9. **Write** — parent serializes accumulated reconcile spec writes after Proceed (deep path is reconcile-only).
10. When spec files were written, run validation gates per host overlay when present (`prettier --check`, `openspec validate --specs`, `verify-config-notation` on dogfood).

**Forbidden:** per-iteration preview on interactive deep; parent-authored loop artifacts without a detect pass; auto-`deep` from orchestration or ambient paths.

### Scheduled path

Resolve depth: host overlay `scheduled_depth` when set; else `dedrift.scheduled_depth` from yaml. When `standard`, § 5 in [scheduled-job-prompt.md](scheduled-job-prompt.md) runs once.

When `deep`:

1. **Pass 1..N detect** — same as interactive steps 2–6; no preview; accumulate reconcile write set.
2. **Verify re-detect** — final detect pass; merge new **stale spec** into write set.
3. Continue [scheduled-job-prompt.md](scheduled-job-prompt.md) § 6–7 with accumulated write set.
4. On `stopped: cap`, still commit partial reconciles and open/update draft PR; record cap in artifact and PR **Run metadata**.
5. Validation gates run after § 6 spec writes per host overlay — after deep loop completes, not inside each iteration.

## Deep pass artifact

|Context|Path|
|-|-|
|Default|`artifacts/openspec-reviews/dedrift-deep-N.md`|
|Orchestration-bound change|`artifacts/openspec-reviews/<change-id>/dedrift-deep-N.md` when parent prompt names a change|

Final run artifact still uses [output-format.md](output-format.md) `dedrift-<slug>.md` with **## Deep pass outcome** appended.

### Required sections

```markdown
## Deep pass outcome

- mode: deep
- depth: scheduled | interactive
- pass_number: <N>
- max_iterations: <cap>
- classification_delta: <stale spec|code violation|needs human decision counts vs prior pass; first pass use "n/a">
- stopped: pending | stable | cap
- prior_deep_artifacts: <paths>

## Classification this pass

|Status|Count|
|-|-|
|stale spec|N|
|code violation|N|
|needs human decision|N|
|aligned|N|

## Per capability

_(Same shape as output-format.md per-capability rows for this pass.)_
```

`classification_delta` all zero vs prior pass **and** per-capability status map unchanged vs prior pass drives `stopped: stable`.

## Stop semantics

|Condition|Parent action|
|-|-|
|Classification counts unchanged vs prior pass **and** per-capability status map unchanged|End detect loop → verify re-detect → preview (interactive) or § 6 (scheduled)|
|`pass_number >= max_iterations` and per-capability status map or counts still changing|End loop; `stopped: cap`; continue with accumulated write set|
|User `stop deep` (interactive)|End loop partial; preview what was accumulated|

## vs standard

||Standard|Deep|
|-|-|-|
|Detect passes|1|Until stable or cap|
|Preview|After single detect|After loop + verify re-detect|
|Scheduled|Default `dedrift.scheduled_depth`|Host overlay or yaml `deep`|
|Conform|Allowed|Rejected|
