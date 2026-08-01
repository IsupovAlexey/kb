# Apply / apply-review read order

During **apply** and **apply-review**, after `openspec/config.yaml` `tacos-config` context, load host `AGENTS.md` in order:

1. OpenSpec / orchestration managed block (`tacos-agents` markers)
2. Implementation-gates managed block (`tacos-implementation-gates` markers) when present — local dev hard stops and `## Commands`
3. [review-gate-pass.md](../review-gate-pass.md) and [../../../tacos-apply-review/references/pipeline-and-verification.md](../../../tacos-apply-review/references/pipeline-and-verification.md) — Summary gate pass and pipeline/**Verify Decision N** obligations before checking off **Apply review:**

An **empty** implementation-gates body (or `<!-- tacos-doctor-discovery: empty -->`) means no mandatory local gates re-runs for stage completion and no apply-review gates **BLOCKER** solely for missing commands. Non-empty body: orchestrator **MUST** re-run listed commands after apply-review fixes per [task-stage-contract.md](../task-stage-contract.md) **Re-runs checks**; apply-review **BLOCKER** when a listed command failed with command + log path under `artifacts/outputs/`.

Stock OpenSpec CLI steps and host opsx command/prompt files are incomplete for tacos. When `orchestration.enabled` is true, MUST NOT follow stock opsx when conflicting — see [stock-override-binding.md](../stock-override-binding.md).
