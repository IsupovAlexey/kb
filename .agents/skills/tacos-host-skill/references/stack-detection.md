# Stack detection

Sniff the host repo before proposing skill names, scope, and reference content. Prefer facts from disk over assumptions.

## Skills roots

1. Read `/tacos-doctor diagnose` output or read `repo-skills.cs` candidates when doctor is available.
2. Prefer the project skills root where `tacos-orchestration/SKILL.md` exists.
3. List existing **non-`tacos-*`** host skills — avoid duplicate names; align naming with neighbors (e.g. `backend-conventions` + `backend-testing`).

## Application stacks

|Signal|Look for|
|-|-|
|.NET|`*.sln`, `*.csproj`, `Directory.Build.props`, `global.json`|
|Node / frontend|`package.json`, `pnpm-workspace.yaml`, `turbo.json`, `apps/*`, `packages/*`|
|Monorepo|workspace fields, multiple package roots, shared `src/` layouts|
|Python / other|`pyproject.toml`, `requirements.txt`, `go.mod` — adapt templates accordingly|

Record detected roots (e.g. `src/`, `apps/api/`, `frontend/`) for applicability cues in generated `description` frontmatter.

## Test and quality tooling

Scan for frameworks and runners to inform testing-skill templates:

- .NET: xUnit, NUnit, MSTest in csproj or test projects
- JavaScript: Jest, Vitest, Playwright, Cypress in `package.json`
- Repo scripts: `Makefile`, `justfile`, CI workflow test steps

## Human docs

Read before inventing conventions:

1. README, CONTRIBUTING
2. `AGENTS.md` outside `<!-- tacos-agents-* -->` and `<!-- tacos-implementation-gates-* -->`
3. Activated `openspec/host/*.md` (not `*.md.template`)

Use declared paths or naming conventions from these docs when they name host skills or review expectations.

## Review gap heuristics (suggest only)

When stacks are detected but matching host skills are missing, note gaps in chat — do not invent yaml paths:

|Detected stack|Typical missing skills|
|-|-|
|.NET backend|`backend-conventions`, `backend-testing`|
|React / frontend app|`frontend-conventions`, `frontend-testing`|
|Docs-only host|policy or prose review skills under detected skills root|

Point maintainers to `/tacos-doctor install` or update after authoring review-oriented skills.
