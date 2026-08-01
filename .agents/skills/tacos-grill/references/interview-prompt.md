# Interview prompt (structured)

Human-facing grill steps run on the **parent** agent. When structured tools are available, **MUST** use them per ## Structured prompt preferred; use plain-text fallback only when not.

## Runtime mapping

|Runtime|Tool|When|
|-|-|-|
|Cursor|`AskQuestion`|Available in agent chat|
|Claude Code|`AskUserQuestion`|Available in CC agent|
|Other / unavailable|Plain text|Numbered options; **one topic per turn**|

Do not batch multiple script topics into one unstructured wall of text when structured tools are available.

## Structured prompt preferred

When `AskQuestion`, `AskUserQuestion`, or the host structured-prompt tool **is** available (e.g. Cursor IDE agent chat):

1. **MUST use structured prompts** — Any tacos skill gate that mandates structured approval MUST call the host tool; **do not** use plain-text fallback or a prose-only menu when structured tools are available.
2. **Next tool call** — After preview or when a gate opens, the **next** parent tool call when structured tools are available MUST be `AskQuestion` / `AskUserQuestion` — not chat prose such as "Approve / Edit / Cancel?"
3. **Detection** — "Available" means the tool appears in the agent's tool list for this session. Absence from the tool list (e.g. Cursor Cloud agents) → use ## Structured prompt unavailable below; presence → structured path only.

Flow-specific preview semantics stay in each skill; turn discipline for the unavailable path is in ## Structured prompt unavailable.

## Structured prompt unavailable

When `AskQuestion`, `AskUserQuestion`, or the host structured-prompt tool is **not** available (e.g. Cursor Cloud agents):

1. **Do not skip** — MUST NOT mark grill complete, check off `Stage grill:`, pass execute confirmation, or perform gated external writes without a completed parent interview (or explicit user **skip** / waive in chat). Use plain-text fallback instead of inferring answers or treating silence as consent.
2. **Plain-text fallback** — Show numbered options that mirror structured option ids (`1. Full (id: full)`); **one topic per turn**; **end turn** after each ask until the user replies.
3. **Accept replies** — User may reply with option number (`1`–`N`) or stable kebab-case id (e.g. `full`, `proceed`, `approve`).
4. **Never** dump the full gather script, batch multiple topics in one message, or treat silence or “continue” as **defaults** or **skip**.

Flow-specific preview semantics (PR body, Jira description, diff preview) stay in each skill; turn discipline and numbered-option shape are defined here.

### Structured gates

All tacos skills that mandate `AskQuestion` (or host equivalent) MUST use ## Structured prompt preferred when structured tools are available, and ## Structured prompt unavailable when they are not. **Full catalog:** [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md).

Surfaces include planning/per-phase/stage/triggered grill, execute confirmation, tacos-ask small-edit confirm, external-write approval, POST-ARTIFACT intent playback, PR triage, Jira push, project overview, dedrift, slice-pr, and assisted-review — full catalog in [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) § Gate catalog.

When adding a new structured gate, add a row to [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md) and cross-link here.

## Grill mode (opening offer)

Call **once** at the start of each pre-artifact interview, **after** gather returns the question script (or at the start of a triggered grill when offering depth). Single-select.

**Prompt:** `How do you want to grill for <phase>?`

Substitute `<phase>` (`planning`, `proposal`, `specs`, `design`, `tasks`, `apply`, `sync`, `explore`). Use **`planning`** for **propose** and **ff**; use a single artifact phase for **continue**. Offer **`assumptions`** only on **planning** grill (`propose`, `ff`, tacos-work Phase 1) — not on per-phase **continue**, stage apply, or triggered grills.

**Options (planning grill — include `assumptions`):**

