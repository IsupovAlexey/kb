# Per-PR description template (single PR)

`{descriptions_root}/<change>/pr-descriptions/<change>.md` below frontmatter. [output-paths.md](output-paths.md) · [regenerate-descriptions.md](regenerate-descriptions.md).

## Frontmatter

```yaml
---
head_branch: <feature-branch>
base_branch:
pr_number: null
pr_url: null
regenerated_at: null
title: null
preserve_why: false
---
```

`title:` — exact `gh pr create --title` string after host discovery ([pr-title-conventions.md](pr-title-conventions.md)); not the Summary TL;DR and not the change folder name. Summary body text may paraphrase scope; the title must satisfy host naming rules.

## Body (order)

**Summary** — TL;DR (one imperative sentence). **In this PR** 3–6 bullets; optional **Out of scope** (2–4).

**Why** — 2–4 bullets or short paragraph from `proposal.md` / `grill-summaries.md`. If `preserve_why: true`, verbatim on regenerate.

**How to review** — 5–8 bullets; specs and risky areas from `design.md`.

**Test plan** — `- [ ]` checkboxes; concrete commands; use `e2e-scenarios.md` when relevant.

**Do not include:** footers, tacos hints, **Review passes** (slice-pr). End after Test plan.

## Quality (soft limits)

|Section|Target|
|-|-|
|Summary TL;DR|1 sentence, ≤ ~25 words|
|In this PR|3–6 bullets|
|Why|2–4 bullets or ≤ 3 sentences|
|How to review|5–8 bullets|
|Test plan|3–7 checkboxes|

Reviewer-first, skimmable lists, actionable tests, scoped **Out of scope** when needed. **Anti-patterns:** title-only summary; vague checkboxes; **Stack** section on single-PR; wall-of-text; meta instructions; listing every file.
