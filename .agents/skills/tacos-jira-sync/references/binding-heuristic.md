# Binding heuristic

Classify Jira URLs/keys in the user message as **bound** (one per change, written to `jira.md`) or **context-only** (ephemeral context, never synced).

## Scoring (per candidate)

|Signal|Weight|
|-|-|
|URL in change-creation / `propose` / `ff` / `continue` / `apply` message|+3|
|User primary language near URL (“this ticket”, “primary”, “bound”, “for this change”)|+3|
|Issue type Story or Task (from fetch or URL context)|+2|
|Project matches `jira.default_project_key`|+1|
|First candidate in proposal draft|+1|

## Decision

- Highest score wins when margin over second place is **≥2**.
- If top two differ by **≤1**, ask the user which issue is bound before writing `jira.md`.
- User explicitly marks a link as related/context-only → never bind; MAY fetch for chat context only.

## Replacement

If `jira.md` already has `issue_key` and a new candidate would bind differently, confirm replacement before overwrite.
