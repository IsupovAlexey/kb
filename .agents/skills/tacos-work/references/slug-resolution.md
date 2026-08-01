# Session slug (from change description)

`<slug>` names `artifacts/tacos-work/<slug>/tasks.md` and `artifacts/openspec-reviews/<slug>/apply-review.md`.

## User input

|Invoke|Meaning|
|-|-|
|`/tacos-work`|Description in same message or next turn|
|`/tacos-work add export for billing admins`|Text after invoke is the **change description** (goes in `tasks.md` **## Intent**)|

## Derivation order

1. **Change description** — From invoke or one ask: _"What change do you want to work on?"_
2. **Derive slug** — Kebab-case from description ([normalization](#normalization-rules))
3. **Confirm when ambiguous** — Collisions or generic slugs (`fix-bug`): show slug + intent line; user may refine once

Do **not** use git branch as primary slug.

## Normalization rules

- Lowercase ASCII; hyphens for spaces
- Drop filler words when distinctive
- `[a-z0-9-]` only; max 64 chars

## Edge cases

|Situation|Action|
|-|-|
|Vague description|Ask once, then derive|
|Existing slug, different **Intent**|Suffix `-2`, `-3`, …|
|User says "use slug X"|Normalized X|

Report slug and `artifacts/tacos-work/<slug>/tasks.md` when creating the session file.

## Archive mode

`/tacos-work archive [<slug>]` — optional slug; distill completed session to `openspec/changes/archive/<date>-<slug>/session.md`.

### Resolution order

1. **Explicit slug** — argument after `archive` (normalized per [normalization rules](#normalization-rules))
2. **Single local session** — exactly one `artifacts/tacos-work/*/tasks.md` exists
3. **Chat context** — unambiguous slug from current tacos-work thread
4. **Ask once** — _"Which session slug should I archive?"_ when still ambiguous

### Preconditions

- Source `artifacts/tacos-work/<slug>/tasks.md` MUST exist locally
- Archive mode MUST NOT run `openspec validate`
