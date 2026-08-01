# Mode: update and config

Load when user invokes `/tacos-doctor update`, `/tacos-doctor config`, or equivalent natural-language request.

## Update

Phase A (pre-refresh): [update.md — Phase A](update.md#phase-a--before-skills-refresh). Bootstrap on `check-prereqs` exit `2`/`3` only — not `--sync-host`.

**After skills refresh:** MUST [re-load tacos-doctor from disk](update.md#phase-b--re-load-gate-mandatory) (`SKILL.md`, `references/update.md` Phase B, `references/check-prereqs.md` bootstrap). Session-loaded procedure is stale; do not run post-refresh steps from memory.

Phase B (post-refresh): [update.md — Phase B steps](update.md#phase-b-steps-after-re-load).

Done when: per [update.md — Done when](update.md#done-when).

When update inserted implementation-gates or changed `*_models`, run config in the same session.

## Config

[config.md](config.md) — sync host subagent `model:` from `openspec/tacos.yaml`.

Done when: per [config.md — Done when](config.md#done-when).

Config does not bump bundle `version` or refresh distribution skills — use update for that.
