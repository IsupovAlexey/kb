# Review delegation (before walkthrough)

Gate findings feed **look outs** in the walkthrough ([walkthrough-document](walkthrough-document.md)). Complete reuse or delegation **before** writing walkthrough artifacts.

## Where gate output is saved

All under `artifacts/openspec-reviews/<change>/` (gitignored):

|File|Written by|
|-|-|
|`apply-review-assisted.md`|Fresh Task delegation from assisted review (preferred reuse target)|
|`spec-review-assisted.md`|Fresh Task delegation from assisted review (preferred reuse target)|
|`apply-review-<n>.md`|Staged apply or manual `/tacos-apply-review` (reusable when same diff)|
|`<artifact>-review.md`, `planning-bundle-review.md`, or `*-review-r2.md`|POST-ARTIFACT / manual planning spec review (reusable when same diff)|

Record paths used in `artifacts/assisted-review/<slug>/_session.md` → `gate_artifacts:` (with `(reused)` or `(fresh)`). Follow-up and later invokes read these files — no re-delegate unless diff changed or user requests refresh.

**Invocation:** Task subagent or Task tool when reuse is invalid ([runtime-delegation.md](../../tacos-orchestration/references/runtime-delegation.md)). Read `../../tacos-apply-review/SKILL.md`, `../../tacos-spec-review/SKILL.md`.

**Model:** Parent **MUST NOT** pass `model` from `openspec/tacos.yaml` on Task; the host applies `model:` from each installed agent file.

## Reuse gate artifacts

Before launching Task children, check `artifacts/openspec-reviews/<change>/` and the session compare in `artifacts/assisted-review/<slug>/_session.md` (or the resolved git range from this invoke).

|Priority|Read when present and **same diff**|
|-|-|
|1|`apply-review-assisted.md`, `spec-review-assisted.md`|
|2|Latest `apply-review-<n>.md` from staged apply; latest `<artifact>-review.md`, `planning-bundle-review.md`, or `*-review-r2.md` for the change|
|3|_(none)_ → run fresh delegation per tables below|

**Same diff** means the compare range matches `_session.md` or `_full.patch` was regenerated for this invoke; if the user changed PR/range, treat artifacts as stale and delegate fresh (or ask once).

**Reuse:** Load gate files for **look outs** only ([walkthrough-document](walkthrough-document.md)) — do not paste severity tables into the walkthrough. Note in chat one line which artifacts were reused.

**Fresh delegation:** No usable artifacts; diff changed; user says “refresh review” / “re-run apply-review”; or reuse would mix findings from a different compare.

Record reused paths in `_session.md` under `gate_artifacts:`.

## Scope (bound mode)

