# Jira orchestration hooks

Skill: `../../tacos-jira-sync/SKILL.md`.

## Planning-phase URL detection

During `propose`, `ff`, `continue`, `update`, or `apply` while creating/updating **planning** artifacts (`proposal`, `specs`, `design`, `tasks`):

0. When `jira.md` already exists for the change, **read it first** and treat it as authoritative ticket context for artifact writes on this turn — per [planning-artifact-loop.md](planning-artifact-loop.md) ### `jira.md` as planning context.
1. Scan the **current user message** for Jira URLs and issue keys.
2. If `jira.enabled: true` and transport available → `resolve-binding` → `fetch` → write `jira.md`; put canonical URL/key in `proposal.md` only (no full body).
3. If `jira.enabled: false` → warn once per change session; skip fetch; link in `proposal.md` allowed.
4. If `jira.md` exists and user did not request refresh → use snapshot; no fetch.

Do **not** auto-fetch on casual chat outside these commands.

## Natural-language routing

Resolve active change (default one in-progress; ask if many). Then tacos-jira-sync:

|Intent|Keywords (examples)|Workflow|
|-|-|-|
|Regenerate `jira.md`|`regenerate`, `rebuild jira.md`, `refresh jira from artifacts`|`regenerate-jira-md` (excludes prior `jira.md` body); when `jira.enabled`, delegate via **`tacos-jira-regenerate`** per [runtime-delegation.md](runtime-delegation.md)|
|Regenerate + Jira|above + `sync` / `push to jira`|`regenerate-jira-md` + preview → [approval-prompt.md](approval-prompt.md#jira-push) → `push-from-jira-md`|
|Push file only|`sync to jira`, `push to jira` (no regenerate)|Preview → approval prompt → `push-from-jira-md`|
|Pull from Jira|`fetch`, `pull`, link + from jira|`fetch`|

Refuse out-of-scope fields (assignee, custom fields, transitions).

## Manual invocation (`/tacos-jira-sync`)

User-invocable skill (same workflows). Requires Jira URL/key and sync direction (from / to Jira) in the message when not already bound. Resolve active change: default when only one in-progress change; **ask user** when ambiguous. See tacos-jira-sync `SKILL.md` **Manual invocation**.

## Approval (non-bypassable)

**MUST** follow [approval-prompt.md](approval-prompt.md#jira-push) — preview then structured prompt or follow-up yes; intent ≠ approval; orchestration cannot skip the gate.
