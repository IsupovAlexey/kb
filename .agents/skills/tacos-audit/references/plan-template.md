# Audit plan template

Level 2 hybrid plan — evidence + executor hardening + tacos-work-compatible **## Work** tail.

Default: `artifacts/tacos-audit/<slug>/audit-plan.md`. Use `plans/NNN-<slug>.md` when 3+ unrelated findings selected.

````markdown
---
audit_slug: <slug>
finding_ids: [AUD-001]
planned_at_sha: <git rev-parse --short HEAD at plan write>
handoff: execute
category: tech-debt
effort: M
confidence: HIGH
---

# Audit plan: <title>

## Why

<impact from findings table>

## Evidence

- `path:line` — <observation>
- Rejected false positives: see index.md (if any)

## Current state

<advisor-verified excerpt — not from child report alone>

## Drift check (run before execute)

```bash
git diff --stat <planned_at_sha>..HEAD -- <paths>
```

## Commands (discovered during recon; verify at execute)

|Gate|Command|
|-|-|
|format|`<from recon>`|
|test|`<from recon>`|

## Scope

**In:** <paths>
**Out:** <explicit exclusions>

## STOP conditions

- If <X> → stop and report to user
- If verify command fails for unrelated reason → stop

## Work

**Testable outcome:** <one sentence>

- [ ] <implementation step 1>
- [ ] <implementation step 2>
- [ ] Verify: <command + expected>
- [ ] Apply review: parallel Task — `agent-tacos-apply-review` + `agent-tacos-additional-apply-review` when applicable (parent merge); write `artifacts/openspec-reviews/<slug>/apply-review.md`
- [ ] Human review: sign-off before marking audit index DONE
````

## Handoff-specific notes

|`handoff`|**## Work** shape|
|-|-|
|`execute`|Full checklist above|
|`work`|Short **Intent** block + pointer — tacos-work fills **## Work**|
|`propose`|Scope summary + suggested change name — propose owns artifacts|
|`dedrift`|Single row: `/tacos-dedrift reconcile <cap>` after preview|
