# Authoring — extend before invent, YAGNI/KISS

Language-agnostic implementation guidance. Adapt examples to your stack; bind repo-specific rules in `openspec/config.yaml` `context` and the consuming project's agent docs.

Structural maintainability **BLOCKER** rubric (code judo, spaghetti, wrappers, boundaries, file decomposition): [structural-maintainability.md](structural-maintainability.md).

## Extend before invent

- Extend existing domain services, repositories, handlers, and registration before parallel abstractions.
- New services, cross-cutting layers, or novel patterns need explicit justification in OpenSpec `design.md` (or equivalent).
- When `design.md` exists: decisions **SHOULD** state what is extended; new abstractions **SHOULD** include rationale in **Decisions**.

### Example — extend owner vs new service

**Prefer (extend the type that owns the area):**

```text
// Order already owns pricing
class Order {
  applyDiscount(...) { ... }
}
```

**Defer new service** until `design.md` documents why extension is wrong:

```text
// Same domain concern — new top-level type per feature
class OrderDiscountService {
  apply(order) { ... }
}
```

## Prefer methods on existing cohesive types

- Default: add methods to the type that already owns the area — not a new `*Service` (or equivalent) per feature.
- New type only when:
  - Clear bounded context or lifetime differs from existing types, or
  - Design/OpenSpec documents why extension is wrong.

## Simplicity

- Smallest change that meets the requirement: straight-line flow, few dependencies, no layers “for later.”
- Simplify heavy first drafts before shipping.

**Abstraction**

- New interface, base, generic, factory, or wrapper: **2+ real uses today** or explicit `design.md` justification.
- No wrapper that only delegates with no added behavior — see [structural-maintainability.md § Thin wrappers](structural-maintainability.md#thin-wrappers) for BLOCKER detail.
- Match solution complexity to problem complexity; no options for never-varying behavior.
- Patterns in `openspec/config.yaml` `context` are intentional host architecture — not over-engineering.

**Readability**

- Guard clauses over deep nesting — see [structural-maintainability.md § Spaghetti growth](structural-maintainability.md#spaghetti-growth) for busy-path BLOCKER patterns.
- Break opaque multi-step chains (fluent APIs, nested maps, long pipelines) into named steps when intent is unclear.

## DRY (when to share, when to repeat)

### Rule of three / drift

- Default: extend cohesive types; extract shared helper or constant when the **same** logic appears in **3+** places or two copies diverged (drift risk).
- Do **not** extract when copies look alike but **change for different domain reasons** — keep distinct bounded contexts separate.

### Example — rule of three vs premature helper

**Prefer (two similar call sites, different evolve paths):**

```text
function validateShipAddress(a) { ... }
function validateBillAddress(a) { ... }
// Wait for third occurrence or drift before extract
```

**Defer extraction** when only two call sites may diverge by domain:

```text
function validateShipAddress(a) { validateAddress(a, "ship"); }
function validateBillAddress(a) { validateAddress(a, "bill"); }
// validateAddress() — wait for third occurrence or drift before extract
```

**BLOCKER signal (~5+ identical lines)**

- Identical logic blocks (~5+ lines) in multiple places without domain reason to keep copies separate — extract or consolidate; do not defer
- Same validation or guard sequence copy-pasted with slight edits across unrelated types
- Same magic literal in many files without a shared constant when drift risk is real

**Remediation example — duplication BLOCKER:**

```text
// BLOCKER: ~8 identical lines in two handlers
function validateShipAddress(a) { checkCountry(a); checkPostal(a); ... }
function validateBillAddress(a) { checkCountry(a); checkPostal(a); ... }

// Fix: extract shared helper OR keep separate with documented domain reason
function validateAddress(a, kind) { ... }
```

Rule of three remains guidance for **when to proactively extract** — it does not downgrade a ~5+ line identical copy to acceptable.

### Extraction placement (language-agnostic)

DRY decides **whether** to consolidate duplicate logic; placement decides **where** shared logic lives. Count **production consumers** (not test-only callers; multiple call sites in one owner module count as one consumer).

|Consumers|Default placement|
|-|-|
|1|Private or nested on the owner (method, nested type, module-local function) — not a new top-level file or module|
|2 in the same owner module|Same-file private helper or absorb into the dominant type|
|2+ distinct production owners|Shared top-level module or type is OK when the owner is not already unwieldy|

When `review.apply_review_additional_skills` includes a host layout skill, **host placement wins** over core DRY remediation wording. Co-location rules in host skills govern file and module structure; this rubric governs consolidation timing (rule of three, ~5+ line BLOCKER).

Before recommending "extract helper," ask: **How many production consumers?** Single-consumer consolidation satisfies DRY without a new top-level artifact.

**Accept when**

- Two occurrences of a simple expression (wait for the third)
- Test arrange blocks that differ by scenario (tests stay self-contained)
- Interface signatures that must repeat by language or framework rules
- Similar code that changes for different domain reasons

## Single responsibility (light)

**Type:** describable in one sentence without unrelated “and”; constructor dependencies serve one concern.

**Method:** one abstraction level — do not mix parse, rules, I/O, and metrics in one block; prefer **coordinate** or **compute**, not both; avoid very long methods mixing unrelated steps without delegation.

**Carve-outs:** orchestration types may call multiple services; mapping types may touch many fields; test classes group related cases.

**Boundaries (generic):** entry points (HTTP, CLI, message consumers) validate → delegate → return; domain types enforce invariants without direct infrastructure calls. Layer-leak BLOCKER detail: [structural-maintainability.md § Boundary and type cleanliness](structural-maintainability.md#boundary-and-type-cleanliness).

## Naming

- **Match peers** — same suffix conventions (`*Service`, etc.); no `*Resolver` / `*Handler` / `*Manager` without boundary reason in design.
- **Name by responsibility**, not a single consumer or UI.

## Comments and docs

- No comments that only restate identifiers or obvious behavior.
- Prefer silence unless “why” is non-obvious or policy requires public API docs.

## Relation to review skills

- **Planning artifacts** — [../../tacos-spec-review/SKILL.md](../../tacos-spec-review/SKILL.md)
- **Implementation diffs** — [../../tacos-apply-review/SKILL.md](../../tacos-apply-review/SKILL.md) (loads this skill; [checklist.md](checklist.md) for diff-scoped checks; structural **BLOCKER** cites [structural-maintainability.md](structural-maintainability.md))
