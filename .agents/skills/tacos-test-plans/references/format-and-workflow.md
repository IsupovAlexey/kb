# Test plan pack: format and workflow

Use with [../SKILL.md](../SKILL.md) (workflow, post-run summary). Case-level format: [test-case-format.md](test-case-format.md). Preview gate: [preview-gate.md](preview-gate.md).

## Audience and roles

|Actor|Role|
|-|-|
|QA|Creates and updates packs via `/tacos-test-plans`; commits on a QA branch; owns scenario text|
|Eng|Reads committed packs during `/tacos-tdd` and **apply**; writes automated tests; MUST NOT edit `openspec/test-plans/**`|
|Reviewers|Product and design stakeholders review `<slug>-test-cases.md` on the QA PR before eng starts; traceability to specs|

## Pack layout

Each slug lives at `openspec/test-plans/<slug>/`:

|File|Required|Role|
|-|-|-|
|`<slug>-test-cases.md`|Yes|Primary acceptance-case document|
|`grill-summaries.md`|No|Scope grill decisions for this pack (written on create/update when grill ran)|
|`sources.md`|No|Scan audit (paths, URLs, change folder)|
|`journeys.md`|No|Journey-level handoff notes for eng TDD (advisory)|
|`diagrams.md`|No|Mermaid or ASCII when too large for test-cases intro|

Templates: [templates/](templates/) · host-editable copy: `openspec/schemas/tacos/templates/test-plan-pack/`

## Source of truth

1. Files committed under `openspec/test-plans/<slug>/` are authoritative acceptance criteria during eng work.
2. Eng agents MUST NOT rewrite scenario text, expected results, or `TC-*` ids to match implementation.
3. When implementation and scenarios diverge, eng escalates — QA re-invokes `/tacos-test-plans` with replace approval on a QA branch.
4. Linking: eng change MAY record `Test plan: openspec/test-plans/<slug>/` in `tdd.md`, `proposal.md`, or `design.md`. No auto-sync back to the pack.

## QA workflow (branch → commit)

```text
Host gather skills (inputs)
        │
        ▼
Create QA branch
        │
        ▼
/tacos-test-plans <slug>
  → scan sources
  → scope grill (tacos-grill mechanics)
  → synthesize (subagent when installed)
  → plan review (subagent when installed)
  → human preview gate
        │
        ▼
Proceed → write pack → commit → PR
```

Detail: [scope-grill.md](scope-grill.md) · [plan-review.md](plan-review.md) · [preview-gate.md](preview-gate.md)

Eng work starts after QA commits (or eng change explicitly links the slug).

## Level of detail

Acceptance-case granularity — maps to spec `#### Scenario:` lines and requirement bodies. **Not** journey-level closure (that is `tacos-e2e-scenarios` on a change folder).

- One or more `TC-*` cases per requirement when behavior is testable.
- Placeholders when design is open — do not skip the requirement section.
- `journeys.md` when eng benefits from a thin journey map; does not replace test cases.

## Distinction from e2e scenario closure

||`tacos-test-plans`|`tacos-e2e-scenarios`|
|-|-|-|
|Output|`openspec/test-plans/<slug>/`|`openspec/changes/<name>/e2e-scenarios.md`|
|When|QA-led; may predate eng change|POST-ARTIFACT on a change|
|Granularity|Acceptance cases (`TC-*`)|2–8 journey scenarios|
|Auto-invoke|Never during eng apply|When `orchestration.e2e_enabled`|

This skill MUST NOT create `e2e-scenarios.md` or `tdd.md` unless the user invokes those workflows separately.

## Anti-patterns

- One `TC-*` per spec scenario line with no synthesis (OK when scenarios are already distinct)
- Editing test plans during eng TDD to make Red tests pass
- Silent overwrite of an existing slug without replace approval
- Writing implementation selectors or internal API contracts in Expected result cells
