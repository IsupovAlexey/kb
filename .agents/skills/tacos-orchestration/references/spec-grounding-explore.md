# Bounded spec grounding via named subagent

During the **specs** artifact phase, agents that need existing-code grounding MUST delegate to **`agent-tacos-spec-grounding`** (or the host **explore** subagent with `readonly: true` when the named agent is not installed) instead of opening implementation source in the parent (orchestrator) thread.

**Exception:** When the **Task** tool is unavailable in this host session, the parent MAY read source directly — record the constraint in chat. Slower or inconvenient delegation is **not** a valid exception.

## Procedure (before spec write)

When proposal **Capabilities** or design **Boundaries** imply grounding on existing behavior:

1. Enumerate surfaces needing grounding (one behavior question per surface when possible).
2. Launch **`agent-tacos-spec-grounding`** — one Task per surface; **parallel in one parent turn** when ≥2 surfaces and the host supports concurrent spawn.
3. Merge observable-behavior bullets from child returns into spec authoring context.
4. Write delta specs from merged bullets — not from parent-context code reads.

Detail: [planning-artifact-loop.md](planning-artifact-loop.md) **continue** specs step.

## Parent prohibition

The parent agent MUST NOT read implementation source files in root context during specs authoring for grounding purposes when Task is supported. Grill summaries, proposal, and design are sufficient parent-context inputs; code reads belong in the delegated pass.

## Named delegate

When Task is supported and `.cursor/agents/agent-tacos-spec-grounding.md` or `.claude/agents/agent-tacos-spec-grounding.md` exists, the parent MUST launch **`agent-tacos-spec-grounding`** — not inline parent reads or ad-hoc explore without the named agent.

**Fallback:** When the named agent file is missing or spawn fails, use host **`explore`** with `readonly: true` and the prompt template below.

```text
Task({
  description: "spec-ground <surface>",
  subagent_type: "agent-tacos-spec-grounding",
  prompt: "<prompt template below with substitutions>"
})
```

Model comes from the installed agent file (`orchestration.spec_grounding_models`) — do not pass `model` from yaml on Task.

## Subagent scope (observable behavior only)

|In scope|Out of scope|
|-|-|
|Inputs and outputs (APIs, events, CLI args, HTTP bodies)|Algorithms and internal logic|
|State changes and persistence effects|Data structures and class hierarchies|
|Side effects (notifications, writes, external calls)|Call paths, file layout, refactor maps|
|Error and empty outcomes users or hosts can observe|Performance characteristics unless user-visible|

## Prompt template

Substitute `<module-or-surface>` and `<behavior-question>` for the change at hand:

```text
Read-only spec grounding for tacos specs phase.

Scope: observable behavior of <module-or-surface> only.
Question: <behavior-question>

Report ONLY:
- Inputs the surface accepts
- Outputs or responses it produces
- State it reads or writes
- Side effects (external calls, events, notifications)
- Observable error/empty outcomes

Do NOT report: algorithms, internal data structures, call graphs, file paths,
or implementation recommendations.

Return concise bullets suitable for writing SHALL/MUST requirements and WHEN/THEN scenarios.

Return shape (bullet cap, no file contents): [explore-return-contract.md](explore-return-contract.md). Observable-behavior scope above still applies — omit file paths and implementation maps from spec grounding reports.
```

## Using explore output

- Translate observable facts into behavioral requirements and scenarios — not implementation previews.
- Do not paste explore output verbatim as a spec inventory; merge into durable obligations.
- If explore reveals a planning gap, update grill-summaries or pause for triggered grill before inventing requirements.

## Broader explore usage

For non-specs codebase search and navigation (**explore** command, apply discovery), see [proactive-explore-delegation.md](proactive-explore-delegation.md).

Cross-links: [planning-artifact-loop.md](planning-artifact-loop.md) ## Content ownership; [runtime-delegation.md](runtime-delegation.md).
