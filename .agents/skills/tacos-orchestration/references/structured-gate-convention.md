# Structured gate convention (all tacos skills)

Every user-approval, mode selection, binding confirm, grill interview, or preview gate in tacos skills follows this contract. Detail per flow stays in each skill; turn discipline is shared.

## Universal rule

|Structured tools in session tool list|Required behavior|
|-|-|
|**Present** (`AskQuestion`, `AskUserQuestion`, or host equivalent)|Opening a gate or showing a preview → the **next parent tool call MUST** be the structured prompt tool — **forbidden:** prose-only menus (`Proceed / Edit / Cancel?`, numbered options in chat without the tool).|
|**Absent** (e.g. Cursor Cloud agents)|Plain-text fallback per [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable — **end turn**; wait for next user message. **Do not skip** the gate.|

Canonical wording: [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt preferred and ## Structured prompt unavailable.

**Preview + gate in one turn:** Show preview in chat, then call the structured tool in the **same** response — preview text is not a substitute for the tool call.

## Gate catalog

|Gate|Skill / reference|Option ids (typical)|
|-|-|-|
|Grill mode (planning grill — propose, ff, tacos-work Phase 1)|[tacos-grill/interview-prompt.md](../../tacos-grill/references/interview-prompt.md)|`full`, `short`, `defaults`, `assumptions`, `skip`|
|Grill mode (per-phase continue)|[tacos-grill/interview-prompt.md](../../tacos-grill/references/interview-prompt.md)|`full`, `short`, `defaults`, `skip`|
|Grill mode (stage apply)|[tacos-grill/interview-prompt.md](../../tacos-grill/references/interview-prompt.md)|`full`, `short`, `defaults`, `skip`|
|Grill script topics|[tacos-grill/interview-prompt.md](../../tacos-grill/references/interview-prompt.md)|per script|
|Triggered grill (apply, sync, explore)|[tacos-grill/triggered-grill.md](../../tacos-grill/references/triggered-grill.md)|per question|
|Change / phase disambiguation|[tacos-grill/interview-prompt.md](../../tacos-grill/references/interview-prompt.md)|per change id / phase|
|POST-ARTIFACT intent playback (planning sign-off)|[post-artifact-signoff.md](post-artifact-signoff.md) ### Human planning sign-off|`proceed`, `edit_planning`, `cancel`|
|tacos-work execute confirm|[session-runbook.md](../../tacos-work/references/session-runbook.md) Phase 2.5|`proceed`, `edit`, `cancel`|
|tacos-work planning grill|[planning-grill.md](../../tacos-work/references/planning-grill.md)|grill mode + topics|
|tacos-ask small-edit confirm|[small-edits.md](../../tacos-ask/references/small-edits.md)|`proceed`, `edit`, `cancel`|
|Dedrift mode (reconcile / conform)|[tacos-dedrift/modes.md](../../tacos-dedrift/references/modes.md)|`reconcile`, `conform`|
|Dedrift capability scope|[tacos-dedrift/SKILL.md](../../tacos-dedrift/SKILL.md)|capability ids or `all`|
|Dedrift preview (spec/code writes)|[tacos-dedrift/preview-gate.md](../../tacos-dedrift/references/preview-gate.md)|`proceed`, `edit`, `cancel`|
|Apply-review MINOR polish (tacos-work Phase 4; staged apply)|[tacos-work/minor-polish-gate.md](../../tacos-work/references/minor-polish-gate.md)|one option per open MINOR row; `allow_multiple: true`; unselected = waived|
|Test plan preview (write pack)|[tacos-test-plans/references/preview-gate.md](../../tacos-test-plans/references/preview-gate.md)|`proceed`, `edit`, `cancel`|
|Test plan replace (existing slug)|[tacos-test-plans/references/preview-gate.md](../../tacos-test-plans/references/preview-gate.md)|`proceed`, `edit`, `cancel`|
|Test plan scope grill mode|[tacos-test-plans/references/scope-grill.md](../../tacos-test-plans/references/scope-grill.md)|`full`, `short`, `defaults`, `skip`|
|Orchestration dedrift choice|[orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md)|`reconcile`, `conform`, `skip`|
|GitHub PR create / sync|[approval-prompt.md](approval-prompt.md#github-pr-create-and-sync)|`approve`, `edit`, `cancel`|
|Jira push|[approval-prompt.md](approval-prompt.md#jira-push)|`approve`, `edit`, `cancel`|
|Jira binding confirm|[approval-prompt.md](approval-prompt.md#jira-binding-confirmation)|per tie/replace options|
|Project overview (opt-in, scope plan, write)|[sync-archive-prompts.md](../../tacos-project-overview/references/sync-archive-prompts.md)|`yes`/`no`, `approve_plan`/`refine`/`cancel`, `approve`/`edit`/`cancel`|
|PR triage routing, fix, resolve, publish|[approval-gates.md](../../tacos-pr-triage/references/approval-gates.md)|per gate table|
|PR triage stuck CI (loop)|[loop-mode.md](../../tacos-pr-triage/references/loop-mode.md)|`continue-wait`, `stop-loop`|
|Slice plan approval|[plan-and-approve.md](../../tacos-slice-pr/references/plan-and-approve.md)|`approve`, `revise`, `cancel`|
|Slice branch execution|[plan-and-approve.md](../../tacos-slice-pr/references/plan-and-approve.md)|`approve`, `defer`, `cancel`|
|Slice post-verify confirm|[post-verify-confirm.md](../../tacos-slice-pr/references/post-verify-confirm.md)|`approve`, `defer`, `cancel`|
|Assisted-review change binding|[change-binding.md](../../tacos-assisted-review/references/change-binding.md)|`bind`, `adhoc`, `skip` or per change id|
|Context delegation disambiguation|[runtime-delegation.md](runtime-delegation.md)|per prompt|

**Not gated (no AskQuestion):** scheduled dedrift ([scheduled-job-prompt.md](../../tacos-dedrift/references/scheduled-job-prompt.md)), PR triage bypass sweep, loop watch/poll, ambient dedrift with unambiguous behavioral direction.

## Adding a new gate

1. Add a row to the catalog above.
2. Cross-link from [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured gates.
3. In the skill reference: preview semantics + option id table + **forbidden:** prose-only when structured tools are present.
