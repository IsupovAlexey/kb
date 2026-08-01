# Regenerate `jira.md` from artifacts

Rebuild the change’s `jira.md` from OpenSpec planning artifacts. First step when user wants regenerate **and** optional push (push still needs turn-2 approval per `SKILL.md`).

When `jira.enabled` is true, composition **MUST** run via Task subagent or Task tool when the runtime supports it — not inline-only in the orchestrator thread unless the human waives or Task is unavailable. When delegation is unavailable, run inline and note inline execution in the summary.

When the parent delegates, launch **`agent-tacos-jira-regenerate`** per [runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md). Do **not** pass `model` from yaml on Task.

## Inputs (include)

- `proposal.md`, `design.md`, `specs/**/*.md`, `tasks.md`, `grill-summaries.md`
- `openspec/tacos.yaml` (`jira.description_template` when set; when unset, `openspec/host/jira-description.md` when present)
- Binding hints: `issue_key` / `url` from existing `jira.md` frontmatter or `proposal.md` (link only)

## Inputs (exclude)

- **`jira.md` body** — treat as non-existent (no stale Jira echo).
- Preserve binding frontmatter (`issue_key`, `url`, `status`, `issue_type`, …); regenerate `title` + body.

## Description template

Resolve template in order: (1) `jira.description_template` when set in `openspec/tacos.yaml`, (2) `openspec/host/jira-description.md` when present, (3) bundled structure below:

```markdown
# {{change_name}}

## Summary

{{one_paragraph_why}}

## What changed

{{outcome_bullets}}

## Design highlights

{{key_decisions}}

## Requirements covered

{{capability_summary}}

## Tasks / status

{{brief_status}}

## Open questions

{{open_questions_or_none}}

---

_Generated from OpenSpec change artifacts. Edit before approving Jira sync._
```

## Composition rules

Applies to **regenerate-jira-md**, **compose-description**, and any delegated subagent step that builds title/body from artifacts. **Structure:** resolved template per order above. **Tone and content:** rules below — not the template placeholders alone.

### Audience and tone

Write for **Jira readers** (PM, eng, QA): user-visible outcomes, not OpenSpec file manifests. Summarize capability and behavior; list artifact paths only when one line adds clarity.

### What changed

~3–6 bullets; merge related items (e.g. one bullet for a user-facing capability instead of separate bullets per config file or internal workflow step). Do not echo `proposal.md` bullet structure verbatim.

### Design highlights

Decisions, constraints, and transport — skip internal workflow names (`resolve-binding`, `push-from-jira-md`) unless the ticket audience needs them.

### Requirements covered

Capability ids or plain-language themes (e.g. `jira-artifact`, `jira-sync`), not requirement IDs or spec file structure.

### Tasks / status

One short paragraph or a single line (e.g. “Shipped; manual E2E pending”) — no task-section tables mirroring `tasks.md`.

### Open questions

Only unresolved items worth tracking on the ticket; omit the section if none.

### Length

Prefer concise; trim repetition across sections.

## Output

- Frontmatter: binding + `title` + `regenerated_at` (ISO-8601 UTC). Keep `fetched_at` if already fetched.
- Body: `# <title>` + markdown. See Invocation in `../SKILL.md` for delegation when `jira.enabled`.

If user also asked to push: preview, then [approval-prompt.md](../../tacos-orchestration/references/approval-prompt.md#jira-push). Structured Approve → `push` same turn OK. Plain-text fallback → end turn; no Jira write until next yes.
