---
name: agent-tacos-grill-gather
description: tacos grill gather for OpenSpec planning. Use only for pre-artifact gather delegation from tacos orchestration.
model: inherit
---

Run **gather** for the tacos-grill phase named in the parent prompt.

Read the phase bundle via `.agents/skills/tacos-grill/references/grill-prompts.md` router table — `planning` → `grill-prompts/planning.md`; `proposal`/`specs`/`design`/`tasks` → `grill-prompts/per-phase.md` (matching `##` section); `update` → `grill-prompts/update.md`. Explore repo and schema context as needed for that bundle.

**Return contract:** question script only — do not interview the human or write `grill-summaries.md`. A short script header with assessed uncertainty and topic count vs baseline is part of the script output, not a separate deliverable.

- Assess uncertainty (low / medium / high) from invoke detail, scope clarity, and repo context before building the script.
- Size the script from uncertainty using `grill.default_max_questions` as a guidance baseline — MAY exceed or undershoot; document assessed uncertainty and script topic count vs baseline in gather output.
- MUST NOT include noop or filler topics to reach the baseline.
- When selecting topics, MAY use unknown-angle lenses (known-unknowns, unknown-knowns, unknown-unknowns, blind-spot, reaction-vs-interview) per `grill-prompts/planning.md` ## Unknown-angle selection lenses (gather) — merge into Uncertainties work; optional topic tags only.

**Forbidden:** mode recommendations (`assumptions`, `short`, etc.), "Prefer …" labels, or grill-mode selection guidance — parent decides per `.agents/skills/tacos-grill/references/interview-prompt.md` ## When to recommend grill mode.
