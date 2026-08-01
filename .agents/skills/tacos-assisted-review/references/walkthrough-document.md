# Walkthrough document (single delivery)

One complete review per invoke (after diff, optional binding, and [review-delegation](review-delegation.md) when applicable). On Cursor, deliver **markdown companion** (full triads) **+ canvas** (budgeted highlight reel per [canvas-budget](canvas-budget.md)) ([output-delivery](output-delivery.md), [markdown-walkthrough](markdown-walkthrough.md), [canvas-integration](canvas-integration.md)).

## Assisted spirit (not a gate review)

**You guide; the human judges.** Walk them through the diff like a colleague — not an audit, merge checklist, or substitute for `/tacos-apply-review`.

|Do|Do not|
|-|-|
|**Narrative → code → look outs** per important stop ([Triad per stop](#triad-per-stop))|Gate severity tables or author verify checklists|
|Explain **what and why** before each hunk|Diff-first walls with no story|
|**Look outs** after each stop from `*-assisted.md` / delegation (file-scoped)|One blob of gate output at the top|
|**Most important stops first**; trivial paths as refs at the end|Hide files silently|
|**Also in this change** — every path not triad-walked|“Continue in chat” for P0/P1|

## Goal

**Overview** (branch story) → repeated **stops** (narrative, code, look outs) for P0→P1 → optional brief **wiring** → **Also in this change** (file refs for trivial/mechanical/deferred paths).

## Triad per stop

Each walkthrough **stop** (one file or one tight topic) uses this order:

|Step|Content|
|-|-|
|**1. Narrative**|What changed and **why** it exists in this PR (2–4 sentences, guide voice).|
|**2. Code**|`DiffView` (canvas) or post-change fenced block (markdown) for that path.|
|**3. Look outs**|Bullets or a short `Callout` — themes from `apply-review-assisted.md` / `spec-review-assisted.md` **for this path or topic**. Phrase as “worth noticing…” / “might be worth confirming…” — not BLOCKER or “fix before merge.” Omit the look-outs block when delegation has nothing for this stop.|

Then the **next stop** (next P0/P1 path), same triad.

**Canvas pattern:** `Text` (narrative) → `Card` + `DiffView` → `Callout` (look outs).  
**Markdown pattern:** prose → `###` + `file://` + fence → `> **Look outs:**` list.

Do **not** collect all look outs only at document end — they belong **under the code** they refer to. A short **cross-cutting** `Callout` after Overview is optional when one issue spans many files (max one).

## Path inventory and classification

Build a **path inventory** from `git diff --name-only` / `gh pr diff --name-only`. Classify every path:

|Class|Examples|Presentation|
|-|-|-|
|**A — behavior**|`SKILL.md`, `references/**`, core code, behavior specs|Full **triad** stop|
|**B — wiring**|README, `AGENTS.md`, plugins, doctor|One brief triad or one wiring subsection|
|**C — planning**|`openspec/changes/<id>/**`|Triad when review-flagged or drift; else **Also in this change**|
|**D — mechanical**|CRLF-only, generated, lockfiles|**Also in this change** only|

## Priority (stop order)

|Priority|Source|Order|
|-|-|-|
|**P0**|Cited in `*-assisted.md` or delegation themes|First stops|
|**P1**|Other class A|After P0; entry workflow → delegation → delivery → other refs|
|**P2**|Class B|Brief triad or wiring subsection|
|**P3 / P4**|Class C / D|**Also in this change** (path ref + one line)|

**Large diffs:** Markdown — all P0/P1 triads. Canvas — [canvas-budget](canvas-budget.md) (never every full diff inline). Never demote P0 on canvas.

## Top-level sections

|#|Heading|Content|
|-|-|-|
|1|**Overview**|Branch-level what/why (one paragraph).|
|2|**Walkthrough**|P0→P1 **stops** — each [triad](#triad-per-stop).|
|3|**Wiring & integration**|Optional; brief triad or table when class B matters.|
|4|**Also in this change**|Path refs only — trivial, mechanical, deferred (complete inventory). Canvas: [collapsible list](#also-in-this-change-collapsible).|

**No** global reviewer checklist. **No** separate “worth a closer look” section unless one cross-cutting note in Overview.

## File paths (not links in canvas)

|Host|Paths|
|-|-|
|**Canvas**|`<Code>repo/relative/path</Code>` in headers; `DiffView` `path` prop|
|**Markdown**|`file://` links|
|**Chat**|Absolute paths to artifacts and key files|

**Forbidden in canvas:** `file://` / `c:/` markdown links; “full hunks in companion” blobs instead of per-file `DiffView`.

## Also in this change (collapsible)

**Markdown:** `## Also in this change` then `- [path](file://…)` bullets — one line per path + reason.

**Canvas:** `H2 Also in this change` → one **`CollapsibleSection`** (spoiler) titled e.g. `N more files` with `defaultOpen={false}`:

```tsx
<CollapsibleSection title="12 more files" count={12} defaultOpen={false}>
  <Stack gap={4}>
    <Text>
      <Code>…/tacos-assisted-review/references/diff-input.md</Code> — diff
      gather reference
    </Text>
    <Text>
      <Code>openspec/changes/…/proposal.md</Code> — planning prose
    </Text>
    …
  </Stack>
</CollapsibleSection>
```

Use `<Code>` paths — not `file://` links. Group long lists in one collapsible block instead of a comma-separated paragraph.

## Minimum substance

|Requirement|Bar|
|-|-|
|Inventory|100% of paths classified|
|Stops|≥3 P0/P1 triads (narrative + code + look outs when delegation has input)|
|Also in this change|All paths not fully triad-walked|
|Overview|One paragraph what/why|

Incomplete = code without narrative, stops without look outs when `*-assisted.md` cites that path, or missing **Also in this change** entries.

## Overview

**Good:** “This branch adds `/tacos-assisted-review` — a human-invoked walkthrough skill with dual markdown + canvas delivery…”

**Bad:** “Focus on CRLF, spec drift, stage 3…” (put those in **look outs** under the relevant stop).

## Skip

Per-file stat tables, gate tables in Overview, author verify checklists, withholding P0/P1 for a later turn.
