# Scheduled main-spec dedrift

Reconcile canonical main specs (`openspec/specs/**`) with the codebase for capabilities implicated by recent git changes. **Reconcile-only** — never conform. Human review happens on the draft PR.

Do not ask questions. Do not wait for preview approval. Run `git` without interactive approval.

## 1 — Host overlay

When `openspec/host/dedrift-job.md` exists, read it and apply host overrides (path map, PR identification, branch naming, git baseline). Defaults below apply when unset.

Load `tacos-dedrift` skill references: [modes.md](modes.md), [delegation.md](delegation.md), [output-format.md](output-format.md), [deep-mode.md](deep-mode.md).

Read `openspec/tacos.yaml` → `dedrift.scheduled_depth`, `dedrift.deep_max_iterations`, `project_overview.enabled`, and `project_overview.path` (default `README.md` when unset). When host overlay has `## Project overview`, read it for section map and reconcile rules. When `project_overview.enabled` is true, also load [overview-guidance.md](../../tacos-project-overview/references/overview-guidance.md) and user-visible surface criteria from [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Project overview line (optional).

**Resolve `<pr_branch>`** (used for push, baseline trailers, and PR create) — first match wins:

1. Host overlay `pr_branch` in `openspec/host/dedrift-job.md` when set.
2. **Runtime-assigned development branch** when automation or cloud task instructions name one (e.g. Cursor Cloud `Develop on branch \`cursor/...\``). Use when the host only opens pull requests from that assigned branch.
3. Default: `dedrift/scheduled`.

Use the resolved `<pr_branch>` everywhere below — not a hardcoded branch name.

## 2 — First open vs subsequent run

Detect run mode **before** branch checkout using git only:

|Signal at run start|Mode|Pull request|
|-|-|-|
|`origin/<pr_branch>` **missing**|**First open**|After push: **open** a pull request **once** with the body template (§ 7)|
|`origin/<pr_branch>` **exists**|**Subsequent** (prior PR may be open or unmerged)|After push: **view** the PR for the current branch and **update** its description (§ 7); do not open a new PR unless none exists (deferred first open)|

**Subsequent run** (open PR not merged, or branch reused):

1. **Scope** — read `Dedrift-Baseline:` from the tip commit on `origin/<pr_branch>` (§ 3); collect default-branch commits since that SHA. Never use the PR description for baseline or scoping.
2. **Detect and write** — same as first run; reconcile clear **stale spec** into main specs on `<pr_branch>`.
3. **Record this run** — review artifact (§ 8) and PR body (§ 7) both carry capabilities reconciled, **needs human decision**, unmapped paths, and format- and dedrift-commit stats for this run.
4. **Commit and push** — checkout `<pr_branch>`, commit spec writes, append baseline trailers (§ 9), push.
5. **Update PR description** — after push, **view** the pull request for **this branch** and **update** its description with the § 7 template (host CLI, MCP, or API — whichever is available). When none exists (deferred first open), **open** one once. Replace the body wholesale; do not merge into the old description.

**Why not repo-wide PR search:** scoping baseline lives in git commit trailers, not PR metadata. Listing or searching all pull requests is unnecessary for branch-scoped view/update and tends to invite poll loops (open/draft/CI waits). One branch lookup and one description update per run is enough.

**First open** — when `origin/<pr_branch>` was missing at run start: create branch from `origin/<default-branch>`, push, then open PR once (§ 7).

## 3 — Collect scope and baseline

1. `git fetch origin` when network is available.
2. Resolve default branch: host overlay `default_branch` when set; else parse the branch name from `git symbolic-ref refs/remotes/origin/HEAD` by stripping the `refs/remotes/origin/` prefix (e.g. `refs/remotes/origin/master` → `master`, `refs/remotes/origin/release/2026` → `release/2026`). Fallback order when `origin/HEAD` is unset: `main`, then `master`.
3. Resolve baseline SHA for scoping default-branch changes (**git only — never from the pull request description or hosting PR APIs**):
   - When `origin/<pr_branch>` exists: `git log -1 --format=%B origin/<pr_branch>` and parse the commit-message trailer `Dedrift-Baseline: <sha>` (see § 9). Ignore `Dedrift-Baseline-At:`.
   - When the branch is missing, the trailer is absent, or `<sha>` is empty: treat as missing baseline.
   - When missing baseline: leave `<baseline-sha>` empty and use step 4 **First run / missing baseline** path collection only — do not synthesize a baseline from `git log -1` on the default branch (that can miss earlier commits in the window).
4. Collect changed paths on the default branch since baseline:
   - **Preferred (baseline SHA set):** walk commits with `git log <baseline-sha>..origin/<default-branch> --format=%H` (do not use a flat `git diff --name-only` for scoping — per-commit classification in step 5 requires commit subjects).
   - **First run / missing baseline:** walk commits with `git log origin/<default-branch> --since="<delta_window>" --format=%H` (default `delta_window`: `3 days ago`). `delta_window` MUST be a Git approxidate relative expression (e.g. `3 days ago`, `2 weeks ago`); bare suffix forms such as `24h` are not parsed by Git and MUST NOT be used.
5. **Commit classification** — for each commit SHA from step 4:
   - Read subject: `git log -1 --format=%s <sha>`.
   - List paths: `git diff-tree --no-commit-id --name-only -r <sha>`.
   - **Format-only commit** — when **either**:
     - the subject matches any host `format_commit_patterns` entry (default below) case-insensitively as a substring, **or**
     - the commit has path changes but **no substantive diff**: parent exists (`git rev-parse <sha>^` succeeds) and `git diff <sha>^..<sha> -w --quiet` succeeds (whitespace-only / formatting / line-ending churn).
       Treat as **format-only** and **drop all paths** from this commit before merging into the scope set. Formatting sweeps do not imply behavioral drift.
   - **Dedrift commit** — when not format-only and the subject matches any host `reconcile_commit_patterns` entry (default below) case-insensitively as a substring: treat as **dedrift output** and **drop all paths** from this commit before merging into the scope set. Prior dedrift reconciliations (specs, skills, prompts) do not imply new implementation drift.
   - **Regular commit** — otherwise keep all paths.
   - Default `format_commit_patterns` (host overlay may extend or replace):
     - `prettier`
     - `csharpier`
     - `format markdown`
     - `format check`
     - `formatting`
     - `line ending`
     - `prettierignore`
   - Default `reconcile_commit_patterns` — dedrift-output subjects; host overlay may extend or replace:
     - `[dedrift]`
     - `chore(dedrift):`
     - `Reconcile spec drift`
     - `Reconcile spec`
   - Record format-commit count, dedrift-commit count, and excluded paths in the review artifact (§ 8) and PR **Run metadata** (§ 7).
   - Retain the merged path set from regular commits as **implementation paths** for § 6a overview detect when `project_overview.enabled` is true.
6. When no changed paths remain after step 5: write summary artifact, exit **no-drift** — no branch, no PR. Does not update baseline (§ 9); the next run scopes via `delta_window` until `<pr_branch>` stores a baseline trailer.

## 4 — Map paths to capabilities

|Path pattern|Capability|
|-|-|
|`openspec/specs/<capability>/**`|`<capability>`|
|`<skill-root>/tacos-<name>/**`|Align `tacos-<name>` to spec; else host `skill_map`|
|`dev/**`|`tacos-doctor-install-update`|
|`openspec/schemas/**`|`tacos-doctor-install-update`|
|`openspec/tacos.yaml`|`post-artifact-orchestration`|
|Unmapped paths|Triage per § 4a when not host-excluded; otherwise record under **Unmapped paths (skipped)** with no spec writes|

Host `path_map` overrides rows above for matching prefixes.

Resolve `<skill-root>` per the host skill install layout (see host `AGENTS.md` or install docs when present). Host overlay `skill_map` maps skill folder names to capability ids when the folder name does not align with a spec capability id.

Merge host `path_map` and `skill_map`. Deduplicate into capability scope from mapped paths.

**Unmapped paths** — implementation paths (§ 3) with no capability after mapping and not stripped by host exclude rules: **always** run § 4a when any remain, **whether or not** capability scope is already non-empty.

After § 4a re-map: when capability scope is still empty, skip § 5–6; overview reconcile may still apply at § 6a when `project_overview.enabled` is true.

When **implementation paths** from § 3 are empty (before or after exclusion), write summary artifact per § 8, exit **no-drift**.

## 4a — Unmapped path triage (host overlay)

When § 4 yields **unmapped** paths from **implementation paths** (§ 3) that are **not** stripped by host overlay **intentionally unmapped** rules (`exclude_path_prefixes`, exact-path skips, tooling-only classification):

1. **Cluster** paths by prefix (e.g. `<skill-root>/<name>/`, `openspec/specs/<cap>/`, `dotnet/src/<area>/`, `docs/<area>/`).
2. **Analyze** each cluster against the codebase and `openspec/host/dedrift-job.md` (`path_map`, `skill_map`, governed capability list when present):
   - **Governed host skill** — folder under `<skill-root>/` with a behavioral spec at `openspec/specs/<capability>/spec.md` and not a `tacos-*` vendor skill → add `skill_map` (`<folder>` → capability id) and `path_map` (`<skill-root>/<folder>/` → capability) when missing; add capability to host governed scope list when maintained.
   - **Existing capability extension** — paths clearly owned by a capability already in governed scope → add or refine `path_map` prefix for that capability.
   - **New governed capability** — new behavioral surface with its own main spec on the default branch → add to governed scope when the host maintains a list; add `path_map` / `skill_map` as needed.
   - **Intentionally out of scope** — vendor tacos tooling, CI/release automation, excluded prefixes → keep under **Unmapped paths (skipped)**; when the same prefix recurs, consider proposing host `exclude_path_prefixes` (record under **Needs human decision** when ambiguous).
   - **Ambiguous fit** — record under **Needs human decision** with proposed overlay change and one-line rationale; no overlay write.
3. **Write** clear-fit updates to `openspec/host/dedrift-job.md` on `<pr_branch>` in the same dedrift commit as spec reconciles (parent agent only when host overlay forbids subagent writes).
4. **Re-map** — after overlay writes, include newly mapped paths in the capability scope for § 5–6 in the **same run**.

**MUST NOT** triage paths already stripped by host exclude rules before stock § 4. **MUST NOT** add `tacos-*` vendor skills to host `skill_map` unless the host overlay explicitly governs them.

Record overlay edits under **Host overlay updated** in the PR body (§ 7) and review artifact (§ 8).

## 5 — Detect drift

Resolve depth — host overlay `scheduled_depth` when set in `openspec/host/dedrift-job.md`; else `dedrift.scheduled_depth` from `openspec/tacos.yaml` (default `standard`). When `deep`, run the parent detect loop per [deep-mode.md](deep-mode.md) ## Scheduled path and ## Parent orchestrator before § 6 — **every pass** fresh Task detect per [delegation.md](delegation.md) ## Deep detect loop; when `standard`, single detect pass below.

For each scoped capability, compare `openspec/specs/<capability>/spec.md` to the codebase for **behavioral** obligations per [modes.md](modes.md) ## Comparison scope.

When scoped count **> 6** and depth is **standard**, batch per [delegation.md](delegation.md). When depth is **deep**, batching applies **inside each loop pass** per delegation ## Deep detect loop — not only on the first pass. Subagents detect only; parent merges. When Task is supported, all detect passes use **`agent-tacos-dedrift-detect`** — no inline parent detect regardless of scoped count.

Classify: **aligned**, **stale spec**, **code violation**, **needs human decision**.

- **stale spec** with clear reconcile direction → write set
- **aligned** → no write
- **code violation** → do not conform; record under **needs human decision** in the review artifact (§ 8)
- **needs human decision** → no write; record in the review artifact (§ 8) and PR body template (§ 7)
- Batch detect failure → report unevaluated capabilities; no partial writes

When spec write set is empty and no spec **needs human decision** items and `project_overview.enabled` is false: write summary artifact per § 8, exit **no-drift**.

When spec write set is empty and one or more spec **needs human decision** items exist: continue through § 6a–6b (when enabled) then § 7 — open or update the PR with **Needs human decision** populated when no writes apply; no main spec writes.

## 6 — Write specs

When the spec write set is empty, skip spec writes. Continue to § 6a when `project_overview.enabled` is true; otherwise evaluate combined **no-drift** per § 6b before § 7.

Apply reconciled edits to main specs in the write set. Write serially on parent when batched. Reconcile may add, change, or remove behavioral obligations to match shipped code. Do not weaken or delete obligations solely to avoid conforming code when the finding is **code violation** — record those under **needs human decision** in the review artifact (§ 8) instead. Then continue to § 6a when `project_overview.enabled` is true; otherwise § 6b.

## 6a — Project overview detect

When `project_overview.enabled` is false → skip § 6a and § 6b; evaluate combined **no-drift** per § 6b before § 7.

When `project_overview.enabled` is true:

1. Resolve `{path}` from `project_overview.path`; default `README.md` at repo root.
2. When `{path}` is missing on disk → record under **Overview needs human decision** (missing file); no write; continue to § 6b.
3. Read full `{path}` on disk.
4. **Inputs** — this run's **implementation paths** from § 3; shipped user-visible surface implied by those paths and the codebase (skills, slash commands, config keys documented for hosts, install or layout behavior per task-stage-contract); post-§ 6 reconciled spec state as context only — do **not** paste `openspec/specs/**` into overview body.
5. When host overlay `## Project overview` defines `overview_sections` and reconcile rules — limit detect and writes to those sections; apply host skip lists.
6. **Detect** — compare onboarding prose in scope to shipped surface. Classify each finding:
   - **missing** — user-visible surface absent from overview (e.g. new slash command with no Skills row)
   - **stale** — overview prose contradicts or omits shipped behavior
   - **aligned** — no change
   - **needs human decision** — ambiguous (intentional minimalism vs drift); no write
7. Clear **missing** or **stale** with reconcile direction → **overview write set**. **needs human decision** → record in review artifact (§ 8) and PR **Overview needs human decision**; no write.

Overview maintenance is **reconcile only** — update `{path}` to match the project. No conform semantics and no code writes for overview drift.

## 6b — Write overview and combined no-drift

When overview write set is non-empty: apply soft edits to `{path}` in the working tree (preserve prose outside scoped sections per host overlay and [overview-guidance.md](../../tacos-project-overview/references/overview-guidance.md)). Stage with any § 6 spec writes for a single § 7 commit when both apply.

**Combined no-drift** — when spec write set is empty, overview write set is empty, and there are no spec or overview **needs human decision** items: write summary artifact per § 8, exit **no-drift** — no branch, no PR.

**Continue to § 7** when any of: spec write set non-empty; overview write set non-empty; spec **needs human decision** items; overview **needs human decision** items (including missing `{path}`).

## 7 — Branch and PR

|Setting|Default|
|-|-|
|`pr_branch`|`dedrift/scheduled`|
|`pr_title`|_(discover from host — step below; overlay may set)_|
|`pr_label`|`tacos-dedrift` (apply only when label exists — see below)|
|Draft|`true`|

**PR label (optional, non-blocking):** when host `pr_label` is set, apply it **only if the label already exists** on the host. **MUST NOT** create labels on scheduled runs (`gh label create` and equivalents are forbidden — integrations often lack permission). Open or update the pull request **without** waiting on label setup. When label apply fails (missing label, 403, or no permission), record `label: skipped (<reason>)` in the review artifact and **continue** — label failure is not a fatal PR error. Prefer opening the PR first, then best-effort label attach when tooling allows.

**PR title (before open):** discover host rules per [tacos-pr PR title conventions](../../tacos-pr/references/pr-title-conventions.md) — read host `README.md` (§ Contributing / pull requests), `AGENTS.md`, `CONTRIBUTING.md`, or PR templates when present. Do **not** browse existing pull requests for title patterns on scheduled runs. When docs are silent, use the scheduled dedrift default below. Host overlay `pr_title` overrides discovery when set.

Scheduled dedrift reconciles specs and skills (no user-facing feature): use the host's maintenance/chore convention — e.g. Conventional Commits `chore(dedrift): reconcile main specs` when the host README requires `type(scope): imperative description`. **MUST NOT** use bracket tags such as `[dedrift]` when the host requires Conventional Commits. Record the chosen title source in the review artifact (one line).

Use the resolved title verbatim when opening a pull request.

**Decision-signal runs (needs human decision only):** When both write sets are empty but § 5 or § 6a recorded **needs human decision** items (spec and/or overview) — checkout `<pr_branch>`, commit **baseline trailers only** (§ 9) with no file changes, push, then open or update the PR with the body template below. Use a dedrift commit subject per host convention (e.g. `chore(dedrift): needs human decision`). The PR signals items requiring human review; advancing baseline prevents re-scoping the same default-branch commits on the next run.

**Overview-only runs:** When spec write set is empty but overview write set is non-empty — checkout `<pr_branch>`, commit overview (and baseline trailers per § 9), push, open or update PR with **Overview reconciled** populated. Same PR branch and flow as spec reconcile.

**Branch reuse (git only):** when `git rev-parse --verify origin/<pr_branch>` succeeds, checkout `<pr_branch>` and commit on it. Otherwise create `<pr_branch>` from `origin/<default-branch>`, commit, push.

**Pull request open and description (after push):**

1. **First open** — when run mode is **first open** (§ 2): open a pull request **at most once** with the body template below (use host PR automation, CLI, or MCP — whichever is available).
2. **Subsequent** — when run mode is **subsequent** (§ 2): view the pull request for the **current branch only**. When one exists, update its description with a fresh body from the template below. When none exists (deferred first open), attempt step 1 once.

If open or update fails (auth, no pull-request permission), record the error in the review artifact and **exit** — do **not** retry in a loop or wait on hosting PR or CI state. Label apply failure alone is **not** an open/update failure. Do **not** cherry-pick or duplicate commits onto another branch to satisfy PR tooling.

**Pull request constraints:** view and update (or open once) the pull request for `<pr_branch>` after push. Do **not** use repo-wide pull request search/list for automation, read baseline from the PR description, or poll open/draft/CI state in a loop.

**PR body template** — compose for every run that opens or updates a pull request. Baseline for the **next** run comes from the commit trailer (§ 9), not from re-reading this body. Do **not** list `artifacts/` paths in the PR body — review artifacts are local-only (gitignored).

```markdown
<!-- tacos-dedrift-baseline
sha: <default-branch-sha>
at: <iso-8601-timestamp>
-->

## Summary

Scheduled dedrift — reconcile. Human review required before merge.

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
```

## 8 — Review artifact

Write `artifacts/openspec-reviews/dedrift-<slug>.md` per [output-format.md](output-format.md). Frontmatter: `dedrift_mode: reconcile`, `run_mode: scheduled`. Mirror the PR body sections (**Capabilities reconciled**, **Needs human decision**, **Overview reconciled**, **Overview needs human decision**, etc.) in the artifact for every run.

## 9 — Baseline

When host `persist_baseline` is true (default): append these trailers to the **dedrift commit message** on `<pr_branch>` (not a separate repo file) — including decision-signal commits with no spec file changes:

```text
Dedrift-Baseline: <default-branch-sha>
Dedrift-Baseline-At: <iso-8601-timestamp>
```

`<default-branch-sha>` is `origin/<default-branch>` HEAD at push time — the scoping anchor for the **next** scheduled run.

Also mirror the same `sha` and `at` values in the `<!-- tacos-dedrift-baseline -->` block when composing the PR body (§ 7).

## 10 — Report

Chat summary: run mode (first open vs subsequent), scope, spec and overview counts by status, branch name, artifact path, unmapped paths. When `project_overview.enabled`, note overview sections reconciled or flagged. Include pull request URL when open or update succeeded; when PR update failed, note that commits were pushed on `<pr_branch>`.
