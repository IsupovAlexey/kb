---
name: tacos-host-skill
description: Bootstraps host-local SKILL.md and references/ from repo layout. Invoke via /tacos-host-skill; not OpenSpec apply or tacos-* bundle edits.
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Optional goal (e.g. backend conventions skill, frontend testing rubric, htmx UI patterns)
---

# tacos host skill

Content-bootstrap for **host-local** skills the tacos bundle does not own. Runs on explicit user invoke only (`disable-model-invocation: true`); doctor install/update suggest this workflow when discovery finds gaps — they create host skill files only through this skill after user approval.

## Quick start

|User says|Action|
|-|-|
|`/tacos-host-skill`|Detect stack → clarify purpose → propose scope → bootstrap files|
|`/tacos-host-skill backend testing rubric`|Review-oriented path when cues match|
|`/tacos-host-skill aspire service patterns`|General path|
|Doctor suggests gaps after install|Same workflow; emphasize review-oriented path when wiring is the goal|

## Entry

1. **Load context** — read [stack-detection](references/stack-detection.md); scan detected host skills roots and human docs (README, `openspec/host/`, `AGENTS.md` outside tacos managed blocks).
2. **Clarify purpose** — when goal is missing or could be general vs review-oriented, **AskQuestion** once: domain/workflow skill vs review/conventions/testing rubric. Record choice in chat.
3. **Propose name and scope** — kebab-case directory name without `tacos-*` prefix; align with existing host skills; state target skills root path ([host-layout](references/host-layout.md) — confirm path before creating directories).
4. **Host layout** — follow [host-layout](references/host-layout.md): resolve skills root, confirm target path with user, then portable directory layout before writing files.
5. **Bootstrap content** — from [skill-templates](references/skill-templates.md):
   - **Review-oriented:** conventions, testing, or check-workflow rubric templates; include applicability cues in `description` frontmatter.
   - **General:** domain, infrastructure, or workflow templates without review array guidance unless the user asks.
   - Emit context-diet shape: Entry with progressive-load pattern (router, phase index, or mode-conditional); MUST gates in `SKILL.md` or index `MUST read`; execution steps in generated `SKILL.md`; templates and long rubrics in one-hop `references/` with sibling-relative cross-skill links only.
6. **Review wiring (review-oriented only)** — apply [review-wiring-hints](references/review-wiring-hints.md): suggest `review.spec_review_additional_skills` and `review.apply_review_additional_skills` entries in chat; remind maintainer to run `/tacos-doctor install` or update so empty arrays populate (prefer doctor merge over hand-editing yaml in the same session).
7. **Optional maintainer snippet** — MAY output an `AGENTS.md` routing table row for paste per [host-layout](references/host-layout.md) ## Optional AGENTS.md snippet; write host `AGENTS.md` only when the user explicitly pastes or requests it.
8. **Confirm before write** — preview proposed paths and file bodies; create or overwrite files only after explicit approval ([host-layout](references/host-layout.md) ## Confirm before write).

## Gates

Preview → structured prompt per [structured-gate-convention](../tacos-orchestration/references/structured-gate-convention.md) (**next tool call MUST** be `AskQuestion` / `AskUserQuestion` when available) → else plain text + **end turn** per [interview-prompt](../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable.

|Step|Before…|Option ids / binding|
|-|-|-|
|Clarify purpose|Bootstrap when goal ambiguous|general vs review-oriented — workflow step 2|
|Confirm write|Creating host skill files|explicit approval — [host-layout](references/host-layout.md) ## Confirm before write|

## Done when

- **Purpose resolved:** chat records general vs review-oriented choice when the goal was ambiguous.
- **Path approved:** user explicitly approved the preview list from [host-layout](references/host-layout.md) ## Confirm before write.
- **Files written:** `SKILL.md` at `<skills-root>/<kebab-name>/` plus one-hop `references/` as proposed; generated `name` is kebab-case without `tacos-*`.
- **Review-oriented:** chat includes suggested `review.*_additional_skills` block per [review-wiring-hints](references/review-wiring-hints.md) and reminder to run `/tacos-doctor install` or update.
- **Optional snippet:** when requested, chat includes paste-ready `AGENTS.md` routing row — host `AGENTS.md` on disk unchanged unless the user pastes it.

## When to use

|Situation|Path|
|-|-|
|Doctor reports review skill gaps or empty review arrays|**tacos-host-skill** (review-oriented path)|
|New host conventions, testing, or domain skill|**tacos-host-skill**|
|Multi-stage OpenSpec change implementation|full **apply** — not this skill|
|Edit existing tacos bundle skills|edit shipped `tacos-*` skills in the discovered skills root (maintainer workflow)|

## References

[skill-templates](references/skill-templates.md) · [stack-detection](references/stack-detection.md) · [review-wiring-hints](references/review-wiring-hints.md) · [host-layout](references/host-layout.md)
