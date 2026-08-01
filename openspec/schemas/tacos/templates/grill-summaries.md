---
grill_mode: null
grill:
  planning: pending
  proposal: pending
  specs: pending
  design: pending
  tasks: pending
  update: pending
---

# Grill summaries

Cumulative record of grilling sessions for this change. Used when generating
artifacts and for PR motivation. Do not contradict resolved decisions here.

**Frontmatter:**

- `grill_mode` — `full` | `short` | `defaults` | `assumptions` | `skip` | `null` (not grilled yet).
  **Required** when `grill.planning` is `complete` (use `skip` when user skipped planning grill).

Gate rules: tacos-orchestration `references/grill-gates.md`.

Completion rubric: `grill.planning: complete` only after parent interview (grill mode + topics per `tacos-grill/references/interview-prompt.md` under the host project skills root) and non-empty User inputs in each planning phase section (unless `grill_mode: skip`). `grill.update: complete` only after update-phase interview and non-empty User inputs under `## update` (unless skip). Explore and propose message text alone do not satisfy either.

## Anti-patterns (invalid — re-run planning grill)

These patterns indicate a skipped interview; reset `grill.planning` to `pending` and re-run gather → grill mode offer → parent interview → summarize:

- `grill_mode: short` (or any mode) with `grill.planning: complete` but User inputs cite "explore thread", "explore + propose", or paraphrase `## explore` without topic-turn replies
- Planning phases (`## proposal` … `## tasks`) with User inputs = "Same as proposal" or identical text across phases without per-phase interview
- `## explore` present with substantive decisions while planning phases claim `complete` in the same session without grill mode AskQuestion / AskUserQuestion (or host equivalent) / interview turns
- Frontmatter `complete` on all planning phase keys immediately after explore or the user's propose message — OpenSpec file existence does not satisfy `grill.planning`

Body: `## proposal`, `## specs`, `## design`, `## tasks`, `## update` (plus `## apply`, `## explore` when triggered). Each section: Summary, Decisions, Open questions, User inputs.

## proposal

### Summary

### Decisions

### Open questions

### User inputs

## specs

### Summary

### Decisions

### Open questions

### User inputs

## design

### Summary

### Decisions

### Open questions

### User inputs

## tasks

### Summary

### Decisions

### Open questions

### User inputs

## update

Revision-turn record for **update** command — ripple scope, redirect outcome, apply-in-progress notes. Append on each fresh update grill; do not erase prior planning phase sections.

### Summary

### Decisions

### Open questions

### User inputs

## apply

For staged apply with more than two stages, add one `### Stage N` block per numbered `## N.` stage in `tasks.md` (reuse the Stage 1/2 subsection pattern below).

### Stage 1

#### Summary

#### Decisions

#### Open questions

#### User inputs

### Stage 2

#### Summary

#### Decisions

#### Open questions

#### User inputs

## explore

### Summary

### Decisions

### Open questions

### User inputs
