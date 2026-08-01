# Jira transport (MCP-first)

Prefer **Atlassian MCP** (`plugin-atlassian-atlassian`). Fall back to **Atlassian CLI** on PATH when MCP is unavailable. Use non-interactive flags only.

## Resolve cloud

|Transport|Operation|
|-|-|
|MCP|`getAccessibleAtlassianResources` — use returned `cloudId`|
|CLI|`acli jira workspace list` or site URL from issue link|

## Read issue (fetch)

|Transport|Operation|
|-|-|
|MCP|`getJiraIssue` — `cloudId`, `issueIdOrKey`, `fields` as needed, `responseContentFormat: markdown`|
|CLI|`acli jira issue view <KEY> --json` (or equivalent); map summary, description, status, issuetype|

Request fields needed for `jira.fetch_fields` plus summary and description.

## Create issue

|Transport|Operation|
|-|-|
|MCP|`createJiraIssue` — `projectKey`, `issueTypeName`, `summary`, `description`, `contentFormat: markdown`|
|CLI|`acli jira issue create --project ... --type ... --summary ... --description ...`|

## Update issue

|Transport|Operation|
|-|-|
|MCP|`editJiraIssue` — `fields: { summary, description }`, `contentFormat: markdown`|
|CLI|`acli jira issue edit <KEY> ...`|

## Write scope

Only **summary** (title) and **description** on write. Read may include configured `jira.fetch_fields` (default `status`, `issue_type`).

## Failure

If both MCP and CLI fail, treat Jira as unavailable for that operation; follow warn-once rules in `SKILL.md`. Do not guess issue content.
