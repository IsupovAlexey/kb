# SonarQube API enrichment

Optional read-only SonarQube Web API layer on top of [sonarqube-triage](sonarqube-triage.md) detection. When inactive or failing, triage uses log and comment parsing only — same advisory posture as today.

## Enablement gate

Call the Sonar Web API **only** when **all** are true:

1. `pr.sonar_enabled: true` in `openspec/tacos.yaml`
2. `pr.sonar_host` non-empty (URL, no trailing slash)
3. `pr.sonar_project` non-empty
4. `SONAR_TOKEN` set in the environment (non-empty)
5. Failing check matches Sonar detection in [sonarqube-triage](sonarqube-triage.md)

Otherwise **no Sonar API calls**. When a Sonar-sourced check fails but any prerequisite fails, the **Check failures** section MUST include a skip reason, for example:

- `Sonar API skipped: pr.sonar_enabled is false`
- `Sonar API skipped: pr.sonar_host not configured`
- `Sonar API skipped: pr.sonar_project not configured`
- `Sonar API skipped: SONAR_TOKEN not set`

Set `sonar_enrichment_skipped` on the check record ([persistence-schema](persistence-schema.md)).

## Fetch procedure

When the gate passes for a failing Sonar check:

1. Read `pr.sonar_host`, `pr.sonar_project`, and PR number from `_session.md`.
2. Fetch quality gate status, then issues search — see [sonarqube-api](sonarqube-api.md) for endpoints and **OS-generic** HTTP transport (`curl` / `curl.exe`, else PowerShell `Invoke-RestMethod`).
3. Parse JSON; extract gate status, conditions, and up to 50 issues.
4. Update check record frontmatter: set `sonar_enriched: true`, `sonar_fetched_at` (ISO timestamp of fetch), and `sonar_gate_status` (`OK` or `ERROR` from gate response); clear `sonar_enrichment_skipped` and `sonar_enrichment_failed`. Append `## Sonar enrichment` body section with gate conditions and capped issue list ([persistence-schema](persistence-schema.md)).
5. Include structured gate + issue summary in the **Check failures** report for that check.
6. Classify `failure_kind` from enumerated issues when present; otherwise follow log fallback rubric below.

On **401** or **403**: note auth failure in report, set `sonar_enrichment_failed`, do **not** persist token, fall back to log-only assessment for that check.

On **5xx**, timeout, or malformed response: note transport failure, set `sonar_enrichment_failed`, fall back to log-only assessment.

Enrichment failure for one check does **not** abort triage for other checks or comments.

## Host instructions overlay

Load optional repo-specific prose when assessing a Sonar-sourced check:

|Priority|Path|
|-|-|
|1|`pr.sonar_host_instructions_path` when set (repo-relative)|
|2|`openspec/host/sonarqube.md` when path unset and file exists|
|3|None — generic references only|

Host file is freeform markdown (bot patterns, coverage exclusions, noisy paths, post-fix commands). Include a short **Host Sonar notes** excerpt in **Check failures** when the file exists. Do not parse structured keys from the file.

Scaffold: tacos-doctor `templates/openspec/host/sonarqube.md.template` → rename to `sonarqube.md` to activate.

## Report shape (Check failures section)

Per enriched Sonar check, include:

1. One-line summary (gate status + issue count)
2. Skip or failure note when API not called or degraded
3. Quality gate conditions table (metric, status, threshold vs actual when present)
4. Capped issue list: severity, rule, component, line, message (max 50)
5. **Host Sonar notes** excerpt when host file resolved
6. Advisory `failure_kind`, severity hint, and action — one picklist row for root/standalone only ([check-matrix](check-matrix.md))

API enrichment informs assessment only. **One** Sonar root or standalone row on the check fix picklist — not per issue. Bypass mode autofixes eligible **code** check failures (`failure_kind: code`, `matrix_role` ∈ {`root`, `standalone`}, `action: fix-in-code`) per `bypass-mode.md`; infra/unknown/dependents/needs-context without confirmation stay report-only.

## Log fallback rubric

When enrichment is skipped or fails, classify from bounded log tail and check output using these hints:

|Signal in log|Interpretation|Typical action|
|-|-|-|
|Quality Gate **failed**|Gate conditions not met|`failure_kind: code` when issues listed; else `unknown`|
|**New** issues / new code period|Regressions on changed lines|`fix-in-code`, severity P0–P1 by issue type|
|**Security** hotspot / vulnerability|Security finding|`fix-in-code`, severity P0|
|**Coverage** below threshold|Coverage gate|`fix-in-code` or `defer` when test-only|
|**Duplication**, **maintainability** rating|Non-blocking quality|severity P1–P2|
|Sonar **server unreachable** / scanner install failed|Infrastructure|`failure_kind: infra`, `wait-retry`|

When the log lists file paths and rule keys, cite them in the one-line summary.

### failure_kind defaults (log path)

- Quality gate failed **with enumerated issues** in log → `code`
- Scanner OOM, server 5xx, auth to Sonar failed in log → `infra`
- Gate failed without issue list in available excerpt → `failure_kind: unknown` with `validity: needs-context` when `log_truncated`

When API returns issues, prefer API enumeration over log parsing for `failure_kind` and severity.

### Severity mapping (advisory)

|Sonar category|Severity hint|
|-|-|
|Blocker / Critical on new code|P0|
|Major on new code|P1|
|Minor / Info|P2|

## Persistence

See [persistence-schema](persistence-schema.md) — Sonar enrichment frontmatter and `## Sonar enrichment` body appendix on `checks/<slug>.md`.

On re-sync: refresh GitHub-owned fields and log body; preserve enrichment frontmatter and appendix for a still-failing Sonar check (re-fetch API in the same triage pass when appropriate). When the check turns green on re-sync, clear Sonar enrichment fields and remove the `## Sonar enrichment` appendix per [persistence-schema](persistence-schema.md).
