# Stock grill prompts (router)

Default per-phase prompts for **tacos-grill**. Override via `grill.prompts.<phase>` in `openspec/tacos.yaml`. Omit a key to use the matching bundle below.

|Phase / mode|Bundle|
|-|-|
|planning|[grill-prompts/planning.md](grill-prompts/planning.md)|
|proposal, specs, design, tasks|[grill-prompts/per-phase.md](grill-prompts/per-phase.md) — matching `##` section|
|apply — mandatory stage start|[grill-prompts/apply-mandatory.md](grill-prompts/apply-mandatory.md)|
|apply — triggered|[grill-prompts/apply-triggered.md](grill-prompts/apply-triggered.md)|
|explore|[grill-prompts/explore.md](grill-prompts/explore.md)|
|sync|[grill-prompts/sync.md](grill-prompts/sync.md)|
|update|[grill-prompts/update.md](grill-prompts/update.md)|

`implementation-quality-lenses.md` loads only when planning or apply grill references it.
