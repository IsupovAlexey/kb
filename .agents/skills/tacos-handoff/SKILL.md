---
name: tacos-handoff
description: >-
  Writes a bounded session handoff markdown under artifacts/session-handoff/ for
  continuity in a fresh chat — pointers to OpenSpec change artifacts, chat-only
  essentials, mandatory redaction. Invoke via /tacos-handoff only (not ambient).
disable-model-invocation: true
user-invocable: true
argument-hint: >-
  Optional focus for the next session (e.g. "implement stage 2 only", "debug CI failure")
---

# tacos handoff

Session compact for a fresh chat: explicit `/tacos-handoff` or natural-language request only.

## Quick start

|User says|Action|
|-|-|
|`/tacos-handoff`|Resolve change → collect pointers → session essentials → redaction → write file → report path|
|`/tacos-handoff <focus>`|Same; tailor `Next session` and `Session essentials` to `<focus>`|
|"Compact this session for a new chat"|Same as invoke|

Report one line: `Wrote artifacts/session-handoff/<slug>/<timestamp>-handoff.md`.

## Procedure

1. Resolve change — [change-resolution](references/change-resolution.md); record slug in Meta.
2. Collect pointers — list every on-disk change/review artifact per [dedup-rules](references/dedup-rules.md) Pointers (one path + one-line “read for …” each; omit when dedup says pointer-only or omit).
3. Session essentials — chat-only narrative per [dedup-rules](references/dedup-rules.md).
4. Optional focus — from slash argument (`argument-hint`); when omitted and focus inferred, record assumed focus in Meta only.
5. Redaction — run `dotnet scripts/redact-secrets.cs` on the full draft (stdin or `--file`) per [redaction](references/redaction.md); use script output for write — do not skip for manual scan-only.
6. Write — [output-paths](references/output-paths.md) + [handoff-template](references/handoff-template.md); cap 120 lines / ~4 KB — when over cap, shorten Session essentials and Open threads / risks first, replace trimmed prose with path references; keep Meta, Pointers, and Next session.
7. Report — absolute or repo-relative path to the user.

## Hard rules

- User invoke only (`disable-model-invocation: true`).
- Output path and sections per [output-paths](references/output-paths.md) and [handoff-template](references/handoff-template.md).
- Dedup per [dedup-rules](references/dedup-rules.md).
- One new timestamped file per invocation.

## Done when

- Handoff file exists at `artifacts/session-handoff/<slug>/<timestamp>-handoff.md` with template sections per [handoff-template](references/handoff-template.md) (within cap).
- Draft passed through `scripts/redact-secrets.cs` before write.
- User receives one report line with the written path.
