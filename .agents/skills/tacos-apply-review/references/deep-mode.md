# Deep discovery sweep (apply review)

Manual-only discovery on an **unchanged** implementation diff. Not staged **Apply review:**, not tacos-work Phase 4 auto path, not host `/loop`, not pr-triage `loop`. After substantive diff edits, use [review-format.md](review-format.md) ## After fixes — not continued `apply-review-deep-N`.

## Invoke

|Form|Example|
|-|-|
|Deep|`/tacos-apply-review deep`|
|Deep + cap|`/tacos-apply-review deep 10`|
|With change + stage|`/tacos-apply-review add-feature 2 deep`|

Parse tokens: `deep`; optional positive integer immediately after `deep` overrides `review.deep_max_iterations` for this invoke. Invalid, zero, or negative cap → yaml default.

Config: `openspec/tacos.yaml` — `review.deep_max_iterations` (default **5**).

## Parent loop (mandatory)

The **parent** invoker owns iteration — not the review subagent.

1. Resolve change or tacos-work slug, diff scope, and output directory.
2. **Pass 1:** Fresh Task `agent-tacos-apply-review` — one child per pass; **no** parallel `tacos-additional-apply-review` siblings during deep.
3. **Pass N>1:** Fresh Task with prior `apply-review-deep-*.md` + unchanged diff; output `apply-review-deep-N.md`.
4. Read **## Deep pass outcome** from the latest artifact.
5. **Stop** when `new_hard_major_count: 0` → `stopped: stable`.
6. **Stop** when `pass_number >= max_iterations` with `new_hard_major_count > 0` → `stopped: cap`.
7. After stable deep: user fixes diff → re-review after fixes on **changed** diff.

**Forbidden:** parent-authored `apply-review-deep-N.md`; inline multi-pass in one subagent; auto-`deep` from orchestration or tacos-work Phase 4; parallel `tacos-additional-apply-review` swarm per deep pass.

## Child pass (subagent)

Each deep pass is one bounded `tacos-apply-review` run inside a single fresh Task child.

1. **Core dual-pass** — pass 1 → pass 2 when pass 1 and main-spec drift are clean per existing workflow.
2. **Additional skills** — when `review.apply_review_additional_skills` is non-empty, load [host-additional-skills.md](host-additional-skills.md) and run each applicable entry **sequentially** in the **same** child (mirror re-review after fixes). When the array is empty or missing, stop after core dual-pass.
3. **Applicability** — same rules as re-review; record skips under **`## Skipped additional skills`** (omit when empty).

- **Inputs:** stage or session diff; planning context per [SKILL.md](../SKILL.md) ## Input; all prior `apply-review-deep-*.md` for this sweep.
- **New-only rule:** cumulative prior deep findings matched by **location + essence** across core **and** additional findings. New BLOCKER/CRITICAL/MAJOR only in **## New this pass**.

### Artifact naming

|Context|Path|
|-|-|
|Full tacos / change|`artifacts/openspec-reviews/<change>/apply-review-deep-N.md`|
|Staged stage|`artifacts/openspec-reviews/<change>/apply-review-<stage>-deep-N.md` when stage scope is explicit|
|tacos-work|`artifacts/openspec-reviews/<slug>/apply-review-deep-N.md`|

Default unstaged manual invoke: `apply-review-deep-N.md`.

### Required sections (add to [review-format.md](review-format.md) shape)

```markdown
## Deep pass outcome

- mode: deep
- pass_number: <N>
- max_iterations: <cap>
- new_hard_major_count: <count of new BLOCKER+CRITICAL+MAJOR this pass>
- new_minor_count: <optional>
- stopped: pending | stable | cap
- prior_deep_artifacts: <paths>

## New this pass

_(Only findings not in cumulative prior deep artifacts — core and additional.)_

## Prior sweep (unchanged)

_(Optional — hard/major still open from earlier passes.)_

## Skipped additional skills

_(Optional — applicability-skipped entries; omit when empty.)_
```

**Summary Status / Readiness:** cumulative open severity across the sweep. `new_hard_major_count` drives parent stop only.

## Stop semantics

|Condition|Parent action|
|-|-|
|`new_hard_major_count: 0`|End deep loop|
|`pass_number >= max_iterations` and `new_hard_major_count > 0`|End deep loop; `stopped: cap`|
|User `stop deep`|End loop partial|

## vs re-review after fixes

||Deep|Re-review r2+|
|-|-|-|
|Trigger|Manual `deep` invoke|After diff **edits** from review|
|Scope|Unchanged diff|Changed diff|
|Naming|`apply-review-deep-N.md`|`apply-review-*-r2.md`|
|Additional skills|Sequential in same child when array non-empty|Sequential in same child when array non-empty|
