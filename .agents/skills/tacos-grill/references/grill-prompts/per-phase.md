# Per-phase planning grill prompts

## proposal

Focus on motivation and delivery outcomes before writing `proposal.md`:

- What problem or opportunity drives this change? Why now?
- What delivery outcomes and capability deltas matter? Which capabilities are new vs modified? Any **BREAKING** changes?
- Success criteria: how will we know this change worked?
- Stakeholders, constraints, and dependencies from project context
- **Scope contract** — minimum **in scope** to satisfy the ask; explicit **out of scope**; **adjacent temptations** the agent might add without being asked — route Out and rejected Adjacent to design **Boundaries** (≤3 brief lines)
- **Minimal-by-default** — simplest version that satisfies the ask; what the agent is tempted to over-build — record in **User inputs**; inform design **Decisions** when delivery shape changes
- **Uncertainties to verify** — assumptions (dependencies, integrations, behavior) not yet confirmed — use unknown-angle lenses when helpful ([planning.md § Unknown-angle selection lenses (gather)](planning.md#unknown-angle-selection-lenses-gather)) — record in **User inputs** before summarize; no separate unknowns summary section
- Edge cases at the product/requirements level (not low-level implementation)
- Park non-obvious exclusions for design **Boundaries** (≤3 brief lines) — proposal avoids out-of-scope bullet walls and path lists
- Which [implementation-quality-lenses.md](../implementation-quality-lenses.md) apply — flag gaps for specs/design/tasks; record N/A for non-applicable lenses

Challenge vague language against `openspec/config.yaml` context and existing `openspec/specs/`. Explore the codebase when answers may already exist.

- **Capability research** — for each capability touched, which main specs under `openspec/specs/<capability>/spec.md` change vs current obligations? List under **Modified Capabilities** when altering, extending, or contradicting main — including sibling capabilities of the primary delta folder ([planning-artifact-loop.md](../../../tacos-orchestration/references/planning-artifact-loop.md) ### Generation-time contract)
- **Prose quality** — challenge evaluative leads ("clear", "explicit", "concrete"), marketing labels, and drama; ask for the fact behind vague quality claims (see [artifact-prose.md](../../../tacos-orchestration/references/artifact-prose.md))

## specs

Focus on testable requirements before writing `specs/**/*.md`:

- **What is the invariant?** — durable behavioral obligations agents and reviewers can rely on; not a UI inventory or color/name list
- **Anti-inventory** — do not put presentation tokens, per-instance enumerations, or Figma-level detail in specs; capture deferrals in design and concrete acceptance in tasks
- **Anti-procedure** — do not put agent workflows, pipeline order, artifact paths, or checklist templates in specs; capture runbooks in design and tasks (and skill references when adding skills)
- **Requirement budget** — for skills/tooling/distribution, aim for a small set of broad requirements; merge related obligations
- **Grill → artifact routing** — when recording ## specs **Decisions**, note artifact home per item: durable behavioral invariant → merged spec requirement (not one requirement per bullet); out-of-scope, config deferrals, untouched integrations → design **Boundaries**; install/verification → tasks. Do not list non-goals as spec **Decisions** expecting 1:1 promotion to `### Requirement:` blocks
- **Scope limits stay in design Boundaries** — initial scope, untouched integrations, and "won't modify X" belong in design **Boundaries** (≤3 brief lines when needed), not spec requirements that only say MUST NOT change X
- **Positive scenarios** — plan invariant scenarios with positive user-visible THEN outcomes; do not plan scenarios that only test absence (no files written, gate did not run)
- **Layering** — specs = SHALL/MUST + invariant scenarios; design = decisions and optional external deferral links; tasks = this change's traceable acceptance
- Each capability: core behaviors, actors, and boundaries
- **Implicit branch tracing** — for material flows: what happens when a step fails, a dependency is unavailable, or state is partial mid-flow; who handles recovery — record in **User inputs**; durable outcomes → specs scenarios or design **Decisions**
- **Main-spec cross-check** — before writing each delta, read matching `openspec/specs/<capability>/spec.md`; **STOP** and update proposal **Modified Capabilities** when delta alters or contradicts an unlisted capability ([planning-artifact-loop.md](../../../tacos-orchestration/references/planning-artifact-loop.md) ### Generation-time contract)
- **Uncertainties to verify** — behavioral assumptions neither grilled nor specified yet — use unknown-angle lenses when helpful ([planning.md § Unknown-angle selection lenses (gather)](planning.md#unknown-angle-selection-lenses-gather)) — surface before writing requirements; no separate unknowns summary section
- Edge cases, error paths, and empty/null states
- SHALL/MUST clarity — avoid ambiguous should/may
- Scenarios: one invariant WHEN/THEN per requirement unless a distinct behavioral branch (error, empty, permission) needs its own scenario — not a step-by-step agent runbook
- Modified vs ADDED requirements — do not silently change existing behavior
- Applicable lens requirements as SHALL/MUST or scenarios — e.g. performance budgets, privacy/retention, compatibility, security/authorization (authz), observability signals (see [implementation-quality-lenses.md](../implementation-quality-lenses.md))
- Negative and boundary scenarios for material reliability, security, or migration behaviors

Do not invent requirements. Ask until each requirement is defensible. Do not require full production readiness review (PRR) rigor for trivial or non-runtime changes — document N/A in grill-summaries when lenses do not apply.

- **Prose quality** — requirements are SHALL/MUST but bodies stay factual; no marketing tone or evaluative wrappers ([artifact-prose.md](../../../tacos-orchestration/references/artifact-prose.md))

## design

Focus on technical decisions before writing `design.md`:

- Architecture choices and alternatives considered
- Data model, API, and integration impacts
- Trade-offs per applicable lenses in [implementation-quality-lenses.md](../implementation-quality-lenses.md) — e.g. scalability, performance, dependencies/blast radius, deployment safety, compatibility/migration, cost, maintainability
- Security, privacy/compliance, and data-handling when in scope
- Documented failure modes and “what happens when X fails” for non-trivial components
- Risks and mitigations; **Goals-first** framing — optional **Boundaries** (≤3 brief lines) only when a reader would assume incorrect scope; no Non-Goals exclusion walls
- Contradictions between proposal, specs, and existing code
- Decisions and deferrals — not inventories or task checklists; reference specs by Requirement title only
- **Pipeline / gate insertion** — when scope adds a limiter, authorization (authz) gate, or admission control to an existing service method: record ordered call graph (validate → admit → read → mutate → publish) and dependency classes (API, DB, cache, loop reads); pair each ordering decision with verification intent for tasks ([implementation-quality-lenses.md](../implementation-quality-lenses.md) ## Pipeline and gate insertion)

Stress-test decisions with concrete scenarios. Capture operability choices (timeouts, retries, idempotency, health) in decisions when they affect implementation — not only after apply.

- **Prose quality** — Goals and Decisions use facts and real alternative trade-offs, not imagined contrast or false agency ([artifact-prose.md](../../../tacos-orchestration/references/artifact-prose.md)); scope in/out of this change in plain prose — no `v1` API or reference labels unless quoting an external version

## tasks

Focus on actionable work breakdown before writing `tasks.md`:

- Task ordering and dependencies
- Scope per task — small enough for one session; terse paths and done-when per checkbox
- How each task will be verified — trace via Requirement title or Decision N, not duplicated obligation prose
- **Verify Decision N** rows when design **Decision N** or grill **User inputs** cover pipeline ordering, gate-before-I/O, negative/error paths, or scope boundaries — done-when MUST name a test, trace step, or command; FORBIDDEN embedding `Verify Decision N` inside implementation checkbox text ([planning-artifact-loop.md](../../../tacos-orchestration/references/planning-artifact-loop.md) ### Generation-time contract; [task-stage-contract.md](../../../tacos-orchestration/references/task-stage-contract.md) ## Verify Decision — separate rows)
- Gaps between design and implementable units
- Testing, rollout, and documentation tasks
- No design essays or proposal motivation in task rows
- When lenses applied at planning: verification tasks (testability lens) — tests, rollout checks, migration validation, monitoring hooks — or explicit `Tests: N/A` with reason per stage
- **Prose quality** — task rows are verb-led paths and done-when; no evaluative wrappers ([artifact-prose.md](../../../tacos-orchestration/references/artifact-prose.md))
