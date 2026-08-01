# Runtime delegation

tacos orchestration delegates steps to **named host subagents** when Task/subagent spawn is supported. **tacos-grill** uses three roles for grill phases. Map them to whatever your agent runtime provides.

## Roles (tacos-grill)

|Role|Intent|Runs on|
|-|-|-|
|**Gather**|Explore repo + schema context; output question script|Pre-artifact only; child when possible, else same session|
|**Interview**|Ask human one topic at a time|Parent agent (needs the user-facing chat)|
|**Summarize**|Compress Q/A into decision bullets|Child agent when possible; else parent|

## Delegate (gather / summarize)

**Gather** — pre-artifact phases only (`planning`, `proposal`, `specs`, `design`, `tasks`). When the runtime supports a **child agent with fresh context**, delegate gather via the **named host subagent** `agent-tacos-grill-gather` (see matrix below).

**Summarize** — pre-artifact may delegate via `agent-tacos-grill-summarize`; triggered grills summarize inline in the parent.

**Prompt-by-reference (gather / summarize / e2e):** Installed child bodies cite `grill-prompts.md` router → phase bundle (gather/summarize) or `format-and-workflow.md` (e2e) plus return contracts — not inlined full `SKILL.md` bodies. Templates: [../../tacos-doctor/templates/agents/](../../tacos-doctor/templates/agents/) (`agent-tacos-grill-gather.md`, `agent-tacos-grill-summarize.md`, `agent-tacos-e2e-scenarios.md`). Review agents keep verbatim parallel-launch and parent-merge contract lines.

**Apply review dispatch:** Parent copies `../../tacos-apply-review/references/apply-review-dispatch-prompt.md` verbatim per [post-artifact-signoff.md](post-artifact-signoff.md) **Dispatch contract**.

**Order (pre-artifact):** gather → **parent interview** → summarize. **Forbidden:** summarize before interview completes (including explore or propose message as substitute for **User inputs**).

If delegation is unavailable, run gather or summarize in the current session.

## Interview (parent)

Ask **one topic at a time**. Per [structured-gate-convention.md](structured-gate-convention.md): when structured tools are available, **MUST** use `AskQuestion` (Cursor) or `AskUserQuestion` (Claude Code) per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt preferred — grill mode opening, then one structured prompt per script topic. When structured tools are unavailable, use plain-text fallback per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable — **do not** skip interview or mark gates complete without user input.

Do not rely on a child agent for interactive user prompts.

## Reference hosts (Cursor and Claude Code)

tacos orchestration is written and tested against **Cursor** and **Claude Code**. Other agents that expose subagent/Task delegation and (when needed) multiple concurrent child launches should follow the same rules; hosts without those capabilities use lightweight-host bypass (inline on the parent).

