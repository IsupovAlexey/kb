# Session dismissal catalog

Cross-thread memory for refused automation suggestions within one PR session. When a bot re-posts the same suggestion under a **new** GitHub comment id, the catalog reuses stored disposition and rationale so bypass and loop continuation do not re-prompt or re-triage.

Per-comment records (`comments/<id>.md`) remain authoritative for the **same** id ([persistence-schema](persistence-schema.md) upsert rules). The catalog supplements cross-id reuse only.

## File

`{descriptions_root}/<branch-slug>/pr-triage/_dismissal-catalog.md`

See [output-paths](output-paths.md). Session-scoped — not shared across PRs or branches.

## Frontmatter

```yaml
---
last_updated_at: 2026-07-23T12:00:00Z
entry_count: 1
---
```

Body: one `###` section per catalog entry (below). No duplicate keys — upsert updates the existing section in place.

## Entry shape

Each entry section heading: `### <catalog-key>`

Fenced YAML block:

```yaml
catalog_key: src/foo.cs:42:a1b2c3d4
path: src/foo.cs
line: 42
body_prefix: Remove unused parameter from method signature
source_comment_id: "1234567890"
validity: invalid-or-nit
action: resolve-without-code
fix_state: skipped
status: resolved
notes: Out of scope for this PR — style-only nit
dismissed_at: 2026-07-23T11:30:00Z
```

|Field|Meaning|
|-|-|
|`catalog_key`|Stable match key (see below); equals section heading suffix|
|`path`|Inline path from comment record (required for inline match)|
|`line`|Line number; `null` when unknown (path-only headers in suppressed blocks)|
|`body_prefix`|Normalized prefix used for match (≥ 40 chars when body allows)|
|`source_comment_id`|Comment id that produced this catalog entry (audit only)|
|`validity`, `action`, `fix_state`, `status`, `notes`|Disposition to reuse on match|
|`dismissed_at`|ISO timestamp when entry was first written or last updated|

## Match key

Compute `catalog_key` from the comment record **before** triage:

1. **Normalize path** — forward slashes; trim leading `./`; lowercase not required (case-sensitive path match).
2. **Normalize body** — trim; collapse internal whitespace to single spaces; strip leading/trailing `*` and `_` markdown emphasis from the suggestion text.
3. **Body prefix** — first **40** characters of normalized body when length ≥ 40; otherwise full normalized body. Same rule as [suppressed-review-comments](suppressed-review-comments.md) dedupe.
4. **Key string** — `<path>:<line-or-empty>:<sha256-hex-prefix-8>` where the hash input is `path|line|body_prefix` (pipe-separated; use empty string when `line` is null).

Section heading: `### <catalog_key>` (escape or replace `:` in path only when path itself contains `:` — rare; prefer URL-encoding `:` as `%3A` in the key segment).

**Match rule:** incoming open automation record matches an entry when `path` equals, `line` equals (both null matches path-only), and normalized body shares the same **40-char prefix** (or full body when shorter).

`source: review_suppressed` and thread-backed inline records use the **same** key shape.

## Write triggers (upsert)

After triage or bypass sweep persists a refused automation disposition, upsert a catalog entry when **any** of:

|Trigger|Required fields on comment record|
|-|-|
|Bypass refuse|`author_kind: automation` and `fix_state: skipped` with non-empty `notes`|
|Invalid/nit resolve|`validity: invalid-or-nit` and `action` in (`reply-only`, `resolve-without-code`) and `status` in (`resolved`, `wontfix`)|
|Won't fix|`status: wontfix` with non-empty `notes`|

Do **not** upsert on `defer` alone. Do **not** upsert on applied fixes (`fix_state: applied`) unless the user later re-refuses the same suggestion in-session (then upsert refused disposition).

**Idempotent upsert:** same `catalog_key` → update fields and `dismissed_at` in place; increment `entry_count` only when adding a new key.

## Consult order (mandatory)

After full comment upsert completes ([fetch-and-sync](fetch-and-sync.md)) and **before** advisory triage picklists or bypass sweep triage:

1. Read `_dismissal-catalog.md` when present.
2. For each open automation record per [loop-mode § Open automation baseline](loop-mode.md#open-automation-baseline) where local triage is unset (`validity` null) **or** record was re-opened with cleared triage:
   - If catalog match → copy `validity`, `action`, `fix_state`, `status`, `notes` from the entry; append ` (catalog match)` to triage report line; persist to `comments/<id>.md`.
3. Catalog-matched records with refused disposition (`fix_state: skipped` or `validity: invalid-or-nit`) are **not** open for re-triage picklists — execute persisted disposition in bypass; in normal mode, show in report as pre-triaged.

**Call graph:** sync → **catalog consult** → triage report / bypass sweep → gates.

Applies on every invoke, loop wake (`AGENT_LOOP_WAKE_PR_TRIAGE`), and bot-settle poll after full paginated sync.

## Bypass and loop

- **Bypass sweep** — catalog-matched refused items: skip local edit; run autoresolve with `notes` rationale same as same-id persisted triage ([bypass-mode](bypass-mode.md)).
- **Loop continuation** — fresh agent turn MUST read catalog before treating new automation ids as untriaged; prior chat refusal rationale is **not** required when catalog entry exists.
- **Thread replies** — refusal replies posted to GitHub are **not** ingested on respawn; catalog is the cross-turn memory (out of scope: thread-reply parse).

## Report format

When catalog applies:

```text
- [<id>] validity=invalid-or-nit severity=P2 action=resolve-without-code — <summary> (catalog match; prior: <notes excerpt>)
```

## Forbidden

- Re-triage picklists for catalog-matched refused automation items in bypass mode
- Duplicate catalog sections for the same `catalog_key`
- Cross-session or cross-PR catalog reads
- Dropping catalog on loop counter reset — catalog persists until branch `pr-triage/` folder is removed
