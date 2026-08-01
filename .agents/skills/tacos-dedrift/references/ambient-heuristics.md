# Ambient main-spec maintenance

Applies when an agent changes implementation **outside** explicit `/tacos-dedrift` as the direct result of a user prompt. Same **behavioral-only** layering as apply-review and `grill.triggers.apply_on_spec_drift`.

## Clear direction → update spec + notify

When behavioral impact on main specs is **unambiguous**:

1. Update affected `openspec/specs/<capability>/spec.md` in the same session.
2. Tell the user in chat which spec files changed and what behavioral obligation was added or revised.
3. Do **not** open the full explicit preview gate — ambient path is notify-after-write for clear cases only.

### Clear examples (apply-review precedents)

|Situation|Ambient action|
|-|-|
|User adds a **new user-visible behavior** (e.g. export endpoint, new CLI flag with documented semantics) and specs lack the obligation|Add or extend the governing SHALL/MUST in the matching capability spec; notify in chat|
|User **explicitly requests removal** of a user-visible behavior and reconcile direction is confirmed|Update spec to remove or revise the SHALL/MUST; notify in chat|
|New **authorization or error path** with one obvious capability owner|Update that capability spec's scenario or requirement|

### Not clear — presentation / inventory (no ambient spec write)

|Situation|Ambient action|
|-|-|
|**Figma color match** — design defers presentation, tasks verify, specs require visibility without hex values (apply-review "Color from Figma not in spec")|Do **not** add color tokens to main specs; do **not** treat as drift requiring spec update|
|**Procedure-only** change — paths, pipeline order, templates in design/tasks or skill references; specs state governing behavioral obligations only|Do **not** expand specs with runbook detail|
|**Seed data / inventory** in design or tasks (e.g. "wire three seed specialists") while specs state the behavioral rule only|Do **not** enumerate instances in main specs|

These match apply-review layered compliance: omission of presentation/procedure from specs is intentional when specs state the governing behavioral rule.

## Ambiguous direction → offer `/tacos-dedrift`

Do **not** silent-write main specs when:

- Reconcile vs conform is unclear (code wrong vs spec stale vs both).
- Multiple capabilities could own the obligation.
- Change could be read as presentation-only or behavioral.
- User intent contradicts existing spec SHALL/MUST without explicit approval.

**Action:** State ambiguity in chat; offer `/tacos-dedrift` with mode choice via structured prompt per [structured-gate-convention.md](../../tacos-orchestration/references/structured-gate-convention.md).

### Ambiguous examples

|Situation|Why ambiguous|
|-|-|
|Implementation **hides or removes** content specs require visible without explicit user intent (apply-review "Behavioral violation flagged")|Reconcile spec down vs conform code to restore visibility — direction unclear|
|Refactor moves behavior between modules — spec still describes old boundary|Reconcile spec structure vs conform code to old spec|
|Feature flag defaults changed — unclear if spec documents default or only capability|May be config detail vs behavioral obligation|
|Partial implementation — code ships subset of spec scenarios|Conform remainder vs reconcile spec down|

## Checklist (agent self-test)

Before ambient spec write, confirm **all**:

- [ ] Change affects **behavioral** obligation, not presentation/inventory/procedure deferred to design/tasks
- [ ] Exactly one capability (or obvious split) owns the obligation
- [ ] Reconcile direction is clear — specs should reflect shipped behavior
- [ ] User was not asking for a dedicated dedrift audit (use explicit invoke instead)

If any fail → offer `/tacos-dedrift`.
