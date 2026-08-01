# Entry conditionals (yaml-gated reads)

Load from propose or apply read graphs when flags match — not on explore-only turns.

|When|Read|
|-|-|
|`orchestration.grill_enabled` + planning path|[../../tacos-grill/references/read-graphs/planning.md](../../tacos-grill/references/read-graphs/planning.md), [grill-gates.md](grill-gates.md) or [triggered-grill.md](../../tacos-grill/references/triggered-grill.md) per graph|
|`jira.enabled` or Jira URL in message|`../../tacos-jira-sync/SKILL.md`, [jira-hooks.md](jira-hooks.md)|
|`openspec/changes/<name>/jira.md` exists (planning path)|[planning-artifact-loop.md](planning-artifact-loop.md) ### `jira.md` as planning context; [jira-hooks.md](jira-hooks.md) — authoritative ticket scope before `proposal` / `specs` / `design` / `tasks` writes even when `jira.enabled` is false|
|`orchestration.planning_review_enabled` + POST-ARTIFACT|`../../tacos-spec-review/SKILL.md`; non-empty `review.spec_review_additional_skills` → [host-additional-skills.md](../../tacos-spec-review/references/host-additional-skills.md)|
|`orchestration.e2e_enabled` + POST-ARTIFACT|`../../tacos-e2e-scenarios/SKILL.md`|
|`apply` + `orchestration.staged_apply_enabled`|`../../tacos-apply-review/SKILL.md`, [review-gate-pass.md](review-gate-pass.md), [pipeline-and-verification.md](../../tacos-apply-review/references/pipeline-and-verification.md)|
|`apply` + non-empty `review.apply_review_additional_skills`|[host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md)|
|`apply` + `tdd.md` in change folder|`../../tacos-tdd/SKILL.md`, [tdd-apply-contract.md](tdd-apply-contract.md)|
