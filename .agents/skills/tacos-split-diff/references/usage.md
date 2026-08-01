# split-diff usage

Working directory: run `git` from the host repo root; run `dotnet run scripts/split-diff.cs` from the `tacos-split-diff` skill directory. Callers at host repo root (e.g. `tacos-slice-pr`) use `../../tacos-split-diff/scripts/split-diff.cs` — slice-plan paths: that skill's references (`pr.descriptions_root` in host `openspec/tacos.yaml`).

## analyze

Stdin: omit `--diff-file` and pipe a unified diff (single file or whole feature diff). The CLI reads until EOF.

```bash
git -C <host-repo-root> diff <trunk-ref>...HEAD -- path/to/file \
  | dotnet run scripts/split-diff.cs analyze

git -C <host-repo-root> diff <trunk-ref>...HEAD -- path/to/file > /tmp/file.diff
dotnet run scripts/split-diff.cs analyze --diff-file /tmp/file.diff

git -C <host-repo-root> diff <trunk-ref>...HEAD > /tmp/feature.diff
dotnet run scripts/split-diff.cs analyze --diff-file /tmp/feature.diff
```

From host repo root (caller path):

```bash
git diff <trunk-ref>...HEAD -- path/to/file \
  | dotnet run ../../tacos-split-diff/scripts/split-diff.cs analyze
```

Emits JSON: `files[].path`, `status`, `hunks[]` with `index` (0-based per file) and `header`. Each `hunks` entry is one contiguous change block in that file's diff.

## reconstruct

```bash
git -C <host-repo-root> show <trunk-ref>:src/foo.ts > /tmp/foo.base
git -C <host-repo-root> diff <trunk-ref>...HEAD -- src/foo.ts > /tmp/foo.diff

dotnet run scripts/split-diff.cs reconstruct \
  --base-file /tmp/foo.base \
  --diff-file /tmp/foo.diff \
  --hunks 0,2 \
  --output /tmp/foo.slice1
```

Cumulative slice commits: pass all block indices for the slice in one call (e.g. `--hunks 0,1,2`) with `--base-file` from merge base. Do not chain `reconstruct` on a prior slice output.

Added files (`status: A`): use an empty base when `git show <base>:path` is missing.

Overlapping blocks: error if selected blocks overlap in the base file.

## verify-tip

Run when `tacos-slice-pr` verify gates require it — see [tacos-slice-pr](../../tacos-slice-pr/SKILL.md).

```bash
dotnet run ../../tacos-split-diff/scripts/split-diff.cs verify-tip \
  --feature <feature-branch> \
  --presentation <presentation-branch> \
  --merge-base <trunk-ref> \
  --slice-plan <slice-plan-path>
```

## verify-slices

Run when `tacos-slice-pr` verify gates require it — see [tacos-slice-pr](../../tacos-slice-pr/SKILL.md).

```bash
dotnet run ../../tacos-split-diff/scripts/split-diff.cs verify-slices \
  --slice-plan <slice-plan-path> \
  --presentation <presentation-branch> \
  --trunk-ref <trunk-ref>
```

## Exit codes

- `0` success
- `1` validation error
- `2` usage error
