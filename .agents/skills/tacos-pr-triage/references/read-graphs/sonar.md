# Pr-triage — Sonar (optional)

Load when Sonar check failure detected or `pr.sonar_*` config present — enrichment during checks track only.

## MUST read

- [sonarqube-triage.md](../sonarqube-triage.md)
- [sonarqube-enrichment.md](../sonarqube-enrichment.md)
- [sonarqube-api.md](../sonarqube-api.md)

## When

- After [check-sync.md](../check-sync.md) identifies Sonar-related failures
- Before or during [checks.md](checks.md) assess / matrix steps

## Done when

- Sonar context merged into Check failures section or N/A when no Sonar signal
