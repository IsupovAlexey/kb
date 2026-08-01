# Archive script (`archive-session.cs`)

Deterministic tacos-work session archiver. Working directory: **tacos-work skill root** (directory containing `scripts/archive-session.cs`).

## Commands

```bash
dotnet run scripts/archive-session.cs --repo <repo-root> --slug <slug> --preview [--format json] [--date YYYY-MM-DD]
dotnet run scripts/archive-session.cs --repo <repo-root> --slug <slug> --write [--format json] [--date YYYY-MM-DD]
```

- `--preview` — emit `session.md` to stdout (or JSON with `targetPath` + `sessionMarkdown`)
- `--write` — write `openspec/changes/archive/<date>-<slug>/session.md` only
- Missing `artifacts/tacos-work/<slug>/tasks.md` → exit 1, no write

## `/tacos-work archive` skill mode

1. Resolve slug
2. Run `--preview` (prefer `--format json` for gate display)
3. Structured Approve / Decline gate with target path + markdown
4. On approve: run `--write`
5. Agent MUST NOT paraphrase `tasks.md` — script output is authoritative

## Lens

Work sessions **Archive** shells out to the same script when `scripts/archive-session.cs` exists in the installed tacos-work skill directory.

## Verify

From tacos repo root:

```bash
dotnet run dev/archive-session/verify-fixture.cs
```

Distill rules: [archive-session-template.md](archive-session-template.md) § Deterministic distill.
