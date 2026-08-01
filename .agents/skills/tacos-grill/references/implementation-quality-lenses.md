# Implementation quality lenses (planning-time)

Use during **planning grill** and per-phase grills (`proposal`, `specs`, `design`, `tasks`) — **not** as a fixed checklist on every change or apply stage. Primary home for implementation-quality concerns in **planning artifacts**, before code — **only when applicable**.

**Lens catalog:** edge cases · reliability · scalability · performance · observability · security · privacy/compliance · compatibility/migration · testability/verification · deployment safety · dependencies/blast radius · cost · maintainability/evolvability.

Apply-stage grill ([grill-prompts/apply-mandatory.md](grill-prompts/apply-mandatory.md) and [grill-prompts/apply-triggered.md](grill-prompts/apply-triggered.md)) checks whether the **existing plan** for that stage is still correct; if a lens gap appears, **full-resync** planning artifacts — do not substitute apply interview for missing planning coverage.

## Applicability first (required)

**MUST NOT** run every lens on every change. **MUST NOT** ask about a lens when scope does not implicate it (e.g. prose-only docs, local scripts with no production surface, pure refactors with unchanged runtime contract).

**MUST:**

1. Decide applicability from scope **before** interviewing (repo, proposal message, capabilities).
2. Ask only lenses that apply; skip the rest.
3. Record **N/A — [lens] — [one-line why]** in `grill-summaries.md` **User inputs** when a lens does not apply (so “we considered it” without fake requirements).

Completeness means **appropriate coverage**, not **maximum questions**.

## When to probe (risk-based)

Probe only when the change **touches** the concern. If no row below fits, state that implementation-quality lenses are **N/A for this change** and do not invent production-readiness topics.

|Signal in scope|Likely lenses|
|-|-|
|User-facing API, workflow, or UI|Performance, reliability, edge cases, observability, security (authorization (authz)/input)|
|Background jobs, queues, schedules|Reliability (retries, idempotency), observability, scalability|
|New or changed persistence|Reliability, edge cases, scalability, security (data), compatibility/migration|
|Cross-service or async calls|Dependencies/blast radius, reliability (timeouts, circuit breakers), observability (correlation)|
|Config, feature flags, rollout|Deployment safety, reliability (rollback), edge cases|
|Auth, personally identifiable information (PII), secrets, multi-tenant|Security, privacy/compliance, observability (audit/redaction), edge cases|
|High traffic or bursty load|Scalability, performance, reliability (degradation), observability (service level objectives (SLOs)/signals), cost|
|API/schema/version or consumer contract change|Compatibility/migration, deployment safety, testability|
|New service, on-call, or prod surface|Observability, dependencies, deployment safety, maintainability|
|Infra, quotas, multi-tenant scale|Cost, scalability, reliability|
|Long-lived module many teams touch|Maintainability/evolvability, dependencies|
|Pure markdown / schema / local scripts|Usually all N/A — state why per lens|

Depth follows **risk**: customer impact, blast radius, reversibility. Prefer highest-impact questions; `grill.default_max_questions` is a gather guidance baseline — size up when uncertainty is high, shrink when low; MUST NOT pad to reach the baseline.

## Lenses (what “good planning” captures)

Synthesized from production readiness review, architecture review, and site reliability engineering (SRE) practice. Adapt to change size; do not require full production readiness review (PRR) rigor for trivial edits.

### Edge cases and failure modes

- Document **failure modes** for components you add or change: what breaks, what degrades, what fails closed vs open.
- Boundaries: empty/null, invalid input, partial failure, duplicate delivery, clock skew, permission denied.
- **Negative paths** in specs (WHEN/THEN) and tasks (how verified).
- For distributed work: idempotency, ordering, split-brain, stale reads.

### Reliability

- **Dependencies**: timeouts on every external call; bounded retries with backoff/jitter; circuit breaking where cascading failure is possible.
- **Graceful degradation**: what still works when a dependency is down; admission control or load shedding if relevant.
- **Recovery**: recovery point objective (RPO) / recovery time objective (RTO) or “acceptable data loss / downtime” when material — not required for every change.
- **SLO-minded**: draft service level objective (SLO) or “how we know it’s healthy” in design/tasks for user-visible behavior.

### Scalability

- Expected load shape (steady vs spike); bottlenecks (DB, CPU, fan-out).
- **State**: externalized session/state if horizontal scale matters.
- Async vs sync for slow work; caching only with invalidation story.
- Capacity headroom when growth is material — not mandatory load test for every change.

### Performance (latency and efficiency)

- User-visible or partner-facing **latency budgets** (p95/p99, throughput) when the path is latency-sensitive.
- Hot paths, payload size, N+1 queries, sync vs async for slow work.
- Resource use (CPU, memory, I/O) when it affects stability or cost — distinct from “add more servers” (scalability).

