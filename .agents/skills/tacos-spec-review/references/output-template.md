# Planning review output

Severity, Summary alignment, and required artifact shape for `tacos-spec-review`.

## Severity guide

|Level|Meaning|Action|
|-|-|-|
|BLOCKER|Cannot implement safely|Must fix before approval|
|CRITICAL|Major issues likely|Should fix before implementation|
|MAJOR|Rework risk|Should fix soon|
|MINOR|Nice to have|During implementation OK|

## Summary gate pass (orchestrator + reviewer)

Orchestrators **MUST** read merged planning review artifacts per [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) before human planning sign-off or apply handoff.

**Reviewer alignment:**

- **MUST NOT** write `Status: NEEDS REVISION` or `Readiness: Not ready` without **BLOCKER** / **CRITICAL** rows in **Must address** for each blocking theme
- **MUST** write `Status: NEEDS REVISION` when any blocking **BLOCKER** or **CRITICAL** remains
- **MUST** write `Status: APPROVE WITH CHANGES` / `Readiness: Ready after fixes` when any open **MAJOR** remains; **APPROVE** / **Ready** only when no open **BLOCKER**, **CRITICAL**, or **MAJOR** in scope
- Blocking planning gaps → formal **Must address** row, not only narrative under **Weak points** or open questions

On fail: **`agent-tacos-orchestrator-fixes`** when Task supported (else parent fixes inline) → **delta re-review** (fresh Task; parallel launch when additional skills configured and concurrent spawn supported) until `APPROVE` + `Ready` on **latest** artifact or human waiver. Parent prose that fixes were applied **does not** satisfy the gate — [review-gate-pass.md](../../tacos-orchestration/references/review-gate-pass.md) ## Anti-short-circuit; protocol: [delta-r2.md](delta-r2.md).

## Required output format

```markdown
# Review: [artifact or planning-bundle] / [change name]

## Complexity & split

_(Required for full planning set, batch, or tasks after full plan exists; omit for single mid-continue file.)_

- Level: Low | Medium | High
- Why: [bullets]
- Recommendation: post-implementation `/tacos-slice-pr` when appropriate (one merge PR;
  verify + post-verify confirm before push) | multiple changes + merge order | Proceed as-is: …

## Intent fidelity

_(Required for full planning bundle and POST-ARTIFACT initial pass; omit for single mid-continue file when scope contract N/A.)_

- Scope fidelity: …
- Adjacent / untraced capabilities: …
- Minimal-default / over-build: …

## Implicit branch coverage

_(Required for full planning bundle and POST-ARTIFACT initial pass.)_

- Open validation / failure branches: …
- Happy-path-only gaps: …
- Unstated assumptions: …

## Summary

- Status: [APPROVE / APPROVE WITH CHANGES / NEEDS REVISION]
- Readiness: [Ready / Ready after fixes / Not ready]
- [1-2 sentences]

## Re-review notes

_(Required when prior review provided.)_

- Resolved: …
- Still open / new: …

## Open questions

- …

## Weak points / risks

- …

## Grill alignment

_(When grill-summaries.md exists and grill ran for scope; else "N/A".)_

- Matched: …
- Missing / contradictions: …

## Cross-artifact notes

_(Planning bundle when specs follow behavioral-only authoring; else "N/A".)_

- Detail in design/tasks: …
- Presentation deferrals: …
- Bundle actionability: …

## Must address

- [BLOCKER/CRITICAL, then MAJOR; cap ~15]
- Traceability: untraced net-new `### Requirement:` → BLOCKER unless waived (see dimensions.md § Traceability)
- Intent fidelity / implicit branch examples: adjacent capability without grill trace (MAJOR); validate without mid-flow failure (MAJOR); untraced requirement (BLOCKER)

## Deferred / lower priority

- …

## Optional: Section ratings

|Section|Rating|Note|
|-|-|-|
|…|Strong / Adequate / Weak / Missing|…|
```