|id|label|Meaning|
|-|-|-|
|`full`|Full grill|Ask all topics from the script until exhausted (`grill.default_max_questions` is guidance only — do not stop early solely to hit N; do not pad when script ends early; user may extend)|
|`short`|Short grill|Typical cap at 4 highest-impact topics; **MUST ask ≥1 structured topic question** after mode offer. When gather marked high uncertainty and more than four material topics remain, MAY exceed 4 only after explicit user confirm|
|`defaults`|Use gatherer defaults|Record script `default` values as decisions; only ask where defaults conflict or `answeredBy` is missing; when no gather script, **MUST ask ≥1 structured confirmation**|
|`assumptions`|Assumptions grill|Parent reads gather output and available repo context, presents a structured assumption list for confirm, correct, or expand; record outcomes in **User inputs** as plain prose — not numbered decision ids. See ## Interview minimum (assumptions) and ## When to recommend grill mode|
|`skip`|Skip this phase|Explicit skip → frontmatter skipped; for `planning`, set `grill.planning` and all planning phase keys `skipped`|

Before the mode offer on **planning** (propose, ff, tacos-work Phase 1), parent MAY label **at most one** mode "(Recommended)" per ## When to recommend grill mode — user still chooses. **Default when unsure: no recommendation label.**

**Options (per-phase continue, stage apply, triggered — omit `assumptions`):** same table without the `assumptions` row (`full`, `short`, `defaults`, `skip` only).

**On `full` / `short` / `defaults`:** continue interview per mode — satisfy ## Interview minimum (full / short / defaults). **On `assumptions`:** (planning grill only) satisfy ## Interview minimum (assumptions). **On `skip`:** summarize and persist immediately; do not write the gated planning artifact unless the caller is manual `/tacos-grill` only.

## Interview minimum (full / short / defaults)

Grill mode offer alone does **not** complete any grill gate (`Stage grill:`, `grill.<phase>`, planning sign-off, or manual `/tacos-grill` done-when).

When the user chooses **`full`**, **`short`**, or **`defaults`** (not **`skip`**):

1. **Mode offer is step 0 only** — the next parent action MUST be topic interview per mode. **Forbidden:** repo edits, implementation checkboxes, artifact writes gated on grill, or checking off grill lines before topic interview finishes.
2. **`short`** — MUST ask **at least one** structured topic question (`AskQuestion` / `AskUserQuestion`, or plain-text one topic per turn when tools absent); typical cap at four highest-impact topics from the gather script or active phase bundle. When gather output marked **high** uncertainty and more than four material topics remain, parent MAY exceed four only after explicit user confirm (e.g. "Gather flagged high uncertainty with N topics — continue past 4?"). Inferring answers from `design.md`, `proposal.md`, `specs/`, or `tasks.md` without user-facing prompts does **not** count.
3. **`defaults`** — after mode offer: when a gather script exists, record script `default` values and ask only on conflict or missing `answeredBy`; when **no** gather script (mandatory stage apply, triggered apply/sync/explore), MUST ask **at least one** structured confirmation before recording defaults from planning artifacts.
4. **`full`** — MUST ask script topics until the script is exhausted. `grill.default_max_questions` is a guidance baseline only — MUST NOT stop early solely to hit N; MUST NOT pad with filler when the script ends early; user may extend when material topics remain.

**User inputs** and **Decisions** in `grill-summaries.md` MUST trace to user replies from this session's structured or plain-text topic turns — not unilateral summaries of planning artifacts or prior `### Stage N` bullets.

**Planning topics:** For implementation-quality lenses (full catalog in [implementation-quality-lenses.md](implementation-quality-lenses.md)), ask **only when applicable** per **Applicability first** — do not use full/short mode as an excuse to run the whole catalog on docs-only or non-runtime scope.

## Interview minimum (assumptions)

When the user chooses **`assumptions`** (not **`skip`**) on **planning** grill (propose, ff, tacos-work Phase 1):

