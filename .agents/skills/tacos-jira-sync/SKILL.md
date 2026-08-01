---
name: tacos-jira-sync
description: >-
  Fetches, regenerates, and pushes jira.md for a bound Jira issue; push requires
  preview approval. Orchestration loads this skill on planning Jira URL and
  regenerate/sync/push intent; also /tacos-jira-sync and natural-language routing.
user-invocable: true
---

# tacos jira-sync

One bound Jira issue per OpenSpec change; MCP-first. Config: `openspec/tacos.yaml` (`jira.*`). Artifact: `openspec/changes/<name>/jira.md`.

## Intent → workflow

|Intent / trigger|Workflow|
|-|-|
|Planning + Jira URL (orchestration)|`resolve-binding` → optional `fetch`|
|Regenerate `jira.md`|`regenerate-jira-md`|
|Regenerate + update Jira|`regenerate-jira-md` → preview → approval → `push-from-jira-md`|
|Push/sync current file only|`push-from-jira-md`|
|Preview title/body (no write)|`compose-description`|
|Fetch / pull remote|`resolve-binding` → `fetch`|
|Create-if-missing (user approved)|`push`|
|`/tacos-jira-sync`|Manual invocation below|

Planning-phase URL detection and NL keywords: [jira-hooks.md](../tacos-orchestration/references/jira-hooks.md). No auto-fetch outside planning unless the user asks.

Sample report line: `Regenerated jira.md for add-<feature> (PROJ-123); preview shown — approve to push.`

Delegation: [runtime-delegation.md](../tacos-orchestration/references/runtime-delegation.md) — regenerate-jira-md delegates when `jira.enabled`; fetch and push stay on the parent. Delta or re-review passes use a fresh subagent.

## Gating

- Fetch/push need `jira.enabled: true`. Regenerate local `jira.md` may run when disabled.
- Warn once per change session if Jira unavailable; never push on artifact save.

## Approval

Intent ≠ approval. Gates: [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#jira-push). Push steps: [push-from-jira-md.md](references/push-from-jira-md.md).

## Workflows

resolve-binding — [binding-heuristic.md](references/binding-heuristic.md).

fetch — Remote → `jira.md`; set `fetched_at`. No re-fetch on later planning unless asked.

regenerate-jira-md — [regenerate-jira-md.md](references/regenerate-jira-md.md).

push-from-jira-md — [push-from-jira-md.md](references/push-from-jira-md.md).

compose-description — Preview title + body from planning artifacts without writing `jira.md`. Use [regenerate-jira-md.md](references/regenerate-jira-md.md) template rules; chat preview only. Prefer `regenerate-jira-md` when the file should be updated.

push — Create or update Jira only after approval ([transport.md](references/transport.md)). Create-if-missing: `jira.default_project_key` and `jira.default_issue_type` from `openspec/tacos.yaml`; user message MAY override project key; ask before create if either required value is missing. On create, record returned URL/key in `proposal.md` and `jira.md` frontmatter.

## Manual invocation (/tacos-jira-sync)

1. Change: name in message → context → `openspec list --json` (one in-progress default; else ask).
2. Intent: regenerate · regenerate+Jira · push only · fetch · compose preview (rare).
3. Report: change, key, regenerated? Jira updated? next step.

URL/key optional if bound; required for fetch when unbound. Ask once if regenerate vs push-only unclear.

## Done when

- Regenerate: `jira.md` updated per [regenerate-jira-md.md](references/regenerate-jira-md.md); chat reports change slug and key when bound.
- Fetch: `jira.md` written with `fetched_at`; no push without separate approval.
- Push / create: user approved preview per [approval-prompt.md](../tacos-orchestration/references/approval-prompt.md#jira-push); Jira write completed or user cancelled after preview (no silent write).
- Compose preview: title + body shown in chat only; no `jira.md` write unless user then asks to regenerate or push.

## References

[binding-heuristic](references/binding-heuristic.md) · [transport](references/transport.md) · [regenerate-jira-md](references/regenerate-jira-md.md) · [push-from-jira-md](references/push-from-jira-md.md) · [approval-prompt](../tacos-orchestration/references/approval-prompt.md#jira-push) · [jira-hooks](../tacos-orchestration/references/jira-hooks.md)