Classify from `git diff --name-only` or PR file list. **Reuse** matching artifacts per [Reuse gate artifacts](#reuse-gate-artifacts) before Task.

|Diff includes|Obtain apply findings|Obtain spec findings|
|-|-|-|
|Paths **outside** `openspec/changes/<id>/`|`apply-review-assisted.md` or delegate **`agent-tacos-apply-review`**|—|
|Paths **under** `openspec/changes/<id>/`|—|`spec-review-assisted.md` or delegate **`agent-tacos-spec-review`**|
|**Both**|Apply row + spec row (parallel Task when supported)||
|**Ad hoc** (no change id)|—|— ([rubric-adhoc](rubric-adhoc.md) only)|

This is **assisted-review’s** scope split for which gate artifacts to load — orchestration still never auto-runs `tacos-assisted-review` (manual invoke only per [SKILL.md](../SKILL.md)).

**Reuse vs fresh delegation** — see [Reuse gate artifacts](#reuse-gate-artifacts). Prefer reuse when artifacts match the current diff; delegate fresh when missing, stale, or the user asks to refresh.

## Apply review — parallel launch and parent merge

Follow [post-artifact-signoff.md](../../tacos-orchestration/references/post-artifact-signoff.md) **Apply review — parallel launch and parent merge** with these assisted-review overrides:

|Field|Assisted-review value|
|-|-|
|**Output path**|`artifacts/openspec-reviews/<change>/apply-review-assisted.md`|
|**Diff scope**|Resolved PR or git range; implementation paths only|
|**Inputs**|`openspec/changes/<change>/` planning set|

When `review.apply_review_additional_skills` is empty → one fresh **`agent-tacos-apply-review`** subagent only.

When the array is non-empty and the runtime supports concurrent Task spawns:

1. **Applicability** — for each additional path, infer scope from skill name/description/path and match the review diff per [host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md) **Apply review applicability**. Skip spawn when inference is confident and zero diff paths match; record under **`## Skipped additional skills`** in the merge (omit section when empty).
2. Launch **parallel** subagents in **one parent turn** (do **not** stop after the core child returns):
   - **Core:** `subagent_type` **`agent-tacos-apply-review`** — parent owns merge; core child does **not** load additional skills ([host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md))
   - **Per applicable entry:** one **`agent-tacos-additional-apply-review`** per repo-relative path in `review.apply_review_additional_skills`
3. **Parent merge** into `apply-review-assisted.md`. Include **`## Skipped additional skills`** when any entry was applicability-skipped. On child failure, include `## Parallel delegation warnings` per post-artifact-signoff.

**Sequential fallback:** Applicability (step 1) then core then each **applicable** additional subagent in array order; note fallback in chat.

## Planning spec review — parallel launch and parent merge

Follow [post-artifact-planning-review.md](../../tacos-orchestration/references/post-artifact-planning-review.md) **Planning spec review — parallel launch and parent merge** with these overrides:

|Field|Assisted-review value|
|-|-|
|**Output path**|`artifacts/openspec-reviews/<change>/spec-review-assisted.md`|
|**Scope**|Planning paths present in the resolved diff|

When `review.spec_review_additional_skills` is empty → one fresh **`agent-tacos-spec-review`** subagent only. Parallel and sequential rules match POST-ARTIFACT; parent merge into `spec-review-assisted.md`.

## Task launch (non-normative)

Parent launches **named** subagents only. Substitute `<change>`, `<range-or-pr>`, and paths from the invoke.

**Apply core (Cursor)**

```text
Task({
  description: "assisted apply-review",
  subagent_type: "agent-tacos-apply-review",
  prompt: "Run tacos-apply-review for change <change>. Diff: <range-or-pr>. Write artifacts/openspec-reviews/<change>/apply-review-assisted.md. Parent is tacos-assisted-review — do not load review.apply_review_additional_skills; parent merges parallel children."
})
```

**Spec core (Cursor)**

```text
Task({
  description: "assisted spec-review",
  subagent_type: "agent-tacos-spec-review",
  prompt: "Run tacos-spec-review for change <change>. Scope: planning files in this diff only. Write artifacts/openspec-reviews/<change>/spec-review-assisted.md. Parent is tacos-assisted-review — do not load review.spec_review_additional_skills; parent merges parallel children."
})
```

**Additional apply child**

```text
Task({
  description: "assisted apply-review host skill",
  subagent_type: "agent-tacos-additional-apply-review",
  prompt: "Apply host skill <repo-relative-path> for change <change>; assisted diff <range-or-pr>. Parent merges into apply-review-assisted.md; cite skill path on each finding."
})
```

**Additional spec child**

```text
Task({
  description: "assisted spec-review host skill",
  subagent_type: "agent-tacos-additional-spec-review",
  prompt: "Apply host skill <repo-relative-path> to change <change> planning artifacts in diff scope. Parent merges into spec-review-assisted.md; cite skill path on each finding."
})
```

**Claude Code** — same contract: spawn by agent `name:`; model from installed agent file.

## After delegation

1. Read gate artifacts per [Reuse gate artifacts](#reuse-gate-artifacts) (assisted or staged files).
2. Build [walkthrough-document](walkthrough-document.md); deliver per [output-delivery](output-delivery.md).

|Gate signal|Walkthrough use|
|-|-|
|Apply BLOCKER/CRITICAL/MAJOR themes|**Look outs** under the matching walkthrough stop — not gate table|
|Spec gaps|**Look outs** or narrative on the planning/spec stop|
|Doc drift, CRLF|**Look outs** on the file stop they affect — not author verify checklist|

**Overview** = what the change does (descriptive prose only). Do not paste `*-assisted.md` into Overview.

## Forbidden

- **Inline** `agent-tacos-apply-review` or `agent-tacos-spec-review` on the parent when Task is supported
- Writing canvas or assisted markdown **before** applicable delegation completes or is waived
- Copying gate review markdown wholesale into the walkthrough artifact
