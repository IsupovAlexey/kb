# Structural maintainability

Language-agnostic **BLOCKER** rubric for structural regressions in touched code. Adapt examples to your stack; bind repo-specific carve-outs in `openspec/config.yaml` `context`.

Rubric for `tacos-apply-review` maintainability **BLOCKER** findings — see [Relation to review skills](#relation-to-review-skills).

Classic KISS/YAGNI/DRY/SRP authoring guidance lives in [authoring.md](authoring.md).

## Reviewer stance

Apply review is not only regression detection. Before classifying findings, ask whether a **code judo** move exists in **touched code** — a reframing that keeps behavior but deletes layers, branches, or indirection. When the diff adds a new abstraction, parallel module, or branching surface, briefly consider whether extending the canonical owner or reusing an established helper would make the change dramatically simpler.

Report **BLOCKER** when that simpler path is **clear**. When a plausible judo move exists but needs a documented tradeoff (bounded context, lifetime, host pattern), note it at **MAJOR** with the reframe suggestion — do not stop at behavior-correct alone.

Before approving "extract helper," ask **how many production consumers** — single consumer → inline on owner per [authoring.md § Extraction placement](authoring.md#extraction-placement-language-agnostic); new top-level file or module only with 2+ consumers or an unwieldy owner (host layout skills win when configured).

## Duplication

Apply review flags **BLOCKER** when the stage diff adds ~5+ lines of identical logic in multiple places without domain reason to keep copies separate. Consolidate duplicate logic — cite target **and** placement (inline on owner vs shared module). Single-consumer consolidation is not a new top-level file.

Rule-of-three guidance, placement table, borderline examples, and remediation tone: [authoring.md § DRY (when to share, when to repeat)](authoring.md#dry-when-to-share-when-to-repeat).

## Code judo

In **touched code**, prefer extending the canonical owner and reusing established helpers before adding parallel layers. Apply review treats a missed judo opportunity as **BLOCKER** when the simpler path clearly keeps the diff smaller and the structure clearer.

See [authoring.md § Extend before invent](authoring.md#extend-before-invent) for the general extend-owner pattern; this section covers **BLOCKER** enforcement in the stage diff.

**Prefer (extend owner — acceptable extension):**

```text
// Order already owns pricing; add method in place
class Order {
  applyPromoCode(code) { ... }
}
```

**BLOCKER (new parallel layer for same concern):**

```text
// Same domain area — new top-level type when Order owns pricing
class PromoCodeApplier {
  apply(order, code) { ... }
}
```

**Borderline — acceptable when design documents why:**

```text
// New type with different lifetime/bounded context — note in design.md
class PromoCampaign {
  // Shared across orders; not owned by a single Order instance
}
```

When `design.md` documents a different bounded context or lifetime, a new type is acceptable — not a BLOCKER. Without that note, default to extending the owner.

## Spaghetti growth

Avoid opaque multi-step chains, deep nesting without guard clarity, and ad-hoc branches in already-busy control paths.

**Prefer:**

```text
if (!order) return Err("missing order");
if (!order.isPayable()) return Err("not payable");
return charge(order);
```

**BLOCKER pattern (nested special cases in busy path):**

```text
if (order) {
  if (order.status == "pending") {
    if (order.hasDiscount()) {
      if (order.discount().type == "promo") { ... }
      else if (order.discount().type == "loyalty") { ... }
    } else { ... }
  }
}
```

Remediation: guard clauses, named intermediate steps, or extract a focused helper when the branch set serves one sub-concern — not a new service layer for a single call site.

## Thin wrappers

No delegate-only indirection with no added behavior, validation, or boundary translation.

**BLOCKER:**

```text
class OrderService {
  create(dto) { return orderRepo.create(dto); }
}
```

**Acceptable (adds behavior at the boundary):**

```text
class OrderService {
  create(dto) {
    validate(dto);
    return orderRepo.create(toEntity(dto));
  }
}
```

Intentional host patterns documented in `openspec/config.yaml` `context` are carve-outs — not flagged as wrappers.

## Boundary and type cleanliness

Keep invariants visible at layer boundaries. Avoid unnecessary casts, optionality churn, or ad-hoc object shapes when a clearer contract exists.

**BLOCKER patterns:**

- Domain types calling infrastructure directly (DB, HTTP, filesystem) without an established host carve-out
- New top-level types that leak orchestration logic into leaf helpers
- Widening return types to `any` / untyped maps to avoid defining the contract

**Prefer:**

```text
// Entry validates → delegates → returns typed result
function handleCreateOrder(req): Result<OrderId> {
  const parsed = parseCreateRequest(req);
  if (!parsed.ok) return parsed;
  return orderService.create(parsed.value);
}
```

## Orchestration and atomicity

Avoid serializing independent work when parallel or merged flow is clearer, and avoid multi-step durable updates that can leave state half-applied when one atomic boundary is obvious.

**BLOCKER patterns:**

- Sequential awaits or calls for independent I/O or lookups with no ordering invariant and no clarity gain from serialization
- Related persist/update steps where failure after the first step leaves durable state inconsistent without rollback, compensation, or idempotent recovery
- The same workflow orchestration spread across scattered call sites when one named transaction, saga step, or workflow function would be clearer

**Prefer:**

```text
// Independent fetches — parallel when outcomes merge at the end
const [user, prefs] = await Promise.all([fetchUser(id), fetchPrefs(id)]);
return buildView(user, prefs);

// Related writes — one atomic boundary
await db.transaction(async (tx) => {
  await tx.orders.insert(order);
  await tx.inventory.reserve(order.lines);
});
```

**BLOCKER pattern (serialized independent work + brittle partial update):**

```text
const user = await fetchUser(id);      // independent
const prefs = await fetchPrefs(id);    // independent — no reason to serialize
await saveOrder(order);
await reserveInventory(order.lines);   // can fail after order is saved
```

Remediation: parallelize independent work when it simplifies the flow; group related durable updates behind one transaction, saga step, or explicit compensating action — not a new framework for one call site.

Intentional host patterns (existing saga, outbox, or staged pipeline documented in `context` or `design.md`) are carve-outs.

## Canonical layer

Reuse the layer that already owns the concern — repository for persistence, domain type for invariants, handler for request orchestration. New cross-cutting types need design justification.

Before adding a helper module or `*Service` for one call site, check whether the canonical owner or an existing utility already covers the area.

## Heuristic file decomposition

Not a fixed line count. Flag when **both** apply in the same touched file:

1. **Material growth** — meaningful new surface in the stage diff (exclude whitespace or format-only churn)
2. **Second concern domain** — orthogonal responsibility (e.g. parsing + persistence, orchestration + domain rules)

**BLOCKER example:**

```text
// order-parser.ts grew in this diff with JSON parsing AND SQL mapping
function parseOrderJson(raw) { ... }
function mapRowToOrder(row) { ... }
function saveOrder(order) { ... }
```

Remediation: split or extract with concrete targets — e.g. move persistence to `order-repository.ts`, keep parsing in `order-parser.ts`.

**Not a BLOCKER:** format-only edits, or growth within one concern (more order validation rules in an existing validator).

**Skill references:** when structural BLOCKER rubric and classic authoring guidance (extend, DRY, SRP) both grow in one reference file, prefer splitting into sibling references under `references/` — e.g. this file plus [authoring.md](authoring.md).

## Relation to review skills

- **Planning artifacts** — [../../tacos-spec-review/SKILL.md](../../tacos-spec-review/SKILL.md)
- **Implementation diffs** — [../../tacos-apply-review/SKILL.md](../../tacos-apply-review/SKILL.md) (loads this skill; [checklist.md](../../tacos-apply-review/references/checklist.md) for diff-scoped checks; maintainability **BLOCKER** rows cite sections above)
