# Approval prompt (shared)

Explicit user approval before **external writes** (GitHub, Jira), **project overview** file updates, **dedrift** spec/code writes, and other gated tacos flows. **Intent ≠ approval.**

**All gates:** [structured-gate-convention.md](structured-gate-convention.md) — when structured tools are in the session tool list, the **next parent tool call** after preview or gate open MUST be `AskQuestion` / `AskUserQuestion`; **forbidden:** prose-only approval menus.

|Runtime|Tool|
|-|-|
|Cursor|`AskQuestion`|
|Claude Code|`AskUserQuestion`|
|Other / unavailable|Plain text — **end turn**; wait for next user message — [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable|

When structured tools **are** available, follow [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt preferred: after preview, the **next tool call MUST** be `AskQuestion` / `AskUserQuestion` — not a prose approval menu in chat. When unavailable, use plain-text fallback — **do not** skip the gate.

Call structured prompts **after** the preview is shown in the same response. Structured selection **is** explicit approval when preview preceded it (same turn OK unless noted).

## Plain-text fallback (all flows)

When structured tools are unavailable, follow [interview-prompt.md](../../tacos-grill/references/interview-prompt.md) ## Structured prompt unavailable for turn discipline and numbered options. Then:

1. Show preview.
2. Ask once for explicit approve / edit / cancel.
3. **End turn** — no write.
4. Next user message: only unambiguous yes (`approve`, `yes push`, `yes sync`, …) → proceed.

## Never counts as approval (all flows)

- Wording in the **starting** message (open/sync/push/regenerate urgency).
- Silence, implied consent, regenerate/preview alone.
- Ambiguous `ok` → re-ask once (structured if possible), then stop.

Flow-specific “never” items are listed under each section below.

## GitHub PR create and sync

After preview, ask before `gh pr create` or `gh pr edit`.

**Create:** Preview MUST show the exact `--title` string (host rules per `tacos-pr` or `tacos-slice-pr` `pr-title-conventions`). Then ask:

`Create draft PR "{title}" from {head} into {base}?` (omit “draft” when not draft.)

|id|label|
|-|-|
|`approve`|Create PR → [gh-pr-create-single.md](../../tacos-pr/references/gh-pr-create-single.md)|
|`edit` / `cancel`|Stop; no GitHub write|

**Sync one:** `Update GitHub PR #{pr_number} ({head}) with this body?` — same id table; `approve` → `gh pr edit`.

**Sync many (same `pr_number`):** `Sync {count} file(s) to PR #{pr_number}?` — on approve, [push-to-github.md](../../tacos-pr/references/push-to-github.md) for scoped file (one edit).

**Never (PR):** Starting open/sync/regen wording; regenerate-only; missing preview before edit.

**Missing `pr_number`:** MAY `gh pr list --head <branch>` to backfill. No PR → offer `/tacos-pr open` or manual frontmatter.

## Jira push

After key + summary + full description preview, ask before any Jira write.

**Prompt:** `Push this description to {issue_key}?`

|id|label|Meaning|
|-|-|-|
|`approve`|Approve push to {issue_key}|Explicit yes → `push` per [push-from-jira-md.md](../../tacos-jira-sync/references/push-from-jira-md.md)|
|`edit`|Edit jira.md first|Stop; no Jira write|
|`cancel`|Cancel|Stop; no Jira write|

Substitute `{issue_key}` (e.g. `PROJ-123`). Create-if-missing: `Create {issue_type} in {project} and push?` with `approve` / `cancel` only.

**Never (Jira push):** `jira.md` footer; regenerate alone without push approval.

## Jira binding confirmation

When `resolve-binding` needs user input (tie, replace bound key), use structured prompt per [structured-gate-convention.md](structured-gate-convention.md) (**next tool call MUST** be the tool when available); plain-text fallback only when tools absent. Binding confirm is **separate** from push approval.

## Project overview

After previewing scoped edits to `project_overview.path`, ask before writing. Sync/archive: run [sync-archive-prompts.md](../../tacos-project-overview/references/sync-archive-prompts.md) Gates 1–2 first.

**Prompt:** `Apply these overview updates to {path}?`

|id|label|
|-|-|
|`approve`|Write → [update-workflow.md](../../tacos-project-overview/references/update-workflow.md) step Write|
|`edit` / `cancel`|Stop|

Structured **approve** after preview MAY write in the same turn.

## Dedrift preview

After combined drift report and proposed spec/code edits on explicit `/tacos-dedrift`, ask before any write.

**Prompt:** `Apply these dedrift changes to main specs (and code if conform)?`

|id|label|
|-|-|
|`proceed`|Proceed → [preview-gate.md](../../tacos-dedrift/references/preview-gate.md) Write|
|`edit` / `cancel`|Stop; no spec or code write|

When structured tools are available, the **next tool call after preview MUST** be `AskQuestion` / `AskUserQuestion` — **forbidden:** prose-only `Proceed / Edit / Cancel?`

## Orchestration dedrift choice

When main-spec drift is detected on verify/sync/archive/tacos-work/tacos-ask/staged apply, ask before invoking `/tacos-dedrift`.

**Prompt:** `Main-spec drift detected for {capabilities}. How should we proceed?`

|id|label|
|-|-|
|`reconcile`|Reconcile — `/tacos-dedrift reconcile` (preview)|
|`conform`|Conform — `/tacos-dedrift conform` (preview)|
|`skip`|Skip — continue workflow without dedrift|

Same structured-tool rule as above; detail: [orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md).
