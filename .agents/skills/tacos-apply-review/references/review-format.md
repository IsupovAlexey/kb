# tacos apply review: format and workflow

Use with `tacos-apply-review` `SKILL.md`. Checklist router: [checklist.md](checklist.md); pass 1: [checklist-pass1.md](checklist-pass1.md); pass 2: [checklist-pass2.md](checklist-pass2.md). Apply gate order: [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md).

## Review dimensions

Pass 1 detail: [spec-compliance-pass.md](spec-compliance-pass.md). Pass 2 uses the rows below and [checklist-pass2.md](checklist-pass2.md).

- **Spec compliance (pass 1)** — requirements, design, tasks; load per Input, cite planning paths in findings ([spec-compliance-pass.md](spec-compliance-pass.md)); cite gaps with references. **Layered:** specs = behavioral SHALL/MUST; design = decisions/deferrals; tasks = stage acceptance. Do not flag BLOCKER/CRITICAL drift solely for presentation/inventory detail in design/tasks when specs state the governing behavioral rule. Flag violations of behavioral obligations or stage acceptance. Do not BLOCKER missing behavior owned by later unchecked stages or explicit deferrals — **Deferred** with target stage citation. **TDD procedure (pass 1):** when `tdd.md` or tacos-work TDD intent applies — batched RED, missing refactor assessment, Green without red paste ([checklist-pass1.md](checklist-pass1.md) ## TDD procedure (pass 1)). **Main-spec drift lane:** after pass 1, detect behavioral drift for implicated capabilities' main specs under `openspec/specs/`; write **## Main-spec drift**; **BLOCKER** when stale spec or code violation and sync-pending does not apply — `stale spec (pending sync)` when **Modified Capabilities** + change delta cover the obligation (not BLOCKER). Distinct from change-folder BLOCKERs. Detail: [checklist-pass1.md](checklist-pass1.md) ### Layered compliance and ## Main-spec drift detect
- **Structural maintainability (BLOCKER)** — duplication (~5+ identical logic lines without domain reason), spaghetti growth, thin wrappers, boundary/type churn, orchestration/atomicity failures, missed code judo, heuristic file decomposition failure — per [structural-maintainability.md](../../tacos-implementation-conventions/references/structural-maintainability.md) (start with [§ Reviewer stance](../../tacos-implementation-conventions/references/structural-maintainability.md#reviewer-stance) for judo search) and [checklist-pass2.md](checklist-pass2.md) ## Structural maintainability (BLOCKER). **BLOCKER** when introduced or left in the stage diff; cite section and concrete remediation
- **Non-code maintainability (BLOCKER)** — when the diff includes skills, schema, or docs: tangled conditional prose, duplicate guidance across references, wrapper skills — per [checklist-pass2.md](checklist-pass2.md) ## Non-code maintainability (BLOCKER). Same **BLOCKER** bar as code; mixed diffs run both structural and non-code sections
- **Conventions (KISS/YAGNI, DRY, SRP)** — [checklist-pass2.md](checklist-pass2.md). **Intra-stage cohesion:** duplicate logic, superseded dead code, naming drift, missed abstractions across tasks in the same stage — [checklist-pass2.md](checklist-pass2.md) ## Intra-stage cohesion. **DRY:** flag duplication and copy-paste that inflate the diff without domain need — keeps apply output minimal and reviewable for human sign-off
- **Host standards** — `AGENTS.md`, `openspec/config.yaml` `context`, host skills in scope
- **Tests** — per host norms in `context` or host docs (when the diff includes testable behavior)
- **TDD compliance** — when `tdd.md` exists or tacos-work TDD intent applies: pass-1 procedure findings in **## TDD compliance** ([checklist-pass1.md](checklist-pass1.md) ## TDD procedure (pass 1)); pass/fail per slice or waiver citation

## Maintainability BLOCKER definitions

Report at **BLOCKER** when the stage diff introduces or leaves unresolved:

|Signal|BLOCKER when|Remediation tone|
|-|-|-|
|Duplication|~5+ identical logic lines without domain reason to keep copies|Consolidate with placement cite — inline on owner when one production consumer; per [authoring.md § Extraction placement](../../tacos-implementation-conventions/references/authoring.md#extraction-placement-language-agnostic) and [§ DRY](../../tacos-implementation-conventions/references/authoring.md#dry-when-to-share-when-to-repeat)|
|Spaghetti|Opaque chains, deep nesting without guards, ad-hoc branches in busy paths|Guard clauses, named steps, or focused extract — not a new service for one call site|
|Thin wrapper|Delegate-only with no added behavior, validation, or boundary translation|Inline at call site or add real boundary behavior|
|Boundary drift|Domain→infrastructure leaks, orchestration in leaf helpers, contract widening|Move to canonical layer; define typed contract|
|Orchestration / atomicity|Gratuitous serialization of independent work; durable updates that can half-apply without recovery|Parallelize independent steps or group writes in one transaction/saga/compensation|
|Missed code judo|New parallel layer when extend-owner or reuse helper is clearly smaller|Extend `Order`-style owner or reuse existing utility — cite simpler path|
|File decomposition|Material diff growth **and** second unrelated concern domain in same file|Concrete split/extract targets (e.g. move persistence to `*-repository`)|

Rubric depth and borderline examples: [structural-maintainability.md](../../tacos-implementation-conventions/references/structural-maintainability.md).

**Sample finding tone (direct remediation):**

```text
BLOCKER | src/OrderService.cs:42 | Thin wrapper — delegate-only create() adds no validation; inline at caller or add boundary behavior per structural-maintainability.md § Thin wrappers
BLOCKER | tacos-foo/SKILL.md | Wrapper skill — only re-exports tacos-bar without procedure; merge into bar or add host-specific rubric
BLOCKER | src/parser.ts:88 | File decomposition — diff adds JSON parsing and SQL mapping in same growing file; extract persistence to order-repository.ts
BLOCKER | src/checkout.ts:120 | Orchestration — saveOrder then reserveInventory with no transaction; group in db.transaction or add compensating reserve failure path per structural-maintainability.md § Orchestration and atomicity
```

## Maintainability readiness invariant

- **Readiness: Not ready** when any maintainability **BLOCKER** is open — including structural and non-code maintainability findings
- Spec and behavioral compliance alone **MUST NOT** yield **Ready** or **Ready after fixes** while any maintainability **BLOCKER** remains
- **Status: NEEDS REVISION** when any blocking **BLOCKER** or **CRITICAL** remains (maintainability, spec compliance, host gates, TDD, etc. per checklist)
- **Status: APPROVE WITH CHANGES** when any open **MAJOR** remains in **Should address (MAJOR)** — orchestrator **MUST** fix before gate pass per [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md)
- **Status: APPROVE** only when no open **BLOCKER**, **CRITICAL**, or **MAJOR** in scope (**MINOR** / **Deferred** OK)
- **Readiness: Ready after fixes** when open **MAJOR** remain; **Readiness: Ready** only with **Status: APPROVE**
- Maintainability **BLOCKER**s MUST be fixed in the **same stage** before the **Human review:** terminal line — no defer-to-later-stage for structural regressions

## Summary ↔ severity alignment (reviewer)

- **MUST NOT** write `Status: NEEDS REVISION` or `Readiness: Not ready` without at least one **BLOCKER** or **CRITICAL** row in **Must address (BLOCKER / CRITICAL)** for each blocking theme — unchecked **Spec compliance** bullets or narrative alone are insufficient
- **MUST** write `Status: APPROVE WITH CHANGES` and `Readiness: Ready after fixes` when any open **MAJOR** remains in **Should address (MAJOR)**; **MUST NOT** use those labels when zero **MAJOR** rows are open
- **MUST** write `Status: APPROVE` and `Readiness: Ready` only when no open **BLOCKER**, **CRITICAL**, or **MAJOR** in scope
- Blocking spec or behavioral gaps → **BLOCKER** or **CRITICAL** row with location and remediation, then matching Summary status
- Orchestrator gate pass and remediation loop: [../../tacos-orchestration/references/review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md)
- Pipeline trace procedure: [pipeline-and-verification.md](pipeline-and-verification.md)

## Severity guide

|Level|Meaning|Action|
|-|-|-|
|BLOCKER|Cannot ship safely|Must fix|
|CRITICAL|Major issues|Should fix before proceeding|
|MAJOR|Rework risk|Should fix soon|
|MINOR|Nice to have|Optional|

## Finding order and cap

BLOCKER/CRITICAL first (maintainability **BLOCKER**s before CRITICAL when both exist), then MAJOR, then MINOR. Soft cap ~15 actionable items; overflow in **Deferred** (themes + counts only).

## Required output format

```markdown
# Apply review: [scope/stage] / [change name]

## Summary

- Status: [APPROVE / APPROVE WITH CHANGES / NEEDS REVISION]
- Readiness: [Ready / Ready after fixes / Not ready]
- Pass 1: [Pass / Fail]
- Pass 2: [Pass / Fail / Skipped — pass 1 BLOCKER/CRITICAL or main-spec drift BLOCKER]
- Implementation changes: [none — no implementation changes were made | brief summary of implementation edits in scope]
- [0-2 sentences]

## Obligation inventory

- _(mandatory pass 1 — list every in-scope obligation before findings)_
- Tasks: _(each implementation checkbox — pass/fail)_
- Requirements / scenarios: _(each implicated `### Requirement:` / `#### Scenario:` — pass/fail)_
- Verify decisions: _(each **Verify Decision N** — pass/fail/N/A)_
- Scope / non-goals: _(material boundaries — pass/fail)_

## Pass 1: Spec compliance

- Result: [Pass / Fail]
- [ ] … traceability and layered compliance bullets …
- Layered notes: _(when specs are behavioral-only — detail from design/tasks vs behavioral gaps)_ …

## Main-spec drift

- Detect scope: [capabilities implicated by diff]
- [ ] `<capability>` — aligned | stale spec | stale spec (pending sync) | code violation | needs human decision
- Result: [Pass / Fail]
- _(sync-pending: main lags change delta listed under **Modified Capabilities** — informational; MUST NOT add **Must address** BLOCKER)_
- _(When Result is Fail: open **BLOCKER** from code violation or stale spec without sync-pending — Summary **Status** MUST be NEEDS REVISION or APPROVE WITH CHANGES; **Readiness** MUST NOT be Ready; add BLOCKER rows in **Must address**; **Pass 2** MUST be Skipped with reason citing main-spec drift)_

## Pass 2: Code quality

- Result: [Pass / Fail / Skipped]
- _(when skipped: one-line reason — pass 1 had open BLOCKER/CRITICAL, or **Main-spec drift** lane has BLOCKER)_
- **Intra-stage cohesion:** [Pass / Fail / N/A] — duplicate logic, dead code, naming drift, missed abstractions across stage tasks

## TDD compliance

- _(when `tdd.md` or tacos-work TDD intent — omit otherwise)_
- [ ] Per-slice pass/fail or waiver citation …

## Pipeline trace

- _(when planning documents pipeline/gate ordering or **Verify Decision N** rows exist and diff touches entrypoint — else `N/A`)_
- Entrypoint: …
- Planned order: …
- Actual order: …
- Result: pass | fail — …

## Must address (BLOCKER / CRITICAL)

- [Finding] | [location] | [action]

## Should address (MAJOR)

- …

## Optional (MINOR)

- …

## Conventions (KISS/YAGNI, DRY, SRP)

- …

## Non-code maintainability

- _(required when diff includes skills, schema, or docs — omit only when diff is code-only)_
- [ ] Tangled conditional prose — pass/fail or N/A
- [ ] Duplicate guidance across references — pass/fail or N/A
- [ ] Wrapper skills — pass/fail or N/A

## Tests

- …

## Deferred

- _(only if over cap)_

## Skipped additional skills

- _(parent merge only — omit section when empty)_
- `<skill-path> | <reason>` e.g. `no matching diff paths`

## Parallel delegation warnings

- _(parent merge only — omit section when empty)_
- `<skill-path> | <one-line error summary>`
```

Clean APPROVE with no BLOCKER/CRITICAL/MAJOR: keep Summary to one line; other sections "None" or "N/A".

**Parent merge:** Include **`## Skipped additional skills`** and **`## Parallel delegation warnings`** only when non-empty. **Child quick-exit:** When `tacos-additional-apply-review` finds zero matching paths after spawn, note `N/A — no matching diff paths` under **Optional (MINOR)** or in **Summary** (not a separate severity level).

## Workflow

1. Inputs: change name, stage/diff scope, output path, prior `apply-review-*.md` if re-review; delegation mode from caller ([host-additional-skills.md](host-additional-skills.md)) — **parallel core-only** (do **not** read `review.apply_review_additional_skills`), **re-review parallel** (match initial pass when concurrent spawn supported) or **re-review sequential fallback** / **deep sequential** (one fresh core child reads array in order after core dual-pass), or **parent merge** (orchestrator or manual parent after parallel children return)
2. Read change context (proposal, specs, design, tasks)
3. Read diff/files in scope
4. **Pass 1** — assess per [spec-compliance-pass.md](spec-compliance-pass.md) and [checklist-pass1.md](checklist-pass1.md) ## Pass 1; pipeline trace when applicable. Run **## Main-spec drift** next; stop pass 2 if pass 1 or drift lane has open BLOCKER/CRITICAL.
5. **Pass 2** — when pass 1 passes, assess [checklist-pass2.md](checklist-pass2.md) and load [tacos-implementation-conventions](../../tacos-implementation-conventions/SKILL.md) when narrative depth is needed
6. Classify severity; apply cap; enforce [Maintainability readiness invariant](#maintainability-readiness-invariant)
7. Write to caller path (e.g. `artifacts/openspec-reviews/<change>/apply-review-1.md`) unless the caller is the parent merging parallel children

## After fixes (re-review)

**Mandatory** when orchestrator fixes follow a failing rN — parent self-report ("fixed", "BLOCKERs resolved") **does not** pass the gate. See [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) ## Anti-short-circuit, **Same-turn STOP**, and **Dynamic re-review checkbox**.

Parent orchestrator **MUST** append **Re-review after fixes** per review-gate-pass **Dynamic re-review checkbox** before fixes when the active checklist exists. Record [Turn-summary delegation record](../../tacos-orchestration/references/review-gate-pass.md#turn-summary-delegation-record) before gate checkoff.

Delegate via **fresh** Task subagent or Task tool — **not** inline in the thread that applied fixes. **Forbidden:** parent-authored `apply-review-*-rN.md`. Load prior `apply-review-<stage>.md` (or `-rN`) and new diff. Confirm resolutions; flag regressions. Write `apply-review-<stage>-r2.md` (increment as needed). Orchestrator reads **latest** Summary for gate pass only.
