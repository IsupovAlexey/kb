# Planning grill prompts

**propose** and **ff** only for the full artifact bundle below — one session before any planning artifact. Order: gather script → **parent interview** (grill mode + topics) → summarize. Gather a single script from topics across **proposal**, **specs**, **design**, and **tasks** below; size from assessed uncertainty using `grill.default_max_questions` as a guidance baseline (may exceed or undershoot — see ## Gather uncertainty sizing). Summarize only after interview; set `grill_mode` and `grill.planning` plus phase frontmatter together. **## Gather uncertainty sizing** and **## Unknown-angle selection lenses (gather)** are shared gather guidance — per-phase and **update** routes link here for sizing and lens selection even when not running the full planning bundle.

## Gather uncertainty sizing

Before building the script, gather MUST assess uncertainty from invoke detail, scope clarity (in/out/complexity), repo familiarity, and unresolved branches. Use `grill.default_max_questions` as a starting baseline — not a target to hit.

|Assessed uncertainty|Script sizing|
|-|-|
|Low — bound scope, docs-only, or detailed prior session|Fewer topics than baseline when material coverage is satisfied — MUST NOT pad with noop questions|
|Medium — typical feature change|Near baseline; highest-impact topics first|
|High — terse invoke, unfamiliar domain, or material open branches|More topics than baseline when warranted — MUST NOT drop material topics solely to stay at baseline|

Gather output MUST document assessed uncertainty (low / medium / high) and script topic count vs yaml baseline. Parent passes assessed uncertainty to the grill mode offer (e.g. "~N from script; gather: high uncertainty").

**Forbidden:** noop or filler topics to reach the baseline; dropping scope contract, uncertainty, or implicit-branch obligations solely to stay at baseline.

## Unknown-angle selection lenses (gather)

When selecting script topics, gather MAY tag topics with unknown-angle lenses to reduce ambiguity — merge into **Uncertainties to verify** / assumptions work; do not add a separate unknowns summary section:

- **Known-unknowns** — gaps the user knows they have not decided yet
- **Unknown-knowns** — criteria the user recognizes when shown options or prototypes, but has not verbalized
- **Unknown-unknowns** — blind spots: what good looks like, historical potholes, questions they have not thought to ask
- **Blind-spot** — unfamiliar domain or codebase area; teach-before-interview probes
- **Reaction-vs-interview** — when showing 2–3 concrete options beats open-ended questions

Interview stays normal topic questions — angle tags are for gather prioritization, not mandatory jargon on every **User inputs** line.

Do not ask tasks-only minutiae if the change scope is still unclear — order topics motivation → scope contract → requirements → architecture → work breakdown.

**Scope contract and intent fidelity (mandatory):** Before artifacts are written, planning grill MUST cover scope contract, minimal-by-default, uncertainty, and implicit branch topics from [per-phase.md](per-phase.md) § proposal and § specs. Record outcomes in **User inputs**; route Out and rejected Adjacent to design **Boundaries**; route durable behavioral branches to specs scenarios or design **Decisions**.

When scope adds a gate or limiter to an existing code path, include **pipeline / call-order** in design or tasks topics per [implementation-quality-lenses.md](../implementation-quality-lenses.md) ## Pipeline and gate insertion.

**Implementation quality (planning home):** Before artifacts are written, decide which concerns **apply** using [implementation-quality-lenses.md](../implementation-quality-lenses.md) (**Applicability first**). **Only when** scope implicates a lens, planning grill **MUST** surface it in the interview and ensure outcomes land in the right artifacts (specs/design/tasks) — not deferred to apply. **MUST NOT** ask about any lens in [implementation-quality-lenses.md](../implementation-quality-lenses.md) when it does not apply; record **N/A** per lens instead. This is the primary path for implementation-quality topics in **implementation planning**, not a universal questionnaire.

Per-artifact topic bullets: [per-phase.md](per-phase.md).

### When to choose grill mode (parent recommendation)

Parent labels **at most one** "(Recommended)" option per [interview-prompt.md](../interview-prompt.md) ## When to recommend grill mode. Gather returns script only — no mode recommendation.

|Situation|Recommend|
|-|-|
|Detailed scope in session (Intent, explore conclusions, prior grill)|`short` or `defaults`|
|After explore with recorded conclusions — confirm/decide|`short`|
|Docs-only / procedural / single-surface edit|`short` or `defaults`|
|Terse invoke + high branch risk + no prior scope artifact|`assumptions`|
|Full discovery needed|`full`|
|Unsure|No label — neutral offer|

## Assumptions grill mode

When the user chooses **`assumptions`** in the planning grill mode offer ([interview-prompt.md](../interview-prompt.md) ## Grill mode (opening offer)):

1. **Read** gather script output and available repo context (paths, patterns, prior artifacts) before presenting assumptions.
2. **Present** a structured assumption list for confirm, correct, or expand — categories: scope interpretation, patterns, file locations, failure-handling defaults, implicit branches. When gather tagged script topics with unknown-angle lenses ([## Unknown-angle selection lenses](#unknown-angle-selection-lenses-gather)), map tags into assumptions: known-unknowns → open decisions to confirm; unknown-knowns → present 2–3 concrete options for reaction; unknown-unknowns and blind-spot → state what you assume about domain/codebase and ask correction; reaction-vs-interview → options not open-ended probes. Scope contract (in / out / adjacent), uncertainty surfacing, and implicit branch tracing MUST still be covered; express them as assumptions to correct rather than only as open interview questions when that is clearer.
3. **Record** outcomes in **User inputs** as plain prose bullets — same traceability as other grill modes. Route Out and rejected Adjacent to design **Boundaries**; route durable behavioral branches to specs scenarios or design **Decisions**.
4. **Plain-text fallback** — one assumption (or tight related group) per turn; user replies confirm, correct, or expand.
5. **Forbidden** — numbered decision ids (`D-NN`), strict id-match gates, or assumption text copied from `design.md` / `proposal.md` without user replies to assumption prompts.
