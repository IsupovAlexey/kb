# Skill templates

Skeleton structures for host-local skills. Read repo docs and existing host skills before filling placeholders — do not ship empty boilerplate as finished rubrics.

## SKILL.md skeleton (all paths)

Progressive-load pattern — pick one and declare in Entry:

|Pattern|When|SKILL shape|
|-|-|-|
|Router + read-graphs|Disjoint modes with large ref corpora|Entry maps mode → `references/read-graphs/<mode>.md`|
|Phase index|Linear or phased workflow|Entry maps phase → one ref section or graph|
|Mode-conditional Entry|Few modes; small SKILL|Entry branches load one bundle per mode|

MUST gates live in `SKILL.md` or index `MUST read` — not only optional refs.

```markdown
---
name: <kebab-case-skill-name>
description: >-
  <Third-person WHAT — scope of rubric or workflow>. Use when <WHEN triggers —
  paths, commands, review phases, or user invoke phrases>.
disable-model-invocation: <true for manual-only host rubrics; omit or false when ambient>
user-invocable: <true when slash-invoked>
---

# <human title>

<One paragraph: what this skill does in the host repo.>

## Entry

1. Resolve mode or phase from caller.
2. Load active bundle (one hop from `references/`).
3. Follow MUST read + procedure in that bundle.

## Quick start

- <trigger> — <procedure row>
- <trigger> — <procedure row>

## Procedure

1. <step>
2. <step>

## References

<one-hop links to references/\*.md>
```

**Description:** third-person WHAT + WHEN; include path globs or review phase when the skill is review-oriented.

## Review-oriented — conventions skill

Use for cross-cutting standards (e.g. backend-conventions, api-design).

**`references/checklist.md` skeleton:**

```markdown
# <Stack> conventions checklist

Apply when reviewing or authoring <scope paths>.

## Structure

- [ ] <placeholder criterion>
- [ ] <placeholder criterion>

## Naming and layout

- [ ] <placeholder criterion>
```

**`references/rubric.md` skeleton (optional):**

```markdown
# <Stack> review rubric

|Dimension|Guidance|
|-|-|
|<dimension>|<what reviewers check>|
```

**Applicability cues in `description`:** name path globs or file types the skill applies to (e.g. `**/*.cs` under `src/`, React components under `apps/web/`). The parent orchestrator uses these cues for apply-review applicability shortcircuit — confident zero-match on the review diff → skip `tacos-additional-apply-review` spawn and record under **`## Skipped additional skills`** in the parent merge. Description prose only — no new frontmatter keys.

## Review-oriented — testing / checks skill

Use for test norms, command workflows, lint/build gates (e.g. backend-testing, frontend-testing).

**`references/commands.md` skeleton:**

```markdown
# Commands

Run from <repo root | package root>.

|Command|When|
|-|-|
|`<command>`|<purpose>|
```

**`references/test-patterns.md` skeleton:**

```markdown
# Test patterns

## Unit

- <pattern placeholder>

## Integration

- <pattern placeholder>
```

**Applicability cues:** tie to test file globs, frameworks detected in [stack-detection](stack-detection.md), or apply-review scope (implementation diff paths and tests). Include path segments in `description` so apply parents can skip spawn when the diff has zero matching files.

## General — domain or workflow skill

Use for stack tooling, UI patterns, infrastructure helpers without review array wiring.

**`references/workflow.md` skeleton:**

```markdown
# <Workflow name>

## Prerequisites

- <placeholder>

## Steps

1. <placeholder>
```

Omit review yaml hints unless the user later asks to wire the skill into `review.*_additional_skills`.

## Progressive disclosure

|Layer|Content|
|-|-|
|`SKILL.md`|Procedure, quick start, one-hop reference links|
|`references/`|Checklists, rubrics, command tables, long examples|
|Host docs|README or `openspec/host/` pointers when the repo documents skill conventions|

Do not chain reference → reference → reference; keep one hop from `SKILL.md`.
