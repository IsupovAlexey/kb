---
name: tacos-split-diff
description: >-
  .NET CLI: unified-diff analyze (JSON change-block index) and reconstruct
  (apply selected hunks to a base file). Use only when tacos-slice-pr
  delegates analyze, reconstruct, verify-tip, or verify-slices.
disable-model-invocation: true
user-invocable: false
---

# tacos split-diff

Callee of [tacos-slice-pr](../tacos-slice-pr/SKILL.md) only — not slash-invocable.

## Quick start

Commands, flags, stdin, and working directory: [references/usage.md](references/usage.md). Run verify subcommands when [tacos-slice-pr](../tacos-slice-pr/SKILL.md) says so — gate order and checks: [verify gates](../tacos-slice-pr/references/verify-gates.md).

## Script

Entrypoint: `scripts/split-diff.cs` (`#:include` `split-diff-*.cs`).

|Command|Purpose|
|-|-|
|`analyze`|Parse diff (stdin or `--diff-file`) → JSON per file|
|`reconstruct`|Apply `--hunks` onto `--base-file`|
|`verify-tip`|Presentation tip tree equals feature; optional slice-plan paths|
|`verify-slices`|Per-slice commits on presentation branch vs slice-plan|

## Done when

|Subcommand|Exit `0` when|
|-|-|
|`analyze`|Valid diff parsed; stdout is JSON with `files[].path`, `status`, `hunks[]`|
|`reconstruct`|Selected hunks applied; `--output` file exists on disk|
|`verify-tip`|Presentation tip tree matches feature branch per supplied refs|
|`verify-slices`|Each slice commit on the presentation branch matches the slice-plan|

Exit `1`: validation or runtime error (message on stderr). Exit `2`: missing subcommand or required flags. Full flag matrix: [references/usage.md](references/usage.md).
