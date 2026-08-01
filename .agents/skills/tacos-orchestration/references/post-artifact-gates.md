# POST-ARTIFACT gates (router)

When `orchestration.enabled` is true and planning artifacts reach apply-ready, **do not** end the turn at “artifacts written” unless the user asks to stop.

**Load first:** [post-artifact-index.md](post-artifact-index.md) — toggle matrix, step order, done/not done, MUST-delegate one-liners.

**Step bundles (on demand):**

- [post-artifact-planning-review.md](post-artifact-planning-review.md) — E2E, planning spec review, orchestrator fixes, delta re-review, split nudge, validate
- [post-artifact-signoff.md](post-artifact-signoff.md) — human planning sign-off, apply-turn apply review parallel launch

Stock OpenSpec CLI and host opsx steps are incomplete for tacos — [stock-override-binding.md](stock-override-binding.md). Hub: [orchestration-binding.md](orchestration-binding.md) § POST-ARTIFACT.
