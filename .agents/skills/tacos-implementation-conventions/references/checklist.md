# Implementation conventions checklist

Quick list for authors and `tacos-apply-review`. See [authoring.md](authoring.md) for classic conventions; [structural-maintainability.md](structural-maintainability.md) for structural **BLOCKER** rubric — checklist rows below align to those references.

## Extend before invent

- [ ] Existing types or modules extended where possible
- [ ] New service/layer/abstraction justified in design or review notes
- [ ] No parallel abstraction for the same domain concern

## Cohesion

- [ ] New behavior on the type that already owns the area
- [ ] New top-level type only with clear boundary or design note

## Code judo

- [ ] Reviewer stance applied — considered whether a judo reframe could delete layers before classifying ([§ Reviewer stance](structural-maintainability.md#reviewer-stance))
- [ ] Touched code extends canonical owner before new parallel layers
- [ ] New helper module or service justified when simpler in-place extension exists
- [ ] Bounded-context exceptions documented in design when a new type is introduced

## Spaghetti growth

- [ ] Guard clauses over deep nesting in busy paths
- [ ] Opaque multi-step chains broken into named steps when intent is unclear
- [ ] No ad-hoc special-case branches added to already-complex control flow without reframe or extract

## Thin wrappers

- [ ] No delegate-only wrapper with no added behavior, validation, or boundary translation
- [ ] Intentional host patterns from `context` not flagged as wrappers

## Boundary and type cleanliness

- [ ] Domain types avoid direct infrastructure calls without host carve-out
- [ ] No orchestration logic leaked into leaf helpers
- [ ] Contracts stay typed — no widening to untyped shapes to avoid defining the boundary

## Orchestration and atomicity

- [ ] Independent I/O or lookups not serialized without ordering need or clarity gain
- [ ] Related durable updates grouped behind one transaction, saga step, or compensating flow when failure mid-sequence would leave inconsistent state
- [ ] Workflow steps not scattered across call sites when one named boundary would be clearer
- [ ] Intentional host saga/outbox/pipeline patterns from `context` or `design.md` not flagged

## Canonical layer

- [ ] Persistence, invariants, and request orchestration stay on their established owners
- [ ] Cross-cutting new types have design justification

## Heuristic file decomposition

- [ ] Touched files with material diff growth do not add a second unrelated concern domain
- [ ] Split or extract targets are concrete when both growth signals apply

## Simplicity

- [ ] New interface, base, generic, factory, or wrapper has 2+ uses or design note — see [§ Thin wrappers](structural-maintainability.md#thin-wrappers) for delegate-only wrappers
- [ ] No options or extension points for never-varying behavior
- [ ] Intentional host patterns from `context` not flagged as bloat
- [ ] No unnecessary interfaces or indirection beyond [§ Thin wrappers](structural-maintainability.md#thin-wrappers)
- [ ] Control-flow clarity — guard clauses and named steps per [§ Spaghetti growth](structural-maintainability.md#spaghetti-growth)
- [ ] No dead code or unused branches
- [ ] No speculative “might need later” features beyond spec
- [ ] Straight-line flow; minimal dependencies

## DRY

- [ ] Shared helper or constant only after rule of three or clear drift risk
- [ ] No ~5+ line identical logic blocks without domain reason — consolidate when copies are truly identical (BLOCKER); cite placement per [§ Extraction placement](authoring.md#extraction-placement-language-agnostic)
- [ ] Single production consumer → private or nested on owner — not a new top-level file or module ([§ Extraction placement](authoring.md#extraction-placement-language-agnostic))
- [ ] Before "extract helper": count production consumers; host layout skills win when configured
- [ ] Repeated validation/guard sequences consolidated when drift risk is real (placement per extraction table)
- [ ] Magic literals centralized when used in many places
- [ ] Similar-but-distinct domain logic not forced into one abstraction
- [ ] Duplication of same concern not left across unrelated types

## SRP

- [ ] Type describable in one sentence; dependencies serve one concern
- [ ] Methods at one abstraction level (coordinate or compute, not both)
- [ ] No very long methods mixing unrelated steps without delegation
- [ ] Entry points validate → delegate → return; domain types avoid direct infrastructure
- [ ] Orchestration carve-outs respected (coordinators, mappers, test classes)

## Naming and comments

- [ ] Names consistent with peers in the same area
- [ ] Names describe responsibility, not a single consumer
- [ ] Comments explain non-obvious “why,” not obvious “what”
