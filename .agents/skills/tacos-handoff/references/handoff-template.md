# Handoff document template

Use these level-2 headings in order. Cap: **120 lines** and **~4 KB**. When over cap, shorten **Session essentials** and **Open threads / risks** first; keep **Meta**, **Pointers**, and **Next session** within the cap.

```markdown
## Meta

- **Change:** <slug or _no-change>
- **Written:** <ISO 8601 local timestamp matching filename>
- **Focus:** <argument text, or inferred focus, or "general continuity">

## Pointers

- `openspec/changes/<slug>/proposal.md` — <one line: read for why/scope>
- `openspec/changes/<slug>/design.md` — <one line>
- `openspec/changes/<slug>/tasks.md` — <one line>
- `openspec/changes/<slug>/specs/` — <one line>
- … (every present artifact per dedup-rules)

## Session essentials

<Chat-only facts per dedup-rules: debugging narrative, waivers, env quirks, thread-only decisions. Bullets only.>

## Open threads / risks

<Unresolved questions, blockers, or risks for the next session.>

## Next session

1. <3–7 imperative steps for the next agent/human session>
2. …
```

**Sections:** Meta, Pointers, Session essentials, Open threads / risks, Next session only. Record inferred focus in **Meta** when the user omits a focus argument.
