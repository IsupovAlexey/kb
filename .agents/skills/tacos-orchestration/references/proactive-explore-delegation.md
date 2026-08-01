# Proactive explore subagent delegation

When the host supports Task or subagent spawn, the parent orchestrator SHOULD delegate codebase search, navigation, and understanding to read-only **explore** children instead of opening implementation source in the parent thread.

This applies beyond the specs-phase named delegate — see [spec-grounding-explore.md](spec-grounding-explore.md) for **`tacos-spec-grounding`** during **specs** authoring.

## When to delegate early

Launch one or more explore subagents **before** the parent reads implementation files when:

|Signal|Action|
|-|-|
|Context is unclear or the relevant module is unfamiliar|One explore child scoped to the question|
|Multiple independent surfaces or subsystems|**Parallel** explore children — one per surface in the **same parent turn** when supported|
|**explore** command needs repo facts before deciding|Explore child first; triggered grill after facts land|
|**apply** task requires locating files, dependencies, or call sites|**MUST** delegate — explore child for discovery; parent implements from merged bullets per [subagent-return-contracts.md](subagent-return-contracts.md) (discovery ≤12)|
|Broad "how does X work?" without a single known path|**MUST** delegate — do not serial-grep across the parent thread first|

## Host explore subagent

Prefer the host's built-in **`explore`** subagent with `readonly: true` when supported:

```text
Task({
  description: "explore auth module",
  subagent_type: "explore",
  readonly: true,
  prompt: "<scoped question — what to find, what to return, what to omit>\n\nReturn shape: see subagent-return-contracts.md (discovery ≤12)"
})
```

During **specs** grounding, prefer **`tacos-spec-grounding`** per [spec-grounding-explore.md](spec-grounding-explore.md) — observable-behavior scope only.

## Parallel launch

When ≥2 independent questions target distinct surfaces, launch explore (or `tacos-spec-grounding`) children **in one parent turn** when the host allows concurrent Task spawns. Merge child outputs before the parent continues.

**Sequential fallback:** When concurrent spawn is unavailable, run children in order and note sequential fallback in the turn summary.

## Parent default

The parent SHOULD treat inline Grep/Read/SemanticSearch on implementation paths as a **fallback** when Task is unavailable in this host session — not because delegation is slower.

On **apply** discovery when Task is supported, inline parent reads are **forbidden** except paths explicitly named in the active stage implementation checkbox for edit (not discovery) — see [task-stage-contract.md](task-stage-contract.md) ## Parent discovery discipline.

Cross-links: [subagent-return-contracts.md](subagent-return-contracts.md); [explore-return-contract.md](explore-return-contract.md); [runtime-delegation.md](runtime-delegation.md); [orchestration-binding.md](orchestration-binding.md).
