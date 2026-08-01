# Optional lens preflight (explicit invoke)

Advisory capability triage before semantic dedrift detect on explicit `/tacos-dedrift` invoke. Preflight does not replace detect, preview, or writes — see [modes](modes.md).

## When to use

- Scope is broad (`reconcile all`, many implicated capabilities) and you want a cheap narrow list first
- Apply-review or orchestration surfaced possible main-spec drift and you want structural signals before semantic compare
- User asked to check whether specs look stale relative to related code paths

Skip when lens is unavailable, the scope is already one capability, or the user chose mode and scope without asking for preflight.

## Call order (explicit invoke only)

1. Optional — load [../../tacos-lens/SKILL.md](../../tacos-lens/SKILL.md); run Spec Validation in the viewer or:

```bash
dotnet run --project <tacos-lens-install>/src/TacosLens.Cli -- check --repo <repo-path>
```

2. Read Spec staleness info findings and sync-readiness summary when present
3. Choose mode and capability scope per [modes](modes.md) (unchanged)
4. Semantic detect per [../SKILL.md](../SKILL.md) ## Explicit invoke procedure

Orchestration surfaces (sync, archive, verify, staged apply, tacos-work, tacos-ask) keep the shared contract in [orchestration-dedrift-pass](orchestration-dedrift-pass.md) — semantic detect there is unchanged; do not insert a lens gate on those paths.

## What lens provides

|Signal|Source|Use for dedrift|
|-|-|-|
|OpenSpec schema errors|`openspec validate` via lens `check` or Spec Validation|Fix validate failures before behavioral drift work|
|Planning consistency|Lens deep checks (delta↔main, cross-artifact, apply-review gates)|Change-folder hygiene; not a substitute for main-spec behavioral compare|
|Spec staleness (info)|Git-age compare when host map exists|Capability triage only — related paths touched after the spec|

Staleness findings mean a related path changed after the spec was last touched — not that a specific SHALL/MUST obligation is wrong. Semantic detect still runs.

## Host staleness map (optional)

Repos may ship `openspec/host/spec-staleness-map.yaml` (or `staleness-map.yaml`). Lens reads capability id → glob list entries and emits info-only Spec staleness findings when matched files have newer git touches than the main spec.

Example shape:

```yaml
capabilities:
  post-artifact-orchestration:
    - openspec/tacos.yaml
    - "**/tacos-orchestration/**"
  tacos-dedrift:
    - "**/tacos-dedrift/**"
```

Omit the file when no map is needed — lens skips staleness checks without it.

## Staleness hint → dedrift journey

1. Run optional lens Spec Validation or `check --repo <path>` on the tacos-powered repo
2. Note Spec staleness info findings (and optional host map hits) for capability `<cap>`
3. Invoke `/tacos-dedrift reconcile <cap>` (or `conform <cap>`) — user chooses mode; staleness alone MUST NOT auto-trigger reconcile or conform
4. Complete detect → preview → structured gate → write per [preview-gate](preview-gate.md)

## Guardrails

- Advisory only — preflight findings do not skip semantic detect on explicit invoke
- Optional — proceed with dedrift when lens is missing or `check` fails for environment reasons
- No auto-reconcile — staleness or validate warnings MUST NOT invoke reconcile/conform without user choice
- Behavioral compare stays in detect — lens does not read code semantics for SHALL/MUST alignment
