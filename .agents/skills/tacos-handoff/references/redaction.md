# Redaction (mandatory before write)

Run on the full handoff draft (all sections) immediately before writing the file under `artifacts/session-handoff/`. The agent MUST pipe draft content through `../scripts/redact-secrets.cs` (or `dotnet scripts/redact-secrets.cs --file <draft>` from the skill root) and write the script stdout — not the pre-redaction draft.

## Secrets-shaped patterns

Replace matches with `[REDACTED]`:

|Pattern|Examples|
|-|-|
|API keys|`sk-…`, `AKIA…`, long alphanumeric key assignments|
|Bearer / tokens|`Bearer eyJ…`, `ghp_…`, `github_pat_…`|
|Passwords|`password=…`, `pwd: …` in connection strings|
|Connection strings|`mongodb://user:pass@…`, `Server=…;Password=…`|

## Procedure

1. Scan **Session essentials** and **Open threads / risks** first (highest risk).
2. Scan **Meta** and **Next session** for pasted secrets.
3. Replace each match with `[REDACTED]` (whole secret token per match).
4. If secrets were pasted, add one bullet under **Open threads / risks**: user may need to rotate exposed credentials.
