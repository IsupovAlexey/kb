# Apply review dispatch prompt

Parent orchestrators (staged apply, tacos-work Phase 4, manual `/tacos-apply-review` parent) MUST use this template when launching **`agent-tacos-apply-review`**.

**Dispatch contract:** Read this file. Copy the fenced `prompt:` block **verbatim** into the Task body — substitute only `{PLACEHOLDER}` tokens. Do NOT summarize, shorten, reorder, or improvise the prompt. Improvising strips load-bearing review procedure from the child.

Parallel additional review: launch separate children per [host-additional-skills.md](host-additional-skills.md); this template is **core only**.

## Placeholders

|Token|Fill with|
|-|-|
|`{MODE}`|`full tacos` \| `tacos-work` \| `re-review`|
|`{CHANGE_OR_SLUG}`|Change id or tacos-work slug|
|`{STAGE}`|Stage number/title for staged apply; `session` for tacos-work; `rN` suffix for re-review|
|`{OUTPUT_PATH}`|Target review artifact path on disk|
|`{DIFF_SCOPE}`|Changed file paths or `git diff` scope summary|
|`{PLANNING_PATHS}`|Newline list of planning files the child MUST read (change folder set or work `tasks.md` + touched specs)|
|`{PRIOR_REVIEW_PATH}`|Prior `apply-review-*.md` path for re-review; `NONE` otherwise|

## Prompt template

```text
prompt: |
  Run tacos-apply-review (core only).

  Mode: {MODE}
  Change or slug: {CHANGE_OR_SLUG}
  Stage: {STAGE}
  Output: {OUTPUT_PATH}
  Diff scope:
  {DIFF_SCOPE}

  Planning inputs (read from disk — cite paths in findings):
  {PLANNING_PATHS}

  Prior review: {PRIOR_REVIEW_PATH}

  Load `tacos-apply-review/SKILL.md` Entry for this mode.
  Pass 1: `spec-compliance-pass.md`, `checklist-pass1.md` — run per-item obligation inventory before findings.
  Pass 2: only when pass 1 and main-spec drift are clean — `checklist-pass2.md`.
  Write output at {OUTPUT_PATH} per `review-format.md`.
  Do not load `review.apply_review_additional_skills` paths — parent merges additional children.
```

## Re-review

Set `{MODE}` to `re-review`, `{PRIOR_REVIEW_PATH}` to the failed artifact, `{OUTPUT_PATH}` to the next `-rN` path per [review-format.md](review-format.md) ## After fixes.
