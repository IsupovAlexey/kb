# Deep discovery sweep (planning review)

Manual-only discovery on **unchanged** planning artifacts. Not POST-ARTIFACT default, not host `/loop`, not pr-triage `loop`. After substantive artifact edits, use [delta-r2.md](delta-r2.md) — not continued `*-deep-N`.

## Invoke

|Form|Example|
|-|-|
|Deep|`/tacos-spec-review deep`|
|Deep + cap|`/tacos-spec-review deep 10`|
|With change|`/tacos-spec-review add-feature deep`|

Parse tokens: `deep`; optional positive integer immediately after `deep` overrides `review.deep_max_iterations` for this invoke. Invalid, zero, or negative cap → yaml default.

Config: `openspec/tacos.yaml` — `review.deep_max_iterations` (default **5**).

## Parent loop (mandatory)

The **parent** invoker owns iteration — not the review subagent.

1. Resolve change, artifact scope, and output basename (e.g. `planning-bundle-review` or `<artifact>-review`).
2. **Pass 1:** Fresh Task `agent-tacos-spec-review` — one child per pass; **no** parallel `tacos-additional-spec-review` siblings during deep.
3. **Pass N>1:** Fresh Task with prior `*-deep-*.md` paths + unchanged artifact paths; output `*-deep-N.md`.
4. Read **## Deep pass outcome** from the latest artifact (below).
5. **Stop** when `new_hard_major_count: 0` → set `stopped: stable`.
6. **Stop** when `pass_number >= max_iterations` with `new_hard_major_count > 0` → set `stopped: cap`.
7. After stable deep: user fixes artifacts → [delta-r2.md](delta-r2.md) on **changed** scope.

**Forbidden:** parent-authored `*-deep-N.md`; inline multi-pass in one subagent thread; auto-`deep` from orchestration POST-ARTIFACT; parallel `tacos-additional-spec-review` swarm per deep pass.

## Child pass (subagent)

Each deep pass is one bounded `tacos-spec-review` run inside a single fresh Task child.

1. **Core rubric** — load full [dimensions.md](dimensions.md) every deep pass.
2. **Additional skills** — when `review.spec_review_additional_skills` is non-empty, load [host-additional-skills.md](host-additional-skills.md) and run each applicable entry **sequentially** in the **same** child (mirror delta r2). When the array is empty or missing, stop after core rubric.
3. **Applicability** — same rules as delta r2; record skips under **`## Skipped additional skills`** (omit when empty).

- **Inputs:** current planning artifacts; all prior `*-deep-*.md` for this sweep.
- **New-only rule:** compare to cumulative prior deep findings by **location + essence** (file/section + defect meaning) across core **and** additional findings. Do not re-emit prior findings as new rows in **Must address** — list them under **## Prior sweep (unchanged)** or omit when unchanged.
- **Emit new rows** only in **## New this pass** (then merge into **Must address** for Summary severity alignment).

### Artifact naming

|Pass|Path|
|-|-|
|1|`artifacts/openspec-reviews/<change>/<basename>-deep-1.md`|
|N|`artifacts/openspec-reviews/<change>/<basename>-deep-N.md`|

`<basename>` matches single-pass naming (`planning-bundle-review`, `proposal-review`, …).

### Required sections (add to [output-template.md](output-template.md) shape)

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

_(Only findings not in cumulative prior deep artifacts — location + essence; core and additional.)_

## Prior sweep (unchanged)

_(Optional — hard/major still open from earlier deep passes; not counted in new_hard_major_count.)_

## Skipped additional skills

_(Optional — applicability-skipped entries; omit when empty.)_
```

**Summary Status / Readiness:** reflect **cumulative** open BLOCKER/CRITICAL/MAJOR across all deep passes, not only this pass. `new_hard_major_count` is for parent stop logic only.

## Stop semantics

|Condition|Parent action|
|-|-|
|`new_hard_major_count: 0`|End deep loop; report stable finding set|
|`pass_number >= max_iterations` and `new_hard_major_count > 0`|End deep loop; report `stopped: cap`|
|User `stop deep`|End loop; report partial sweep|

MINOR and DEFERRED-only passes do **not** satisfy stable stop.

## vs delta-r2

||Deep|Delta r2|
|-|-|-|
|Trigger|Manual `deep` invoke|After artifact **edits** from review|
|Scope|Unchanged artifacts|Changed artifacts|
|Naming|`*-deep-N.md`|`*-review-r2.md`|
|New-only|vs prior **deep** artifacts|vs prior review + resolved/open|
|Additional skills|Sequential in same child when array non-empty|Sequential in same child when array non-empty|
