# Update grill prompts

**update** only — one session before reconciling **existing** planning artifacts. Not greenfield `grill.planning`.

Order: gather (read current artifacts + status) → **parent interview** (grill mode + topics) → summarize → set `grill.update` → reconcile. Gather uncertainty sizing and unknown-angle lenses — [planning.md § Gather uncertainty sizing](planning.md#gather-uncertainty-sizing) and [planning.md § Unknown-angle selection lenses (gather)](planning.md#unknown-angle-selection-lenses-gather).

Do not ask greenfield planning questions (motivation from scratch, full capability inventory, work breakdown for a new change). The change already exists — grill **revision**, **ripple**, and **guardrails**.

## Interview stance

|Planning grill asks…|Update grill asks…|
|-|-|
|What are we building?|What is changing, and what stays the same?|
|Which capabilities are new?|Which **existing** artifacts/files move (`existingOutputPaths` only)?|
|Full requirements and design from zero|Where do artifacts contradict each other after the edit?|
|Task breakdown for new work|Which checked-off apply work is now stale?|

## Topics (full mode)

1. **Revision mode** — targeted edit (user named an artifact/section) vs coherence pass (user said "update" / "make coherent" with no target). For coherence: which contradictions or gaps to resolve first?
2. **Anchor edit** — what is the primary change? If the user named one artifact, treat that as the anchor; other edits are ripple only when needed for consistency.
3. **Ripple scope (bidirectional)** — which other **existing** artifacts may need edits in **any** direction (later artifact change may require revising an earlier one). List concrete paths from `existingOutputPaths`; do not advance the build frontier.
4. **Minimal delta** — what must **not** change (resolved planning decisions, shipped scope, non-goals)? Challenge scope creep disguised as "alignment."
5. **Redirect check** — refinement vs scope-pivot vs missing-frontier. Scope-pivot or missing files → **propose** / **continue**; otherwise stay on **update**.
6. **Apply state** — are implementation tasks checked off? If yes, will this revision change behavioral obligations? → full-resync + **apply** handoff after reconcile; no code edits this turn.
7. **Confirm strategy** — stock **update** confirms each artifact before write; note which revisions the user may reject and leave unchanged.

## Short / defaults

- **Short:** topics 1–3 and 5.
- **Defaults:** topic 1 plus redirect check when ambiguous.

## Gather hints (update phase)

Before interviewing, read `openspec status --json` for the change and the artifacts the user named. Summarize current contradictions — do not propose writes. Gather bullets should cite **existing** file paths and tension points, not greenfield opportunity framing.

## Anti-patterns

- Re-running planning motivation ("why this change exists") when `grill.planning` already completed — read `grill-summaries.md` instead.
- Treating **update** as **ff** batch rewrite of all planning artifacts.
- Inventing new artifact files or glob children — defer to **continue**.
- Asking implementation-quality lenses for net-new scope; lenses apply only when the **revision** introduces a new behavioral obligation.

## Anti-procedure

Do not put agent workflows, pipeline order, or checklist templates in specs; capture runbooks in design and orchestration references.