1. **Mode offer is step 0 only** — after selection, parent reads gather script output and available repo context before the first assumption prompt.
2. **Assumption list** — present assumptions across scope interpretation, patterns, file locations, failure-handling defaults, and implicit branches per [per-phase.md](grill-prompts/per-phase.md) § proposal (scope contract, minimal-by-default, uncertainties) and § specs (implicit branch tracing). Scope contract and implicit-branch obligations MUST still be satisfied; deliver them as assumptions to correct when applicable ([grill-prompts/planning.md](grill-prompts/planning.md) ## Assumptions grill mode).
3. **One topic per turn** — structured prompt per assumption or small related group; plain-text fallback: one assumption per turn.
4. **User action** — each item: confirm, correct, or expand; record corrections in **User inputs** as plain prose bullets.
5. **Forbidden:** numbered decision ids (`D-NN`), strict id-match coverage gates, or summarizing assumptions from planning artifacts without user replies to assumption prompts.

## When to recommend grill mode

Parent MAY label **at most one** option "(Recommended)" in the planning grill mode offer — user still chooses. **Default when unsure: no recommendation label** — present all modes neutrally.

**Gather MUST NOT recommend modes** — gather returns question script only ([agent-tacos-grill-gather.md](../../tacos-doctor/templates/agents/agent-tacos-grill-gather.md)). Parent decides recommendation from this section; **forbidden:** copying gather prose such as "Mode recommendation: Prefer assumptions".

|Situation|Recommend|
|-|-|
|Detailed scope contract already in session (Intent, prior grill **User inputs**, explore conclusions under `## explore`)|`short` or `defaults`|
|After **explore** with crystallized decisions recorded — confirm/decide, not re-discover|`short`|
|Docs-only / procedural / single-surface skill or config edit|`short` or `defaults`|
|Terse invoke **and** high implicit-branch risk **and** no prior scope artifact in session|`assumptions`|
|User wants full discovery or domain is unknown|`full`|

**Do not recommend `assumptions` when:**

- User already supplied a detailed scope contract in the same session (Ask, in/out, complexity, or explore conclusions) — offer **`short`** or **`defaults`**
- **tacos-work** Phase 0 **Intent** already bounds scope — offer **`short`**
- Prior explore or propose text exists **but** conclusions are already recorded — **`short`**, not assumptions
- Only "repo context exists" or "branches are possible" without a terse, underspecified invoke

Prior explore or propose text alone does **not** justify recommending **`assumptions`** when scope is already bound — that pattern is **`short`**.

**Plain-text fallback** — use the block matching the phase (user may reply by number or id):

Planning grill (`planning`, propose, ff, tacos-work Phase 1) — include `assumptions`:

```text
How do you want to grill for <phase>? (~N from script; gather: <low|medium|high> uncertainty)
1. Full — all topics from script until exhausted (~N from script; yaml baseline is guidance only)
2. Short — typical cap at 4 topics (may exceed with confirm when gather flagged high uncertainty)
3. Defaults — use gatherer defaults unless you correct
4. Assumptions — agent states assumptions for confirm/correct
5. Skip — skip this phase
```

Per-phase continue only — omit `assumptions`. Stage apply and triggered grills use separate prompts below (no gather step — see ## Mandatory stage apply and ## Triggered grill):

```text
How do you want to grill for <phase>? (~N from script; gather: <low|medium|high> uncertainty)
1. Full — all topics from script until exhausted (~N from script; yaml baseline is guidance only)
2. Short — typical cap at 4 topics (may exceed with confirm when gather flagged high uncertainty)
3. Defaults — use gatherer defaults unless you correct
4. Skip — skip this phase
```

## Script topics (one question per call)

For each item in the gather question script:

1. **One `AskQuestion` / `AskUserQuestion` per topic** when the script provides `choices` or the topic is a discrete decision.
2. Set `allow_multiple: true` only when the script explicitly allows multi-select (e.g. “pick all that apply for scope”).
3. Include in the prompt: `topic`, `question`, and one-line `why` when it helps.
4. When the script has a `default`, show it in the prompt text (e.g. “Default if you pick Defaults mode: …”) and as the recommended option label when useful.

**Option ids:** stable kebab-case derived from choice text (e.g. `task-stages`, `defer-e2e`). Map selection back to decisions for **User inputs**.

**No choices in script:** still prefer a single structured question with generated options (Yes / No / Other — explain in chat) or plain-text one topic per turn.

## Light grill

When re-grilling and `## <phase>` already exists, offer the same mode prompt with wording “Re-grill `<phase>`” and default to **short** unless the user asks for full.

## Mandatory stage apply (per `tasks.md` `## N` stage start)

When the **stage grill gate** is true ([task-stage-contract.md](../../tacos-orchestration/references/task-stage-contract.md) ## Stage grill gate) and orchestration starts a new apply stage, **STOP** on the unchecked `Stage grill:` line and run on the **parent** before **any** implementation checkbox in that stage. No gather step.

1. **Grill mode (once per stage)** — offer the same four options as **Grill mode (opening offer)** with wording `How do you want to grill for apply — stage <N> (<stage title>)?` Substitute stage number and title from `tasks.md`.
2. On **`skip`:** record waiver in chat and **User inputs** under `## apply` in `grill-summaries.md`; check off `Stage grill:`; do not block implementation.
3. On **`full` / `short` / `defaults`:** run stage-scoped topics per [grill-prompts/apply-mandatory.md](grill-prompts/apply-mandatory.md); satisfy ## Interview minimum (full / short / defaults) before checking off `Stage grill:` (`grill.default_max_questions` is gather guidance for planning — stage apply picks what applies; MUST NOT pad to a count — **not** limited to 1–3).
4. **Forbidden:** Treating grill mode selection as completing stage grill; treating stage grill as triggered apply (1–3 cap only); skipping the mode offer; checking off `Stage grill:` without topic interview or documented skip/waiver.

## Triggered grills (`apply`, `sync`, `explore`)

No gather step. Use structured prompts for each focused question (1–3 typical). **Skip** the full-mode menu — this subsection is for **signal-triggered** apply, sync, and explore only, **not** mandatory stage-start apply (see **Mandatory stage apply** above). Default to short focused questions without the four-mode menu.

## Change / phase disambiguation

When `/tacos-grill` or orchestration needs binding before gather:

|Situation|Prompt|Options|
|-|-|-|
|Multiple in-progress changes|`Which change?`|One option per change id|
|Phase unclear|`Which grill phase?`|`planning`, `proposal`, `specs`, `design`, `tasks`, `apply`, `sync`, `explore`|

Plain-text fallback: list choices and **end turn** after one ask if structured tools are unavailable.

## Never substitute for interview

- Treating grill mode selection (`full` / `short` / `defaults` / `assumptions`) as completing the interview, `Stage grill:`, or `grill.<phase>`.
- Writing **User inputs** or **Decisions** from `design.md`, `proposal.md`, `specs/`, `tasks.md`, or compressed `### Stage N` bullets without user replies to topic prompts in the same session.
- Dumping the full script as prose without waiting for answers.
- Delegating **interview** to Task or running summarize Task before grill mode is chosen.
- Using **explore**, the user's **propose** / **ff** message, or gather `answeredBy` as **User inputs** without parent interview (defaults mode still requires grill mode offer).
- Setting `grill.planning: complete` without `grill_mode` in frontmatter (see grill-summaries template).
- Proceeding to write planning artifacts on **propose** or **ff** while `grill.planning` is `pending` (except explicit **skip**).
- Proceeding on continue while `grill.<phase>` for the target artifact is `pending`.
- Treating silence or “continue” as **defaults** or **skip** — ask once, then stop.
- Completing or skipping without user input when structured tools are unavailable — use plain-text fallback per ## Structured prompt unavailable (or explicit user skip/waiver in chat).
- Using plain-text fallback or a prose-only approval menu when structured tools **are** available — use ## Structured prompt preferred instead.
