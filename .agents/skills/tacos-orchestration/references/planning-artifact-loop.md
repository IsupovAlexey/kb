# Planning artifact loop (canonical)

When `openspec/tacos.yaml` has `orchestration.grill_enabled: true`. Gates and frontmatter: `grill-gates.md`.

## `propose` and `ff`

1. `openspec new change` (propose only) → `openspec status --change <name> --json`
2. **Planning grill:** If `grill.planning` is `pending`, run **tacos-grill** phase `planning` in order: gather (Task) → **parent interview** (grill mode + topics via structured prompt per [structured-gate-convention.md](structured-gate-convention.md); plain-text only when tools absent) → summarize (Task) **before** `grill-summaries.md`.
   - **STOP if** **explore** concludes and **propose** / **ff** runs in the same turn before planning grill completes — do not batch artifact writes ([explore.md](read-graphs/explore.md) ## After explore / before propose).
   - **STOP if** interview was skipped (explore/propose/gather treated as **User inputs**).
   - **STOP if** prefilling `grill-summaries.md` from explore text, propose message, or gather `answeredBy` without parent interview (including `grill_mode: short` / `assumptions` with explore-derived **User inputs**).
   - **STOP if** structured tools are unavailable and grill was completed or skipped without plain-text interview per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable (or explicit skip/waiver in chat).
   - **STOP if** summarize ran before parent interview.
   - **STOP if** `grill.planning` still `pending` after this step — do not create an empty template.
3. **`grill-summaries`:** Create or update from grill result (`grill.planning` and phase keys `complete` or `skipped`; filled phase sections).
   - **STOP if** `grill.planning` is `pending` — do not write `proposal` … `tasks`.
4. **Planning artifacts:** When each of `proposal`, `specs`, `design`, `tasks` is `ready`, `openspec instructions` → write → re-run status (host batch OK after step 3).
   - **MUST** read [artifact-prose.md](artifact-prose.md) immediately before writing each planning artifact (`proposal`, `specs`, `design`, `tasks`) — per-artifact load; propose read graph lists the same obligation.
   - **MUST** apply [### Generation-time contract](#generation-time-contract-propose--ff--continue) row for that artifact before write (not only `artifact-prose.md`).
   - Resolve discoverable facts from existing artifacts before re-asking the human — [artifact-prose.md](artifact-prose.md) ## Discoverable facts.
   - **STOP if** `grill.planning` is `pending` before any planning artifact write.

Stock host “create all artifacts in sequence” is **allowed** only after step 3 completes. Incorporate `grill-summaries.md`; do not contradict resolved decisions.

### Generation-time contract (propose / ff / continue)

Before writing each planning artifact, apply schema instruction gates (source: `openspec instructions <artifact>` / tacos schema):

|Artifact|Gate|
|-|-|
|**proposal**|Read `openspec/specs/<capability>/spec.md` per touched capability; list under **Modified Capabilities** when this change alters, extends, or contradicts main-spec obligations (including sibling capabilities of the primary delta)|
|**specs**|Main-spec cross-check before delta write — **STOP** and update proposal **Modified Capabilities** first when a delta alters/contradicts an unlisted capability; for multi-step flows add failure / unavailable-dependency / partial-completion scenarios (not happy-path-only)|
|**tasks**|Separate `- [ ] Verify Decision N:` rows after implementation (before Tests) when design **Decision N** or grill **User inputs** cover pipeline ordering, gate-before-I/O, negative/error paths, or scope boundaries — FORBIDDEN embedding `Verify Decision N` inside implementation checkbox text; **Apply review** line MUST use the medium checklist from task-stage-contract.md ## Apply review line (`parallel Task`, `tacos-apply-review` + `tacos-additional-apply-review`, parent merge, **MUST NOT** core-only) — FORBIDDEN one-line pointer or `Invoke tacos-apply-review` alone|

Detail: [task-stage-contract.md](task-stage-contract.md) ## Per-stage checklist order (Verify Decision separate rows; Apply review FORBIDDEN shortened).

### fast-forward (`ff`) never skips (when orchestration is on)

**`ff`** fast-forwards planning artifact **writes** after the same gates as **`propose`** — it is not a bypass for quality gates. When `orchestration.grill_enabled` is true, **`ff` does not skip:**

|Gate|Notes|
|-|-|
|**Planning grill**|gather → parent interview → summarize before `grill-summaries.md` when `grill.planning` is `pending`|
|**POST-ARTIFACT**|e2e (when enabled), spec review, `openspec validate --strict`, human sign-off before apply handoff|
|**Per-phase grill on `continue`**|`ff` batches planning artifacts in one run; per-artifact phase grills run on **`continue`**, not on `ff`|
|**Explore / propose / gather as User inputs**|Prior chat, explore, or gather `answeredBy` does **not** satisfy planning grill|
|**Generation-time contract**|Apply schema instruction gates per artifact during `ff` batch writes — same table as [### Generation-time contract](#generation-time-contract-propose--ff--continue); `ff` does not skip Verify embed FORBIDDEN, Modified Capabilities research, specs main-spec **STOP**, or Apply review **medium** line (not one-line pointer)|

**Apply-time only (not `ff` planning):** stage-start apply grill, triggered grill during apply, and staged apply-review run on **`apply`** — see [task-stage-contract.md](task-stage-contract.md) and [triggered-grill.md](../../tacos-grill/references/triggered-grill.md).

Detail: [grill-gates.md](grill-gates.md) § `ff` vs `continue`.

## `continue`

1. `openspec status --change <name> --json` → next artifact `ready`
2. If `grill.planning` is `pending`, run **tacos-grill** phase `planning` (gather → parent interview → summarize) and fill `grill-summaries.md` — **even if** OpenSpec marks `grill-summaries` `done`.
   - **STOP if** interview skipped or summarize ran before interview.
   - **STOP if** `grill.planning` is `pending` — do not write `proposal` yet.
3. If next artifact is planning (`proposal`, `specs`, `design`, `tasks`):
   - `grill.planning` MUST be `complete` or `skipped`.
   - If `grill.<phase>` is `pending`, run **tacos-grill** for that phase (gather → parent interview → summarize); **STOP** if summarize before interview.
   - **MUST** read [artifact-prose.md](artifact-prose.md) immediately before writing that artifact — same obligation as ## `propose` and `ff` step 4.
   - **MUST** apply [### Generation-time contract](#generation-time-contract-propose--ff--continue) row when the artifact is `proposal`, `specs`, or `tasks`.
   - When next artifact is **`specs`** and grounding on existing behavior is needed: launch **`tacos-spec-grounding`** per surface per [spec-grounding-explore.md](spec-grounding-explore.md) **Procedure** — parallel when ≥2 surfaces — **before** writing delta specs; parent merges observable-behavior bullets only.
   - Write **only** that artifact; re-run status.
4. Jira / other artifacts per orchestration `SKILL.md`

### `jira.md` as planning context

When `openspec/changes/<name>/jira.md` exists, the parent **MUST** read it before writing or updating `proposal`, `specs`, `design`, or `tasks` on **`propose`**, **`ff`**, or **`continue`**. Treat `jira.md` as authoritative ticket scope for those artifacts — do not re-infer scope from chat alone or duplicate the ticket body into `proposal.md`. When a new artifact would contradict `jira.md`, flag the conflict and resolve with the user before writing.

Detail: [jira-hooks.md](jira-hooks.md) **Planning-phase URL detection**.

Gather/summarize **MUST** use Task when supported. Interview **MUST** run on the parent.

When `orchestration.grill_enabled` is false, use stock opsx artifact steps only.

## Content ownership

Four planning layers — **proposal → specs → design → tasks** — each adds **delta only**. Do not restate content owned by another layer. Grill capture stays in `grill-summaries.md`; artifacts encode resolved decisions only.

|Content type|Primary artifact|Others may|
|-|-|-|
|Problem / why now|proposal **Why**|design **Context** (1 line)|
|Delivery outcomes|proposal **What Changes**|—|
|Capability inventory|proposal **Capabilities**|specs (by folder name)|
|Exclusions, non-goals, untouched subsystems, config deferrals|design **Boundaries** (≤3 brief lines)|grill-summaries **Decisions** note routing; tasks for install verification; specs MUST NOT use negative-only requirements|
|Behavioral obligations|specs|design/tasks cite Requirement title|
|Technical decisions + rationale|design **Decisions**|tasks cite Decision N|
|APIs, modules, procedure for this change|design|tasks name paths|
|Change-scoped done-when|tasks|apply-review|
|Decision verification (test/trace done-when)|tasks **Verify Decision N** rows; tacos-work **## Work**|apply-review [pipeline-and-verification.md](../../tacos-apply-review/references/pipeline-and-verification.md)|
|Grill capture|grill-summaries|artifacts encode decisions only|

Schema artifact instructions state **what this layer owns**; they do not duplicate this table. **tacos-spec-review** mirrors it for planning review enforcement.
