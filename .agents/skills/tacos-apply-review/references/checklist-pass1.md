# tacos apply review — pass 1 (spec compliance)

Use with `tacos-apply-review` `SKILL.md` step 3. Router: [checklist.md](checklist.md). Pass 2: [checklist-pass2.md](checklist-pass2.md) after pass 1 and main-spec drift are clean.

For narrative rubric detail on pass 1, read [spec-compliance-pass.md](spec-compliance-pass.md).

## Work binding (`tacos-work`)

When the review is for a **tacos-work** session (not `openspec/changes/<name>/`):

- [ ] Loaded `artifacts/tacos-work/<slug>/tasks.md` (Intent, Planning, ## Work) — not change-folder planning artifacts
- [ ] **Verify Decision N** rows under **## Work** treated as primary acceptance scope ([pipeline-and-verification.md](pipeline-and-verification.md))
- [ ] Behavioral compliance checked against touched `openspec/specs/**` and the work plan / spec-touch lines in `tasks.md`
- [ ] Did not BLOCKER solely because `proposal.md`, change-folder delta specs, or `design.md` under `openspec/changes/` are absent
- [ ] Output path is `artifacts/openspec-reviews/<slug>/apply-review.md` unless caller documents a different work path

## Pass 1 — Spec compliance

Run first. Rubric: [spec-compliance-pass.md](spec-compliance-pass.md). **MUST NOT** evaluate naming, comments, structural maintainability, KISS/YAGNI, DRY, SRP, or prose-quality slop in this pass.

- [ ] **Obligation inventory** — enumerated every in-scope task row, requirement, scenario, **Verify Decision N**, and material non-goal before findings; recorded under **## Obligation inventory**; assessed each line pass/fail ([spec-compliance-pass.md](spec-compliance-pass.md) step 2)
- [ ] Implements requirements and scenarios from proposal/specs/design (full tacos) or main `openspec/specs/**` + work `tasks.md` plan (tacos-work)
- [ ] Traceable to `tasks.md`; no untracked scope creep
- [ ] Key design decisions reflected in the diff (code, config, skills, or docs as applicable)

### Layered compliance

Evaluate the diff against a **layered** planning set:

- **Proposal** — motivation, capability deltas, and coarse impact only (thin; no path laundry or behavioral SHALL/MUST)
- **Specs** — behavioral invariants and obligations (SHALL/MUST)
- **Design** — technical decisions, deferrals, and architecture constraints
- **Tasks** — change-scoped acceptance criteria for the stage under review

- [ ] Does not BLOCKER/CRITICAL solely because the diff implements presentation or inventory detail documented in design/tasks but omitted from specs, when specs state the governing behavioral rule
- [ ] Does not BLOCKER/CRITICAL solely because the diff follows procedure (paths, pipeline, templates) documented in design/tasks or skill references but omitted from specs, when specs state the governing behavioral obligations
- [ ] Does not BLOCKER/CRITICAL solely because `proposal.md` omits paths, routes, or implementation detail when design and tasks for the stage cover that detail
- [ ] Does not BLOCKER/CRITICAL solely because the diff matches design/tasks acceptance without repeating proposal What Changes bullets in the diff narrative
- [ ] Flags drift when implementation violates a behavioral obligation in specs or acceptance in tasks/design for the stage
- [ ] **BLOCKER** when planning requires reject-before-I/O but the diff runs dependency I/O before the gate (including pre-existing loops); grill/planning trump narrow design method lists — do not waive as layered note
- [ ] Does not **BLOCKER** / **CRITICAL** for missing behavior owned by unchecked implementation tasks in a later `## N.` stage, or explicitly deferred to a named later stage in `design.md` / `tasks.md` for the active change — record under **Deferred** with target stage/task citation; still **BLOCKER** when current stage tasks, **Verify Decision N**, or design for **this** stage require the behavior now
- [ ] When implementation matches intended behavior but canonical main specs under `openspec/specs/` are stale or conflict for the same behavioral obligations (outside change-folder scope), flag **BLOCKER** in **## Main-spec drift** with reconcile or conform remediation — except **sync-pending** per [## Main-spec drift detect](#main-spec-drift-detect) below
- [ ] When `review.apply_review_additional_skills` is empty and the diff touches paths that would match a host review rubric, note `/tacos-host-skill` in the artifact (SUGGESTION) — bootstrap host-local review skills then `/tacos-doctor update`
- [ ] Staged `## N.` apply review uses pending tasks in that stage (and referenced design decisions) as primary acceptance scope for change-specific detail; specs for cross-cutting behavioral invariants
- [ ] **Planning traceability** — load context per [SKILL.md](../SKILL.md) ## Input; cite planning file paths in pass-1 findings; do not rely on conversation memory as the sole source — re-read artifacts when stale, on re-review, or when uncertain

When pass 1 has open **BLOCKER** or **CRITICAL**, still run [## Main-spec drift detect](#main-spec-drift-detect), then stop before pass 2 ([checklist-pass2.md](checklist-pass2.md)).

## TDD procedure (pass 1)

Run during [## Pass 1 — Spec compliance](#pass-1--spec-compliance) when `tdd.md` exists (full tacos) or tacos-work TDD intent applies. Procedure rules: [red-green-refactor.md](../../tacos-tdd/references/red-green-refactor.md).

- [ ] **Batched RED** — multiple **Red** rows checked or multiple failing tests introduced before first **Green** / VERIFY-GREEN for a slice → **BLOCKER** (per-test atomic cycle violated)
- [ ] **Missing refactor assessment** — **Green** or **Refactor** checked without recorded per-test refactor outcome (action or explicit skip-with-reason) per slice → **BLOCKER**
- [ ] **Green without red paste** — **Green** checked without chat-visible failing-test output when Red was required → **BLOCKER** unless waiver recorded (`grill-summaries.md` ## apply for full tacos; session `tasks.md` per red-green-refactor for tacos-work)
- [ ] Task checkoffs follow **Red** → **Green** → **Refactor** order per slice; no **Green** before its **Red** done-when
- [ ] No production-only behavioral slice without a preceding **Red** task row for that slice

Record outcomes under **## TDD compliance** in the review artifact (pass/fail per slice or waiver citation). Omit when TDD does not apply.

## QA test plans (pass 1)

When the stage diff touches `openspec/test-plans/**` during eng **apply** (or POST-ARTIFACT for the same change):

- [ ] **Eng mutation of QA test plans** — any create, edit, or delete under `openspec/test-plans/` by eng tacos skills during apply → **BLOCKER** (test plan packs are QA-owned; updates via `/tacos-test-plans` on a QA branch with replace approval)
- [ ] **Retrofit scenario to match code** — diff changes `TC-*` expected results or scenario text to make failing Red/Green pass without a documented QA waiver → **BLOCKER**
- [ ] **Read-only pass** — when the change only reads test plans for Red input and makes no edits under `openspec/test-plans/`, record pass under **## TDD compliance** or note N/A when TDD does not apply

Omit when the stage diff has no `openspec/test-plans/` paths.

## Main-spec drift detect

Run after [## Pass 1 — Spec compliance](#pass-1--spec-compliance) rows, before pass 2. Compare each implicated capability's main spec under `openspec/specs/<capability>/spec.md` to the stage or session diff (behavioral obligations only).

- [ ] Artifact includes **## Main-spec drift** with per-capability status: aligned | stale spec | stale spec (pending sync) | code violation | needs human decision
- [ ] **Full tacos (change folder):** before **BLOCKER** for stale main spec, check `proposal.md` **Modified Capabilities** and a matching change delta under `openspec/changes/<change>/specs/<capability>/spec.md` (or cross-capability MODIFIED requirement in another listed delta per proposal). When both hold and the delta addresses the behavioral obligation that differs from main, record `stale spec (pending sync)` — informational only; MUST NOT add **Must address** BLOCKER (sync/archive merges deltas to main)
- [ ] **BLOCKER** when main spec states a behavioral obligation that conflicts with shipped implementation and sync-pending does not apply — stale spec without listed delta, or code violation
- [ ] **tacos-work:** when session has no active change folder, stale main vs implementation without listed **Spec touch** paths remains **BLOCKER** as before
- [ ] **Pass** when all implicated capabilities are aligned or sync-pending only (no code violation)
- [ ] MUST NOT downgrade change-folder behavioral BLOCKERs because main specs could be reconciled instead

When this section has open **BLOCKER** (excluding sync-pending-only), stop — record **Pass 2: Skipped** and fail Summary readiness.
