# Planning review dimensions

Full normative rubric for `tacos-spec-review` on every pass (POST-ARTIFACT, manual `/tacos-spec-review`, delta-r2, and deep). POST-ARTIFACT gate order: [post-artifact-index.md](../../tacos-orchestration/references/post-artifact-index.md) ## Step order (planning turn).

**Challenger mandate:** Planning spec review runs with an explicit challenger mandate — find what the author did not think to ask, what implicit decisions remain open, and what breaks; not evaluate whether the proposed solution is good or help the bundle look complete. Operate in a fresh bounded context separate from the authoring orchestrator thread when Task is supported.

**Load:** Always load this file on every planning review pass. Do not defer rubric sections to delta-r2 or deep-only loads.

## Review dimensions

- **Completeness** — no TBD placeholders; required sections present; on a planning bundle, **behavioral** obligations (SHALL/MUST) in specs, with presentation/instance detail deferrable to design/tasks only per [Cross-artifact completeness](#cross-artifact-completeness) — do not treat design/tasks alone as satisfying a missing behavioral obligation in specs
- **Open questions** — unresolved items with concrete next actions
- **Weak points / risks** — ambiguities, edge cases, ordering risks (design narrowing grill pipeline obligations to anchor method names without hot-path inventory)
- **Actionability** — bundle is implementable from proposal + specs + design + tasks together; do not require every detail in specs when design/tasks carry change-scoped acceptance (see [Cross-artifact completeness](#cross-artifact-completeness))
- **Consistency** — aligned terminology across artifacts in scope
- **Grill alignment** — when `grill-summaries.md` exists and `grill.<phase>` is `complete` for the scope under review, planning artifacts **MUST** reflect resolved **Decisions** and **User inputs** for that phase (and cross-phase planning grill outcomes on a bundle pass). `skipped` grill or empty phase sections → note N/A, do not require alignment. `## explore` is not binding — only grilled phase sections count
- **Complexity & split** (full planning set or batch) — level Low/Med/High, why, and delivery-time recommendation: one change with post-implementation `/tacos-slice-pr` when Med/High and multiple logical layers (review passes on **one** merge PR, not multiple trunk merges); or multiple OpenSpec changes + merge order when work should be planned separately. Do **not** require **Planned PRs** in `proposal.md` — split is assessed at delivery, not as a planning checklist column. Nudge only — do not block on complexity alone. Do **not** recommend stacked merge PRs or per-slice branches to trunk
- **Security / privacy** (when relevant) — auth, personally identifiable information (PII), exports, external APIs; not a full audit

## Intent fidelity

Assess whether the planning bundle matches the user's original ask and grill scope contract — not only internal artifact coherence.

**Flag MAJOR** when:

- `proposal.md` lists a new or modified capability with no trace to grill scope-contract **User inputs** or the user's original ask
- `design.md` introduces a new service or major abstraction without a **Decision** when grill **User inputs** document a simpler acceptable path
- scope inflation: artifacts deliver materially more than grill **In scope** / scope-contract **User inputs** without justification

**Flag BLOCKER** when:

- `design.md` introduces a new service or major abstraction without a **Decision** and grill **User inputs** explicitly document a simpler acceptable path that the bundle ignores

**Assess:** scope fidelity (in / out / adjacent vs grill **User inputs**); adjacent-feature detection (capabilities or requirements with no scope-contract trace); minimal-default over-build (simplest version from grill vs design complexity).

#### Scenario: Adjacent capability without scope trace

- **WHEN** `proposal.md` **Capabilities** adds a capability with no grill scope-contract **User input** trace
- **THEN** spec review reports MAJOR under **Intent fidelity** naming the capability and missing trace

#### Scenario: Over-build vs simplest path

- **WHEN** grill **User inputs** document a simpler acceptable path and `design.md` adds a new service without a **Decision** citing that path
- **THEN** spec review reports MAJOR or BLOCKER under **Intent fidelity** citing the simpler path

## Implicit branch coverage

Assess whether behavioral obligations trace through material failure, validation, and partial-completion branches — not only happy paths.

**Flag MAJOR** when:

- a spec requirement or scenario names validate, authorize, obtain, or process behavior without a corresponding mid-flow failure or recovery outcome when the flow is multi-step
- grill **User inputs** or design **Decisions** imply permission denied, validation failure, or dependency unavailability but specs contain only happy-path scenarios for that requirement
- the planning bundle relies on an unstated assumption material to delivery (neither grilled in **User inputs** nor specified in artifacts)

**Flag BLOCKER** when:

- grill **User inputs** or design **Decisions** imply a material error class (permission denied, validation failure, dependency unavailability) but specs contain only happy-path scenarios for the governing requirement

#### Scenario: Validate without mid-flow failure outcome

- **WHEN** a requirement names validate or authorize behavior in a multi-step flow without mid-flow failure or recovery outcome
- **THEN** spec review reports MAJOR under **Implicit branch coverage** naming the open branch

#### Scenario: Happy-path-only for material error class

- **WHEN** grill **User inputs** imply validation failure handling but specs contain only happy-path scenarios
- **THEN** spec review reports MAJOR or BLOCKER under **Implicit branch coverage**

## Traceability

Planning review treats untraced behavioral obligations as review failures on every pass.

**Flag BLOCKER** (unless waived in chat) when:

- a net-new `### Requirement:` has no trace to a grill **Decision** or **User input**
- any behavioral obligation in scope lacks grill or scope-contract trace (stricter than CRITICAL-only deferral)

**Flag MAJOR** when:

- a capability delta in `proposal.md` **Capabilities** has no scope-contract trace in grill **User inputs**
- requirement sprawl: many narrow requirements for a bounded change without justification per existing requirement-budget guidance

**Pass** when each net-new requirement cites or clearly implements a grill **Decision** or scope-contract **User input** — record under **Grill alignment** or **Intent fidelity**.

#### Scenario: Untraced requirement blocks gate pass

- **WHEN** a delta spec adds a `### Requirement:` with no grill trace
- **THEN** spec review reports BLOCKER under **Traceability** and Summary MUST NOT be **APPROVE** + **Ready** until remediated, waived, or traced

#### Scenario: Traced requirement passes

- **WHEN** each net-new requirement cites or clearly implements a grill **Decision** or scope-contract **User input**
- **THEN** traceability review records pass for those requirements

## Cross-artifact completeness

When delta specs follow **behavioral-only** authoring (SHALL/MUST invariants, scenarios for distinct behavioral paths), treat **design.md** and **tasks.md** as authoritative for change-specific implementation detail.

**Do not BLOCKER** for missing presentation inventories, per-instance enumerations, or visual tokens in specs when:

- design documents an external deferral or technical decision covering that detail, and
- tasks include traceable acceptance for the change.

**Still BLOCKER** on missing behavioral obligations, ambiguous SHALL/MUST, contradictions between specs/design/tasks, thin tasks with no deferral when specs omit detail the bundle must cover, or when grill/planning requires a control before external I/O but design names only a subset of calls without ordered hot-path inventory (grill **User inputs** override narrow method lists).

**Do not BLOCKER** for missing **implementation procedure** in specs (workflows, pipeline order, artifact paths, checklist templates, skill file layout) when design.md and tasks.md (and applicable skill references named in design) cover that detail for the change.

**Flag MAJOR** when specs **duplicate** procedure already specified in design/tasks (runbook repeated as requirements — maintenance noise). Recommend fewer, broader requirements and move steps to design/tasks.

**Flag CRITICAL** when a `### Requirement:` is **negative-only**: the title names an avoided state ("No …", "Without …", "Does not …", "Chat-only …" as exclusion framing), or the body has no positive SHALL/MUST—only MUST NOT / SHALL NOT laundry. Recommend merge into a positive requirement, move to design **Boundaries** (≤3 brief lines), or delete.

**Flag MAJOR** when:

- a requirement only restates design scope limits, scope framing, or "MUST NOT modify/unship X" with no positive obligation that could be violated (move negative scope to design **Boundaries** or delete; do not park exclusion laundry in proposal; delete release-milestone scenarios such as "WHEN a release ships");
- a scenario's **THEN** asserts only non-occurrence (no files created, gate did not invoke skill, subsystem untouched) without a positive user-visible outcome—recommend a positive WHEN/THEN or delete the scenario;
- grill **Decisions** appear as a 1:1 set of narrow requirements (especially skills/tooling)—recommend merging per schema requirement budget;
- the same orchestration non-invocation guardrail is duplicated across capabilities (keep in [orchestration-binding.md](../../tacos-orchestration/references/orchestration-binding.md) or the owning skill once).

**Actionability** on a planning bundle: rate whether the **combined** proposal, specs, design, and tasks set is implementable — not whether specs alone contain every detail. Note when detail intentionally lives in design/tasks or skill references.

#### Scenario: Deferred presentation not a BLOCKER

- **WHEN** a spec states a behavioral rule without hex colors and design defers colors to Figma with tasks referencing verification against Figma
- **THEN** spec review does not BLOCKER the spec for missing colors

#### Scenario: Behavioral gap remains BLOCKER

- **WHEN** a spec omits an error or permission behavior that design and tasks do not cover
- **THEN** spec review raises BLOCKER for incomplete behavioral coverage

#### Scenario: Thin tasks without deferral is BLOCKER

- **WHEN** a spec omits presentation or instance detail and design has no deferral or decision, and tasks lack concrete acceptance for that detail
- **THEN** spec review raises BLOCKER for incomplete bundle coverage (actionability failure), not approval to inflate the spec with inventories

#### Scenario: Deferred procedure not a BLOCKER

- **WHEN** specs state behavioral obligations only and design plus tasks document pipeline order, paths, and checklist shape for the change
- **THEN** spec review does not BLOCKER the spec for missing procedural steps

#### Scenario: Duplicated procedure is MAJOR

- **WHEN** specs enumerate workflow steps or paths that design.md already specifies for the same change
- **THEN** spec review reports MAJOR procedure bloat and recommends consolidating into design/tasks

#### Scenario: Negative-only requirement is CRITICAL

- **WHEN** a spec includes `### Requirement: No tacos.yaml configuration` or similar with only MUST NOT clauses and no positive SHALL/MUST
- **THEN** spec review reports CRITICAL and recommends a positive requirement title, design **Boundaries**, or deletion

#### Scenario: Absence-only scenario is MAJOR

- **WHEN** a scenario's THEN is only "no new artifact files are created" or "orchestration MUST NOT invoke X" without a positive user-visible outcome
- **THEN** spec review reports MAJOR and recommends a positive WHEN/THEN or routes the guardrail to the owning skill once

#### Scenario: Grill decision sprawl is MAJOR

- **WHEN** a skill capability has one narrow requirement per grill **Decisions** bullet (output contract, config, non-goals) instead of merged behavioral requirements
- **THEN** spec review reports MAJOR and recommends merging obligations and moving exclusions to design **Boundaries**

## Cross-artifact distinctness

When reviewing a planning bundle, enforce the four-layer content ownership matrix in [planning-artifact-loop.md](../../tacos-orchestration/references/planning-artifact-loop.md) ## Content ownership. Each layer adds **delta only** — intentional downstream reference (Requirement title, Decision N) is not overlap.

**Flag MAJOR** when the same content appears in more than one artifact — name the primary artifact and recommend consolidating upstream. Common cases:

- proposal **What Changes** duplicated in design **Decisions** or tasks checkboxes
- behavioral SHALL/MUST or scenario prose copied from specs into proposal, design, or tasks
- design procedure or path lists duplicated verbatim in tasks without terse traceability
- tasks re-stating proposal motivation or design decision rationale as obligation prose

**Still BLOCKER** only for contradictions between artifacts, missing behavioral obligations the bundle cannot implement, or bundle actionability failure — not for consistent overlap alone.

**Positive framing (MAJOR):** Report MAJOR when proposal uses out-of-scope exclusion walls or any scope-limit bullets (move to design **Boundaries** when non-obvious); report MAJOR when design lacks Goals-first framing, uses exclusion walls, or includes more than three brief **Boundaries** lines. Recommend consolidating upstream into design **Boundaries** or deleting redundant negatives.

#### Scenario: Overlap across proposal and design

- **WHEN** proposal What Changes and design Decisions state the same API shape or subsystem list
- **THEN** spec review reports MAJOR and recommends keeping implementation detail in design only

#### Scenario: Verbatim spec copy in tasks

- **WHEN** tasks checkboxes restate SHALL/MUST obligation text from specs instead of citing Requirement title
- **THEN** spec review reports MAJOR and recommends title-level traceability only

#### Scenario: Positive framing in design

- **WHEN** design uses a long exclusion list or more than three out-of-scope bullets
- **THEN** spec review reports MAJOR and recommends Goals-first framing with brief Boundaries only

#### Scenario: Overlap alone is not BLOCKER

- **WHEN** proposal and design repeat the same delivery outcome but specs, design, and tasks remain implementable with no contradiction
- **THEN** spec review reports MAJOR with guidance to consolidate upstream — not BLOCKER solely for overlap

## Prose quality

When reviewing planning artifacts, flag LLM filler per [artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md) (slop patterns and scope naming). Each finding MUST use a canonical pattern id from that reference and include a **concrete rewrite suggestion** (e.g. delete the lead sentence, replace marketing label with a factual bullet) — not pattern id alone. Prose slop alone is **not** BLOCKER unless it obscures bundle actionability.

**Flag CRITICAL** when clustered slop or evaluative/marketing padding hides what the change actually does (actionability failure).

**Flag MAJOR** when:

- a sentence is an evaluative predicate lead removable with zero information loss
- imagined contrast argues against a non-claim ("not just X, but Y"; "X is not Y; X is Z") outside a real Decision alternative
- false agency ("the system wants", "the document understands")
- marketing quality labels without measurable behavior (robust, scalable, enterprise-grade, seamless, …)
- nominalization chains where verbs would be clearer
- drama intensifiers without a named failure mode (crucial, dangerous blind spot, …)
- `scope-version-shorthand` — `v1` / `v2` labels on API surface, endpoints, reference titles, or delivery scope where no external version exists; recommend plain in/out-of-scope prose per [artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md) § Scope naming

**Specs note:** normative SHALL/MUST is required — flag slop in requirement **bodies**, not normative keywords.

#### Scenario: Evaluative lead is MAJOR

- **WHEN** design Goals opens with "The ownership model is clear." before listing owners
- **THEN** spec review reports MAJOR `evaluative-lead` and recommends deleting the lead sentence

#### Scenario: Marketing padding in proposal is MAJOR

- **WHEN** proposal What Changes uses "robust, scalable foundation" without behavioral detail
- **THEN** spec review reports MAJOR `marketing-label` and recommends stating the concrete outcome

#### Scenario: Slop cluster obscures actionability is CRITICAL

- **WHEN** design Decisions are mostly quality labels and contrast rhetoric with no concrete APIs, modules, or trade-offs
- **THEN** spec review reports CRITICAL and recommends rewriting with facts per [artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md)

#### Scenario: Scope version shorthand is MAJOR

- **WHEN** design Boundaries or a Decision titles an internal API slice as "API v1 surface" or tasks say "v1 endpoints" without citing a vendor version
- **THEN** spec review reports MAJOR `scope-version-shorthand` and recommends naming included endpoints and deferred work plainly (e.g. gate + issues only; coverage tree and duplications deferred)

## Artifact-specific notes

- **proposal** — Why, What changes, Capabilities, Impact — motivation and capability deltas only; no path lists, routes, or behavioral SHALL/MUST; coarse subsystem names in Impact only; no out-of-scope bullet walls — park non-obvious exclusions in design **Boundaries**; MAJOR when duplicating design decisions or spec obligations; MAJOR on prose slop per [Prose quality](#prose-quality)
- **specs** — `### Requirement:` + `#### Scenario:`; positive behavioral SHALL/MUST and invariant scenarios (not inventories, runbooks, or negative-only requirements); few broad requirements for skills/tooling; MUST NOT only as one inline guardrail per positive requirement; no absence-only scenarios; no grill 1:1 requirement sprawl; no TBD/TODO/placeholders; anchored to existing capability/module/API/pattern; scope limits in design **Boundaries** (when needed); presentation/instance/procedure defer per [Cross-artifact completeness](#cross-artifact-completeness); CRITICAL on negative-only requirements; MAJOR on absence-only scenarios, duplicate procedure, exclusion laundry, duplicated orchestration guardrails, or slop in requirement bodies per [Prose quality](#prose-quality)
- **design** — Context, **Goals** (+ optional **Boundaries** ≤3 brief lines when needed), decisions (extend by default; new service/layer/abstraction explicit), risks; cite specs by Requirement title — no proposal What Changes rehash or verbatim SHALL/MUST; Mermaid sequenceDiagram when multi-system/async or brief no-diagram note when single bounded context; MAJOR on exclusion walls per [Cross-artifact distinctness](#cross-artifact-distinctness); MAJOR/CRITICAL on prose slop per [Prose quality](#prose-quality)
- **tasks** — Terse paths + verification per stage; trace via Requirement title or Decision N — no obligation prose or proposal/design rehash; maps to design/specs; one primary outcome per stage; split mixed/oversized stages; checkpoint before new service/abstraction; stage contract below — violations are BLOCKER unless waived; MAJOR on evaluative wrappers per [Prose quality](#prose-quality)
- **e2e-scenarios** — Human-scannable journeys; traceability to requirement titles
- **planning bundle** — Holistic pass + one Complexity & split block + grill alignment across phases

## grill-summaries alignment

When `grill-summaries.md` is in the change folder:

1. Read frontmatter (`grill.planning`, `grill.proposal`, …) and the matching `## <phase>` sections.
2. For each resolved decision in **Decisions** / **User inputs**, confirm the reviewed artifact encodes it (or documents an intentional deviation in design Open Questions).
3. Flag as **BLOCKER** (unless human waived in review):
   - Artifact contradicts a resolved grill decision
   - Major grill decision with no trace in proposal, specs, design, or tasks (silent drop)
   - Phase marked `complete` but **User inputs** empty while the artifact invents requirements not asked in grill
   - `grill.planning: complete` (or phase `complete`) without `grill_mode` in frontmatter, or with `grill_mode` not `skip` and planning-phase **User inputs** empty (interview skipped)
   - Evidence planning grill skipped parent interview (e.g. summarize-only, explore/propose copied into **User inputs** with no grill mode)
4. Flag as **MAJOR**: partial capture (grill decision only in one artifact but required in specs/tasks too).
5. Do not require alignment to `## explore` or explore-only notes.

On a **planning bundle** review, include a short **Grill alignment** subsection in the output (or bullets under Must address) listing matched, missing, and contradictory items.

## Bounded spec grounding (specs phase)

When reviewing delta **specs** that depend on existing code behavior:

1. Confirm the bundle follows [spec-grounding-explore.md](../../tacos-orchestration/references/spec-grounding-explore.md) — observable-behavior obligations only; no implementation inventory in requirements.
2. Flag as **BLOCKER** (unless waived):
   - Specs cite file paths, class names, or call graphs as if parent read implementation during specs when Task delegation is the norm for the host
   - Requirements grounded on internal data structures or algorithms contrary to the observable-behavior scope table
3. Flag as **MAJOR**:
   - Specs duplicate implementation procedure that belongs in design/tasks
   - Multiple surfaces grounded in one spec block without evidence of per-surface delegation when ≥2 distinct surfaces are implicated

Note: Reviewers cannot always prove parent skipped **`tacos-spec-grounding`** — flag observable symptoms (implementation inventory in specs) rather than orchestration telemetry.

## tasks.md structure (blockers)

When reviewing `tasks.md` or a bundle that includes it, verify each `##` stage. Failures are **BLOCKER** unless the human waives in chat (state in review).

**Testable outcome per stage (BLOCKER unless waived):** When staged apply is enabled (`orchestration.staged_apply_enabled`, tacos default), each numbered `## N.` stage MUST include a `**Testable outcome:**` line **immediately after the heading** — one plain sentence stating what the user or reviewer can verify after the last implementation checkbox in that stage completes. The line MUST appear **before** the stage grill gate line (`Stage grill:`) and before numbered implementation checkboxes. Missing line, empty sentence, or outcome placed after implementation work → **BLOCKER**. Vague outcome with no observable check (“implement signals”, “update evaluator”) → **MAJOR**. Cross-check: tacos schema `tasks` artifact instruction and `openspec/schemas/tacos/templates/tasks.md` (same pattern as tacos-work **## Work**).

When reviewing, count numbered `## N.` headings and confirm each has exactly one `**Testable outcome:**` before the first `- [ ]` contract or implementation line.

When the **stage grill gate** is true ([task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate), each `##` stage MUST start with a separate `- [ ]` line whose text starts with **`Stage grill:`** before any implementation checkboxes. Missing or merged into Apply review / Human review → **BLOCKER**.

Per stage, after implementation work, checklist order:

1. **Mocks (optional)** — mock/fixture lines only when the host documents them in `context` or optional `rules.tasks`; otherwise omit
2. **Verify Decision N** (when design decisions require) — separate `- [ ] Verify Decision N: <done-when>` rows; FORBIDDEN embedding Verify Decision inside implementation checkboxes ([task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Verify Decision — separate rows)
3. **Tests** — explicit test line(s), or `Tests: N/A` with one-line reason
4. **Project overview (optional)** — when `project_overview.enabled` and this stage ships user-visible surface per [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Project overview line (optional); omit when internal-only
5. **Apply review** — separate `- [ ]` line matching [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Apply review line **medium** checklist (`parallel Task`, `tacos-apply-review` + `tacos-additional-apply-review`, parent merge, **MUST NOT** core-only — not the one-line pointer alone)
6. **Human review** — separate `- [ ]` line starting with `Human review:` or `Human:`

Also: no orphan test-only stage; behavioral stages need tests in that stage.

**Decision verification matrix (BLOCKER unless waived):** When `design.md` **Decision N** or grill **User inputs** document pipeline ordering, gate-before-I/O, named negative/error paths, or scope boundaries, the matching `##` stage MUST include a `- [ ] Verify Decision N: <done-when>` row (or `Verify Decision N: N/A — <reason>` when genuinely not testable in that stage). Missing verification row → **BLOCKER**. Vague done-when (“add tests”, “verify behavior”) without named test or trace criterion → **MAJOR**. Embedding `Verify Decision N` / `— **Verify Decision N**` inside an implementation checkbox instead of a separate row → **BLOCKER**.

**Apply review line contract (BLOCKER unless waived):** Each stage **Apply review:** line MUST include `parallel Task`, `tacos-apply-review`, `tacos-additional-apply-review`, `parent merge`, and `**MUST NOT** core-only` per [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Apply review line (first fenced block). One-line pointer or `Invoke tacos-apply-review` alone → **BLOCKER**.

**Modified Capabilities vs main-spec contradiction (BLOCKER unless waived):** When a change delta alters or contradicts obligations in an existing `openspec/specs/<capability>/spec.md` (including a sibling capability of the primary delta folder), `proposal.md` **Modified Capabilities** MUST list that capability and the change MUST include a matching delta (or explicit cross-capability MODIFIED requirement). Delta that contradicts main without listing the capability → **BLOCKER**.

**Main-spec staleness with listed delta (not BLOCKER):** When **Modified Capabilities** lists a capability and the change includes a matching delta spec that addresses the obligation that differs from main, planning review MUST NOT raise **BLOCKER** solely because main `openspec/specs/<capability>/spec.md` still shows pre-sync text — sync/archive merges deltas. Record informational note under **Optional (MINOR)** or cross-artifact completeness when useful.

Schema quality (BLOCKER unless waived): specs with TBD/TODO/placeholders in requirements; new/non-trivial requirements with no anchor to existing capability, module, API, or pattern; design decisions that introduce a new service, cross-cutting layer, or major abstraction without an explicit Decision and rationale; multi-system or async flows without a sequence diagram (or missing one-line no-diagram note when in-scope); tasks stages that do not map to design/specs, mix unrelated outcomes, or are too large for one review pass without split; implementation tasks for a new service or major abstraction without a prior design justification or checkpoint task.

Must match the tacos schema `tasks` instruction and [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md).

When `orchestration.staged_apply_enabled` is false, terminal review lines MAY be omitted (host toggle).
