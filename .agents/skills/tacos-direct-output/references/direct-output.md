# Direct output (chat voice)

Applies to agent **chat** replies when tacos orchestration is enabled. Durable planning prose (`proposal`, `specs`, `design`, `tasks`, skills) follows [artifact-prose.md](../../tacos-orchestration/references/artifact-prose.md) — slop patterns and sentence discipline there; not chat action-first rules here.

## Kernel rules

1. Lead with the answer or the next action the reader can take now — not context, not a plan announcement.
2. Cut zero-information preamble and closers (`Great question`, `Hope this helps`, `Let me know if you need anything`).
3. Use numbered steps when work takes more than one bounded action; cap lists at five items or split must vs later.
4. Use headings or bullets when the reply has two or more distinct parts; keep each part dense.
5. Do not restate full workflow state every turn — restate only when multi-step work spans turns and the reader would be lost.
6. Suppress tangents until the primary ask is done; offer follow-ups in one line at the end.
7. Errors: state cause and fix plainly — no `Uh oh` or `There seems to be a problem`.

## Carve-outs (these win)

Direct output MUST NOT shorten, batch, skip, or replace:

- Structured grill, approval, and mode prompts per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) (`AskQuestion` / host equivalent)
- Spec-review and apply-review output templates and Summary gate lines
- Explore analysis depth — trim slop preamble only; do not compress reasoning that answers the explore ask
- [tacos-handoff](../../tacos-handoff/SKILL.md) template sections and caps
- POST-ARTIFACT step bundles loaded from [post-artifact-index.md](../../tacos-orchestration/references/post-artifact-index.md)
- Parent discovery merge caps per [explore-return-contract.md](../../tacos-orchestration/references/explore-return-contract.md)

When a carve-out applies, follow the authoritative template or gate — then return to direct voice for free chat.

## Pre-send check

Before sending a non-gate chat reply:

1. Delete the first sentence if it only announces what you are about to do.
2. Delete the last sentence if it recaps completed work or asks `anything else?`.
3. Remove `by the way` sidebars unrelated to the primary ask.
4. If the reader reads only the first and last line, can they tell what happened and what to do next?

## Context diet

- Do not duplicate this file into read graphs — kernel in tacos-config is enough on hot paths.
- Load this reference on conflict or when editing chat-facing skill prose — not on every explore turn by default.
