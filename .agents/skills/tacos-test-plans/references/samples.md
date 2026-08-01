# Non-normative samples

Examples for skill authors and QA orientation during `/tacos-test-plans` runs. **Not** part of the runtime contract — hosts are not required to adopt these paths.

Authority for format: [test-case-format.md](test-case-format.md) and [templates/](templates/).

## Host artifact locations (illustrative)

|What|Example path pattern|
|-|-|
|Test-case pack (tacos default)|`openspec/test-plans/<slug>/<slug>-test-cases.md`|
|QA report mirror (optional)|`.qe/report/<slug>-test-cases.md` or `artifacts/test-plans/<slug>/test-cases.md`|
|Domain-specific format skill (optional host)|`<host-skill-root>/references/test-case-format.md`|

### TC AREA patterns (illustrative)

|Feature domain|AREA prefix|Example id|
|-|-|-|
|Scheduling|`SCHED`|`TC-SCHED-001`|
|Company profile settings|`CP`|`TC-CP-001`|
|Business units|`BU`|`TC-BU-001`|
|Technicians|`TECH`|`TC-TECH-001`|
|Job types|`JT`|`TC-JT-001`|
|Zones|`ZN`|`TC-ZN-001`|

tacos default: derive AREA from test-plan slug primary token (see [test-case-format.md](test-case-format.md)). Hosts MAY use domain tables like the above.

## tacos distribution templates

|What|Path|
|-|-|
|Canonical bundle (maintainer)|`../../tacos-doctor/schemas/tacos/templates/test-plan-pack/test-cases.md`|
|Gate-synced skill mirror|`templates/test-cases.md`|
|Host-editable schema copy|`openspec/schemas/tacos/templates/test-plan-pack/test-cases.md`|

After `tacos-doctor` schema sync, hosts edit templates under `openspec/schemas/tacos/templates/test-plan-pack/`.
