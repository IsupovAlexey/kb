# SonarQube Web API

Read-only GET helpers for tacos-pr-triage Sonar enrichment. Token stays in the environment — never in yaml, persistence, chat echoes, or commits.

## Prerequisites

|Input|Source|
|-|-|
|Base URL|`pr.sonar_host` in `openspec/tacos.yaml` (no trailing slash)|
|Project key|`pr.sonar_project`|
|Pull request id|PR number from triage session (`_session.md` `pr_number`)|
|Token|`SONAR_TOKEN` environment variable|

Enablement gate and when to call the API: [sonarqube-enrichment](sonarqube-enrichment.md).

## Authentication

SonarQube accepts HTTP basic auth with the token as the username and an **empty** password.

**MUST NOT** print, persist, or commit `SONAR_TOKEN`. Use env expansion in shell examples only; agents MUST redact token values in chat output.

## HTTP transport (OS-generic)

Use the first option that works on the host shell.

### 1. curl (preferred when available)

**Unix / macOS / Linux / Git Bash / WSL:** `curl` on `PATH`.

**Windows:** use **`curl.exe`** — PowerShell aliases `curl` to `Invoke-WebRequest`. Probe with:

```powershell
Get-Command curl.exe -ErrorAction SilentlyContinue
```

When `curl` / `curl.exe` is available, use silent fail (`-sf` / `-f`) and basic auth:

```bash
# PR_NUMBER from _session.md pr_number; SONAR_* from openspec/tacos.yaml
export PR_NUMBER=42

# Unix-style (bash, sh, Git Bash, WSL) — project status
curl -sf -u "${SONAR_TOKEN}:" \
  "${SONAR_HOST}/api/qualitygates/project_status?projectKey=${SONAR_PROJECT}&pullRequest=${PR_NUMBER}"

# issues/search (same auth; ps=50 cap)
curl -sf -u "${SONAR_TOKEN}:" \
  "${SONAR_HOST}/api/issues/search?componentKeys=${SONAR_PROJECT}&pullRequest=${PR_NUMBER}&resolved=false&ps=50"
```

```powershell
# PR number from _session.md pr_number; SONAR_* from openspec/tacos.yaml (export or $env:)
$prNumber = 42

# Windows when curl.exe is available — project status
curl.exe -sf -u "${env:SONAR_TOKEN}:" `
  "$($env:SONAR_HOST)/api/qualitygates/project_status?projectKey=$($env:SONAR_PROJECT)&pullRequest=$prNumber"

# issues/search
curl.exe -sf -u "${env:SONAR_TOKEN}:" `
  "$($env:SONAR_HOST)/api/issues/search?componentKeys=$($env:SONAR_PROJECT)&pullRequest=$prNumber&resolved=false&ps=50"
```

Set `SONAR_HOST` and `SONAR_PROJECT` from yaml; set PR number from `_session.md` `pr_number` before calling (do not hardcode).

### 2. PowerShell (Windows fallback)

When `curl.exe` is not available, use **`pwsh`** or **`powershell`** with `Invoke-RestMethod`:

```powershell
$token = $env:SONAR_TOKEN
$sonarHost = $env:SONAR_HOST.TrimEnd('/')
$project = $env:SONAR_PROJECT
$prNumber = 42  # from _session.md pr_number
$pr = $prNumber

$pair = "${token}:"
$bytes = [System.Text.Encoding]::ASCII.GetBytes($pair)
$basic = [Convert]::ToBase64String($bytes)
$headers = @{ Authorization = "Basic $basic" }

$uri = "$sonarHost/api/qualitygates/project_status?projectKey=$([uri]::EscapeDataString($project))&pullRequest=$pr"
Invoke-RestMethod -Uri $uri -Headers $headers -Method Get

# issues/search — same $headers; swap path and query:
$issuesUri = "$sonarHost/api/issues/search?componentKeys=$([uri]::EscapeDataString($project))&pullRequest=$pr&resolved=false&ps=50"
Invoke-RestMethod -Uri $issuesUri -Headers $headers -Method Get
```

Use `[uri]::EscapeDataString()` for query parameter values when building URLs in PowerShell. Do not use `$host` — it is a PowerShell automatic variable.

On **401**, **403**, **5xx**, or transport errors: note in triage report, set `sonar_enrichment_failed` on the check record, fall back to log-only assessment — do not abort triage ([sonarqube-enrichment](sonarqube-enrichment.md)).

## Endpoints

Base path: `{pr.sonar_host}/api/`

### Quality gate — project status

```
GET /qualitygates/project_status?projectKey={projectKey}&pullRequest={pullRequest}
```

**Response fields (use in report / persistence):**

|Field|Use|
|-|-|
|`projectStatus.status`|`OK` or `ERROR` → `sonar_gate_status`|
|`projectStatus.conditions[]`|Gate conditions table (metric, operator, threshold, actual, status)|

### Issues — search (unresolved, capped)

```
GET /issues/search?componentKeys={projectKey}&pullRequest={pullRequest}&resolved=false&ps=50
```

|Query param|Value|
|-|-|
|`componentKeys`|`pr.sonar_project`|
|`pullRequest`|PR number|
|`resolved`|`false`|
|`ps`|`50` (hard cap)|

**Response fields (use in report / persistence):**

|Field|Use|
|-|-|
|`issues[]`|Capped issue list|
|`issues[].severity`|BLOCKER, CRITICAL, MAJOR, MINOR, INFO|
|`issues[].rule`|Rule key|
|`issues[].component`|File/component key|
|`issues[].line`|Line when present|
|`issues[].message`|Short message|

Do not dump full JSON into the triage report — summarize gate conditions and issues per [sonarqube-enrichment](sonarqube-enrichment.md).
