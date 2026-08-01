# Host layout

Host skill **directory creation** is the maintainer's job — this skill bootstraps content after the user confirms the target path.

## Resolve skills root

|Signal|Action|
|-|-|
|Documented skills root in README or `AGENTS.md`|Use that root for proposed paths|
|`tacos-doctor diagnose` skills install roots|Prefer root where `tacos-orchestration` lives|

## Directory layout

After user confirms write:

1. Target: `<skills-root>/<kebab-case-name>/`
2. Create `SKILL.md` at skill root.
3. Create `references/` for templates from [skill-templates](skill-templates.md).
4. Do **not** nest skills under another skill directory.

## Naming

- kebab-case directory matching `name` frontmatter
- Must **not** use `tacos-*` prefix (bundle-owned)
- Avoid colliding with existing host skill directory names

## Confirm before write

Show the full path list (skill root + each reference file). Wait for explicit approval before creating or overwriting files.

## Optional AGENTS.md snippet

MAY output a markdown table row for the maintainer to paste into host `AGENTS.md` — example:

```markdown
| <workflow> | `/host-skill-name` | `<skills-root>/<name>/SKILL.md` |
```

Do **not** write or patch host `AGENTS.md` automatically; doctor owns tacos managed blocks.