### Observability

- **Golden signals** where runtime matters: latency, traffic, errors, saturation (or RED: rate, errors, duration for services).
- **Structured logs** with correlation/request IDs across boundaries when multiple steps exist.
- **Alerts/runbooks**: actionable alerts tied to user impact; on-call expectations when new failure modes matter.
- Tracing/metrics when distributed or hard to debug; skill/docs-only changes → N/A with reason.

### Pipeline and gate insertion

When adding a limiter, authorization (authz) check, validation gate, cache layer, or admission control to an **existing** entrypoint:

- Record an **ordered call graph** in design **Decisions** or grill **User inputs**: validate → admit/limit → read → mutate → publish (adapt labels to domain).
- Name **dependency classes** subject to the gate — external API, DB, cache, message bus, **loop-body reads** — not only “before persist.”
- Each ordering decision → suggested **verification** shape in tasks or tacos-work **Verify Decision N** (named test or hot-path trace).
- Negative paths (validation fail, over-limit, partial failure) → how verified; do not leave “add tests” without done-when.

See [grill-prompts.md](grill-prompts.md) **design** when scope wires a gate into a service method.

### Security (shift-left in planning)

- Authn/authorization (authz) model for new surfaces; least privilege for new permissions or tokens.
- Data: classification, encryption in transit/at rest when applicable.
- Supply chain or secrets when dependencies or credentials change.
- Threat sketch for high-risk changes (abuse, injection, tenant isolation) — not a full audit for every task.

### Privacy and compliance

- **When:** PII, regulated data (health, financial), cross-border transfer, audit obligations, retention policy.
- Lawful basis, minimization, retention/deletion, subject rights — as requirements when relevant.
- Separation from **security** (controls) vs **compliance** (obligations and evidence).
- Logging/metrics must not leak PII; document redaction or exclusion.

### Compatibility and migration

- **When:** API, schema, config, or behavioral contract changes with existing consumers or data.
- Backward compatibility, versioning, deprecation window, dual-write/read, cutover plan.
- Rollback of **data** and code; reversible migrations — pair with deployment safety.

### Testability and verification

- **When:** Runtime or behavioral change that must be trusted in prod.
- How reliability, security, performance, and observability claims are **verified** (tests, drills, synthetic checks, SLO measurement) — not only designed.
- Negative tests, chaos or failure injection when risk warrants; explicit `Tests: N/A` only with reason.

### Deployment safety

- **When:** Anything shipped to shared or production environments.
- Repeatable builds, staged rollout, canary/feature flags, kill switch.
- Rollback procedure rehearsed or documented; blast radius of a bad deploy.
- Environment parity (staging vs prod) when mis-parity caused past incidents.

### Dependencies and blast radius

- **When:** Upstream/downstream systems, shared infra, or fan-out.
- Dependency map: who you call, who calls you, shared fate (DB, queue, identity).
- Failure propagation and containment; coupling that blocks independent deploy.
- Contract/version assumptions on dependencies.

### Cost

- **When:** New infra, sustained load growth, multi-tenant scale, expensive dependencies (GPU, managed services).
- Cost drivers, rough ceilings, $/transaction or $/tenant if material.
- FinOps N/A for docs-only or zero marginal runtime changes.

### Maintainability and evolvability

- **When:** Long-lived codepaths, platform surfaces, or multi-team ownership.
- Modularity, extension points, ADRs for costly-to-reverse decisions.
- Can the team understand, change, and operate this in six months?
- Avoid designs that force big-bang changes for small product asks.

## Phase ownership (where decisions land)

|Lens|Primary artifact|
|-|-|
|Product edge cases, scope|`proposal.md`, `specs/**/*.md`|
|SHALL/MUST, scenarios, security/privacy/performance **requirements**|`specs/**/*.md`|
|Architecture, trade-offs, failure modes, dependencies, rollout|`design.md`|
|Verification, migration steps, deploy/ops tasks|`tasks.md`|

Planning grill gather script SHOULD pull from this table when scope signals match; per-phase grills deepen the owning artifact only.

## Gather and interview tactics

1. **Scan scope** (proposal message, repo, existing specs) — which rows in “When to probe” apply?
2. **Pick 2–4 highest-risk lenses** from the catalog for this change; do not march through every heading.
3. **One topic per question**; record outcomes in `grill-summaries.md` **Decisions** / **User inputs** and ensure matching artifact sections exist before apply.
4. **Explicit N/A** — when a lens does not apply, say so in summarize (avoids false completeness).

## Apply stage (boundary only)

If stage grill or triggered apply reveals missing coverage for any lens in the **plan**, stop implementation, update specs/design/tasks, then resume — see [task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) full-resync.
