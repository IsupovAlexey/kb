# Diff input

Resolve the review target before binding or rubric work.

## Accepted inputs (equal priority)

|Input|Resolution|
|-|-|
|GitHub PR URL|`gh pr diff` / `gh pr view` on current repo|
|PR number|Same as URL (`42`, `#42`)|
|Git range|`base...HEAD`, `base..HEAD`, named branches|
|Prose compare|"PR to master", "this branch to main" — use checked-out HEAD vs inferred base|
|Branch pair|"from `feat/x` to `develop`" — resolve refs explicitly|
|File paths|User-listed paths → `git diff` or read working tree|
|Unified diff|Pasted patch in message|

Assume the user has the **review branch checked out** when prose refers to "this branch" or "PR to master".

## Resolution order

1. Parse invoke message and `argument-hint` tokens (PR, range, paths).
2. If only a change id appears without diff, ask for PR or diff — change id alone is not a diff.
3. If nothing resolves, **ask once**: PR URL/number, git range, or paths — then stop until answered.
4. When invoke omits a target, ask once — then wait for PR URL, number, range, or paths (no silent inference from branch or history).

## Gathering

|Source|Command / action|
|-|-|
|PR|`gh pr diff <n>` (or host equivalent)|
|Local range|`git diff <base>...HEAD`|
|Paths|`git diff -- <paths>` or read files|

Run git/gh as **separate** shell commands (or PowerShell-safe `;` chains). **Do not** use bash `&&` in PowerShell — it fails with `ParserError`.

Default base for prose compare ("PR to master"): resolve deterministically — (1) `gh repo view --json defaultBranchRef -q .defaultBranchRef.name` when `gh` is available; (2) else `git symbolic-ref --short refs/remotes/origin/HEAD` (strip `origin/`); (3) else if only one of `main` / `master` exists locally (`git rev-parse --verify`), use that ref; (4) else if both exist, **ask once** which base to use — do not guess. Then `git diff <base>...HEAD`.

After gathering, write the unified patch to `artifacts/assisted-review/<slug>/_full.patch` (create slug dir as needed). Record compare range and metadata in `_session.md` per [follow-up-deep-dive](follow-up-deep-dive.md).

Use `_full.patch` for grep and follow-up hunks — do not re-fetch the same diff every turn. For very large diffs, prioritize highest-value hunks in the walkthrough ([walkthrough-document](walkthrough-document.md)); do not paste `git diff --stat` into artifacts.

## Errors

|Situation|Response|
|-|-|
|`gh` missing for PR|Ask for pasted diff or install `gh`|
|Invalid ref|State ref; ask for correction|
|Empty diff|Report no changes; stop|
