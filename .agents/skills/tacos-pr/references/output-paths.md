# PR description output paths

Generated output under repository-root `{descriptions_root}/` (from `pr.descriptions_root`, gitignored), not `openspec/changes/`.

```text
{descriptions_root}/<change>/pr-descriptions/<change>.md
```

Single-PR output omits numeric prefixes (post-hoc slice skill uses numbered files).

|Situation|File|
|-|-|
|Regenerate / open|`<change>.md` (create if missing)|
|Legacy|[regenerate-descriptions.md](regenerate-descriptions.md)|
|Sync|Matching `pr_number` or unambiguous `head_branch`; skip `slice_index`|
|Ambiguous|Ask|

Other folder files: out of scope for regenerate; sync one scoped file with `pr_number`, not slice note.

```yaml
pr:
  descriptions_root: artifacts/openspec-artifacts
```

Do not commit unless the host project opts in. Overwrite only this change’s single-PR description file; no tombstones.