|Capability|Cursor|Claude Code|Other hosts|
|-|-|-|-|
|Delegate gather / summarize / review|`Task` with `subagent_type` matching agent `name:`|Task / subagent spawn by name|Host equivalent when present|
|Model for delegated step|Host reads `model:` from installed agent file|Same|Per host docs|
|Parallel review children|Multiple `Task` calls in one turn when supported|Concurrent spawns when supported|Sequential fallback + note in summary|
|Grill interview (parent)|`AskQuestion`|`AskUserQuestion`|Plain-text fallback — [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable|

**Model configuration:** Per-step models live in `openspec/tacos.yaml` as `*_models` maps (`cursor` / `claude`). **tacos-doctor** renders them into each host’s agents directory frontmatter at install (`{{TACOS_MODEL}}` substitution). The orchestrator **MUST NOT** pass `model` from yaml on Task at runtime — the host applies the installed agent file.

After changing `*_models`, run **`/tacos-doctor config`** to refresh frontmatter on existing agent files (body unchanged), or edit `model:` in the installed file directly.

Official docs:

- [Cursor subagents — model configuration](https://cursor.com/docs/subagents#model-configuration)
- [Claude Code subagents — choose a model](https://code.claude.com/docs/en/sub-agents#choose-a-model)

## MUST-delegate matrix (host subagents)

When Task/subagent delegation is supported, launch the **stable agent `name:`** below. `subagent_type` (Cursor Task) or equivalent **MUST** match the agent `name:` frontmatter.

|Step|Agent `name:`|`openspec/tacos.yaml` key|
|-|-|-|
|Grill gather|`agent-tacos-grill-gather`|`grill.gather_models` — prompt-by-reference: `grill-prompts.md` router → phase bundle; script-only return|
|Grill summarize|`agent-tacos-grill-summarize`|`grill.summarize_models` — prompt-by-reference: `grill-prompts.md` router → phase bundle + summaries template|
|Planning spec review (core)|`agent-tacos-spec-review`|`review.spec_review_models`|
|Apply review (core)|`agent-tacos-apply-review`|`review.apply_review_models`|
|Additional spec review|`agent-tacos-additional-spec-review`|`review.additional_spec_review_models`|
|Additional apply review|`agent-tacos-additional-apply-review`|`review.additional_apply_review_models`|
|E2E scenarios|`agent-tacos-e2e-scenarios`|`orchestration.e2e_models` — prompt-by-reference: `format-and-workflow.md`; no overwrite|
|Spec grounding (specs phase)|`agent-tacos-spec-grounding`|`orchestration.spec_grounding_models`|
|Jira regenerate|`agent-tacos-jira-regenerate`|`jira.generate_desc_models`|
|Staged apply / tacos-work implement|`agent-tacos-apply-implement`|`orchestration.apply_implement_models` — isolated worktree; parent merges before apply review; return per [subagent-return-contracts.md](subagent-return-contracts.md) ## Implement|
|Planning or apply review fixes|`agent-tacos-orchestrator-fixes`|`orchestration.orchestrator_fixes_models` — failing review artifact path + scope only; fresh re-review after fixes|
|Implementation gates (host or scoped)|`agent-tacos-gate-runner`|`orchestration.gate_runner_models` — pass/fail + log path under `artifacts/outputs/`; ≤5 excerpt bullets; skip when apply-review Summary signals no code change per [task-stage-contract.md](task-stage-contract.md) ## Gate-runner skip|
|Dedrift detect|`agent-tacos-dedrift-detect`|`orchestration.dedrift_detect_models` — structured table ≤15 rows; no inline detect in parent when Task supported|

## Manual skill delegation (`/tacos-audit`)

When the user invokes `/tacos-audit` and Task/subagent delegation is supported, the parent MUST launch the installed agents below per [../../tacos-audit/SKILL.md](../../tacos-audit/SKILL.md); otherwise run category audit read-only in the parent session and run execute in an **isolated git worktree** per [../../tacos-audit/references/execute-runbook.md](../../tacos-audit/references/execute-runbook.md) — never mutate the user's working tree. Not orchestration POST-ARTIFACT — explicit audit slash invoke only.

|Step|Agent `name:`|`openspec/tacos.yaml` key|
|-|-|-|
|Category audit (read-only)|`agent-tacos-audit-explore`|`orchestration.audit_explore_models` — prompt-by-reference: `audit-explore-dispatch-prompt.md`; parallel per tier cap|
|Execute (worktree)|`agent-tacos-audit-executor`|`orchestration.audit_executor_models` — after execute confirm; inline full audit-plan text in child prompt (worktree still required)|

**Prompt-by-reference:** explore cites `audit-playbook.md` and `audit-explore-dispatch-prompt.md`; executor cites `execute-runbook.md` — not inlined full `tacos-audit/SKILL.md`. Parent owns recon, vetting, preview gates, and apply-review verdict after execute.

When Task is unavailable, parent runs explore scouts read-only in the parent session and runs execute in an isolated worktree per execute-runbook.md; notes `Inline audit explore` / `Inline audit execute (worktree)` in the post-run summary. The parent MUST NOT implement execute in the user's working tree.

Fallback: host `explore` subagent with `readonly: true` when `agent-tacos-audit-explore` is not installed — per [proactive-explore-delegation.md](proactive-explore-delegation.md).

## Manual skill delegation (`/tacos-test-plans`)

When the user invokes `/tacos-test-plans` and Task/subagent delegation is supported, the parent MUST launch the installed agents below when present per [../../tacos-test-plans/SKILL.md](../../tacos-test-plans/SKILL.md); otherwise run synthesis and plan review inline per skill references. Not orchestration POST-ARTIFACT — explicit QA slash invoke only.

|Step|Agent `name:`|`openspec/tacos.yaml` key|
|-|-|-|
|Test plan synthesis (after scope grill)|`agent-tacos-test-plans`|`orchestration.test_plans_models` — bundled defaults match `e2e_models` when key absent|
|Automated plan review (before preview)|`agent-tacos-test-plan-review`|`orchestration.test_plans_models` — same|

**Prompt-by-reference:** synthesis cites `test-case-format.md` and `format-and-workflow.md`; review cites `plan-review.md` — not inlined full `tacos-test-plans/SKILL.md`. Parent owns scope grill interview, preview gate, and pack writes.

When Task is unavailable, parent runs synthesis and plan review inline per skill references and notes `Inline execution` / `Inline plan review` in the post-run summary.

**Host install:** **tacos-doctor** copies templates from [../../tacos-doctor/templates/agents/](../../tacos-doctor/templates/agents/) into the host agents directory when that host is detected (copy-missing; `config` / `set-schema` syncs frontmatter `model:` from yaml without replacing bodies). Valid `model` ids, `*_models` keys, and refresh: [../tacos-doctor/references/config.md](../tacos-doctor/references/config.md). Installed agents root, host detection, and subagent drift: **`/tacos-doctor diagnose`** ([../tacos-doctor/references/schema.md](../tacos-doctor/references/schema.md)); per-host directory layout in your editor’s host documentation.

**Parallel additional review:** One child per path in `review.spec_review_additional_skills` via `agent-tacos-additional-spec-review`; one child per **applicable** path in `review.apply_review_additional_skills` via `agent-tacos-additional-apply-review` (applicability-skipped apply paths recorded in parent merge) — skill path in the prompt; not via per-entry yaml model overrides.

## Proactive explore (host explore subagent)

When Task is supported, the parent SHOULD delegate codebase search, navigation, and unfamiliar-area investigation to read-only **explore** children early — before serial parent-thread Grep/Read. Launch **multiple explore children in one parent turn** when independent questions target distinct surfaces.

During **specs** grounding, use **`agent-tacos-spec-grounding`** per [spec-grounding-explore.md](spec-grounding-explore.md) — not generic explore alone when the named agent is installed.

Detail: [proactive-explore-delegation.md](proactive-explore-delegation.md).

## Task launch (non-normative)

The parent launches **named** subagents only. Do **not** pass `model` from yaml.

**Cursor**

```text
Task({
  description: "spec-review core",
  subagent_type: "agent-tacos-spec-review",
  prompt: "Run tacos-spec-review for change <name>; scope: planning bundle; do not load review.spec_review_additional_skills — parent merges parallel children."
})
```

**Claude Code** — same contract: spawn by agent name; model comes from the installed agent file for that name.

Parallel review: launch one Task per child (core + each additional skill) **in the same parent turn** when the host allows concurrent spawns; do **not** treat the core child alone as a complete planning spec review when `review.spec_review_additional_skills` is non-empty, or a complete Apply review when `review.apply_review_additional_skills` has **applicable** entries (applicability-skipped additional paths per [host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md) do not count). Applies to POST-ARTIFACT, staged **Apply review:**, and manual `/tacos-spec-review` / `/tacos-apply-review` parents — not inline host skills on the parent. Otherwise sequential fallback per [post-artifact-planning-review.md](post-artifact-planning-review.md) (**Planning spec review — parallel launch**) / [post-artifact-signoff.md](post-artifact-signoff.md) (**Apply review — parallel launch**).

**Planning spec (example additional child)**

```text
Task({
  description: "spec-review host skill",
  subagent_type: "agent-tacos-additional-spec-review",
  prompt: "Apply host skill <repo-relative-path> to change <name> planning artifacts; cite skill path on each finding."
})
```

**Apply (example additional child)**

```text
Task({
  description: "apply-review host skill",
  subagent_type: "agent-tacos-additional-apply-review",
  prompt: "Apply host skill <repo-relative-path> to change <name> stage <N> diff; cite skill path on each finding."
})
```

**Apply implement (worktree)**

```text
Task({
  description: "apply-implement stage N",
  subagent_type: "agent-tacos-apply-implement",
  prompt: "Implement stage <N> scope for change <name>; task rows: <cited checkboxes>; isolated worktree; return implement summary per subagent-return-contracts.md ## Implement only."
})
```

**Orchestrator fixes (planning or apply)**

```text
Task({
  description: "orchestrator-fixes apply stage N",
  subagent_type: "agent-tacos-orchestrator-fixes",
  prompt: "Remediate from review artifact artifacts/openspec-reviews/<change>/apply-review-<N>.md; scope: stage <N> implementation paths only; return fix summary per subagent-return-contracts.md ## Fix only."
})
```

**Gate-runner (host or scoped profile)**

```text
Task({
  description: "gate-runner stage N",
  subagent_type: "agent-tacos-gate-runner",
  prompt: "Run host implementation gates per AGENTS.md Commands block or scoped profile in task-stage-contract.md ## Scoped gate-runner profile; redirect build/test to artifacts/outputs/; return pass/fail + log path per subagent-return-contracts.md ## Gate only."
})
```

**Dedrift detect**

```text
Task({
  description: "dedrift-detect scoped",
  subagent_type: "agent-tacos-dedrift-detect",
  prompt: "Run detect for capabilities <list>; return structured table per subagent-return-contracts.md ## Detect only."
})
```
