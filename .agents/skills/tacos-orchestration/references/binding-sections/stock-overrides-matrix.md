# Stock overrides matrix

When `orchestration.grill_enabled` is true, stock opsx and host prompts may conflict with tacos. **MUST NOT** follow the stock column. `SKILL.md` **Stock overrides (hub)** carries a short highest-violation mini-table; this file is the full matrix.

Hub router: [orchestration-binding.md](../orchestration-binding.md). Grill gates: [grill-gates.md](../grill-gates.md). Triggered: [../../../tacos-grill/references/triggered-grill.md](../../../tacos-grill/references/triggered-grill.md).

|Stock / host tendency|tacos MUST / MUST NOT|
|-|-|
|“Prefer reasonable decisions” instead of interviewing|Run **tacos-grill** planning when `grill.planning` is `pending`|
|Batch-create planning artifacts on **propose** / **ff**|**STOP** until planning grill completes and `grill-summaries.md` is filled|
|Empty `grill-summaries.md` template; OpenSpec marks artifact `done`|**STOP** — file existence ≠ `grill.planning` complete|
|**`explore`** or `## explore` as prior grilling|**STOP** — not a substitute for `grill.planning` or phase grills; **`explore`** uses triggered grill only|
|Write `proposal` … `tasks` while `grill.planning` is `pending`|**Forbidden**|
|Skip gather/summarize delegation|Gather/summarize **MUST** use Task when supported; interview on parent|
|Summarize or artifacts before parent interview|**Forbidden** — gather → interview → summarize → `grill-summaries.md`|
|Explore / propose message as **User inputs**|**Forbidden** without grill mode + interview|
|Copy explore into phase sections without **User inputs**; backfill frontmatter without interview|**Forbidden**|
|Same-turn **explore** conclusion then **propose** / **ff** while `grill.planning` is `pending`|**STOP** — end explore turn; next turn runs planning grill before `grill-summaries.md` or planning artifacts|
|Prefilled `grill-summaries.md` from explore (phases `complete`, explore-derived **User inputs**, cross-phase "Same as proposal")|**STOP** — reset and re-run planning grill; file existence ≠ interview complete|
|Stock **apply** “pause if unclear” or inline guess when `grill.triggers.*` matches|**STOP** → **tacos-grill** per [../../../tacos-grill/references/triggered-grill.md](../../../tacos-grill/references/triggered-grill.md)|
|One **`tacos-apply-review`** Task when `review.apply_review_additional_skills` has applicable entries|**Forbidden** on initial stage pass — parallel **`tacos-additional-apply-review`** per applicable path (skip per applicability); parent merge ([post-artifact-signoff.md](../post-artifact-signoff.md) **Apply review — parallel launch**)|
|One **`tacos-spec-review`** Task when `review.spec_review_additional_skills` is non-empty|**Forbidden** on POST-ARTIFACT initial pass — parallel **`tacos-additional-spec-review`** per path; parent merge ([post-artifact-planning-review.md](../post-artifact-planning-review.md) **Planning spec review — parallel launch**)|
|Parent reads implementation during **specs** for grounding when Task is supported|**STOP** — launch **`tacos-spec-grounding`** (or host **explore** `readonly: true` when named agent missing) per [spec-grounding-explore.md](../spec-grounding-explore.md); merge child bullets before spec write|
|Skip **`tacos-spec-grounding`** when installed and Task is supported|**Forbidden** — inline parent reads are fallback only when Task is unavailable|
|Serial parent Grep/Read on implementation before delegation when Task is supported|**MUST NOT** on **apply** discovery — delegate per [proactive-explore-delegation.md](../proactive-explore-delegation.md); **SHOULD NOT** on **explore** and unfamiliar areas|
|Check off **`Apply review:`** or **`Human review:`** when merged review Summary is not **`APPROVE` + `Ready`**|**Forbidden** — [review-gate-pass.md](../review-gate-pass.md) remediation loop until pass or human waiver|
|Check off **`Apply review:`** when merged review omits required **Pipeline trace** (pipeline/gate in scope) or open **Verify Decision N** / pipeline **BLOCKER**s remain|**Forbidden** — [pipeline-and-verification.md](../../../tacos-apply-review/references/pipeline-and-verification.md) + review-gate-pass remediation|
|Planning sign-off or apply handoff while planning review Summary is not **`APPROVE` + `Ready`**|**Forbidden** — fix planning artifacts → delta re-review per [review-gate-pass.md](../review-gate-pass.md)|
|Parent narrates fixes ("BLOCKERs resolved", "fixed before handoff") without fresh re-review artifact r(N+1) and **`APPROVE` + `Ready`** on latest Summary|**Forbidden** — [review-gate-pass.md](../review-gate-pass.md) ## Anti-short-circuit; append **Dynamic re-review checkbox**; [Turn-summary delegation record](../review-gate-pass.md#turn-summary-delegation-record); planning → delta re-review; apply → re-review after fixes|
|Parent writes `*-review-r2.md` or increment without fresh Task re-review|**Forbidden** — [review-gate-pass.md](../review-gate-pass.md) ## Same-turn STOP|
|Check off **`Apply review:`** while a pending **Re-review after fixes** / **Delta re-review after fixes** line remains unchecked|**Forbidden** — [review-gate-pass.md](../review-gate-pass.md) **Dynamic re-review checkbox**|
|Stock opsx "Ready for implementation" or planning complete while latest review Summary fails gate pass|**Forbidden** when `orchestration.planning_review_enabled` or staged **`Apply review:`** applies — read latest `artifacts/openspec-reviews/**` Summary first|
|Stock **update** skip-grill or inline guess when `grill.update` pending and `orchestration.grill_enabled`|**STOP** — run **tacos-grill** update phase per [grill-gates.md](../grill-gates.md) § Update grill|
|Batch-write revised artifacts without per-artifact user confirm on **update**|**STOP** — preserve stock per-artifact confirm before each write|
|Create missing artifacts or new files under glob artifacts on **update**|**Forbidden** — redirect to **propose** or **continue**; note deferred paths for user|
|Treat scope-pivot or missing-frontier as refinement on **update**|**STOP** — redirect to **propose** per [read-graphs/update.md](../read-graphs/update.md) ## Before command|
|Embed `Verify Decision N` inside implementation checkbox text|**Forbidden** — separate `- [ ] Verify Decision N:` row per [planning-artifact-loop.md](../planning-artifact-loop.md) ### Generation-time contract and schema `tasks` instruction|
|Write specs delta without main-spec cross-check or missing **Modified Capabilities** entry|**STOP** — research `openspec/specs/<capability>/spec.md`; update proposal **Modified Capabilities** before contradicting delta per schema `specs` instruction|
|Shorten **Apply review:** line to "Invoke tacos-apply-review" or one-line pointer without `tacos-additional-apply-review` / `parent merge` / `**MUST NOT** core-only`|**Forbidden** — emit medium checklist line per [task-stage-contract.md](../task-stage-contract.md) ## Apply review line (first fenced block); review floor pointer alone is insufficient for generated `tasks.md`|
