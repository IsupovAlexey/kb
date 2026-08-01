# SonarQube triage

Vendored detection and routing for Sonar-sourced check failures. **No external skill dependency.**

## Detection

Treat a check as Sonar-sourced when **any** of:

- Check **name** contains `sonar` (case-insensitive)
- Check **name** matches common GitHub App titles: `SonarCloud`, `SonarQube`, `SonarCloud Code Analysis`, `SonarQube Code Analysis`

Detection runs during check assessment before generic [check-triage-rubric](check-triage-rubric.md) classification.

## Routing

When a failing check matches detection:

1. Evaluate the API enablement gate in [sonarqube-enrichment](sonarqube-enrichment.md).
2. **Gate passes** → fetch gate status and issues per [sonarqube-api](sonarqube-api.md); persist and report per enrichment doc.
3. **Gate fails** → skip API calls, note skip reason in **Check failures**, classify using log fallback rubric in [sonarqube-enrichment](sonarqube-enrichment.md).
4. **API auth or transport failure** → note failure, fall back to log rubric for that check only.

Resolve host instructions per [sonarqube-enrichment](sonarqube-enrichment.md) § Host instructions overlay.

## Picklist

Sonar **code** failures follow the same picklist rules as other checks: **root or standalone** only ([check-matrix](check-matrix.md)). Multiple Sonar jobs in one workflow may collapse to one root. At most **one** Sonar row on the check fix picklist per matrix root/standalone — not per issue.

## Extending detection

Add new App title strings to the **Detection** list in this file when encountered — do not bloat `SKILL.md` prose.
