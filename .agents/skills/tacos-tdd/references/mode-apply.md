# Mode: apply (orchestration-loaded)

Load when `tdd.md` exists in the change folder — orchestration Entry per [tdd-apply-contract.md](../../tacos-orchestration/references/tdd-apply-contract.md).

## Per-stage order

After Stage grill when enabled: per-slice Red → Green → Refactor before Apply review — [red-green-refactor.md](red-green-refactor.md). Task row shapes: [task-slice-template.md](task-slice-template.md).

Docs-only slices may use `Tests: N/A` with a one-line reason instead of Red/Green/Refactor.

## Hard gate

MUST NOT check Green until Red is done and failing output is visible in chat. Log files under `artifacts/outputs/` may supplement; they do not replace chat paste. User waiver → record per [red-green-refactor.md](red-green-refactor.md) **Waiver recording** before Green.

## Committed QA test plans (read-only)

When `tdd.md`, `proposal.md`, or `design.md` links `openspec/test-plans/<slug>/`:

- MAY read `<slug>-test-cases.md`, optional `journeys.md`, and sibling pack files as authoritative Red input
- MAY map `TC-*` rows to focused Red tests alongside Jira, Figma, and change specs
- MUST NOT create, edit, delete, or "fix" files under `openspec/test-plans/` during propose, apply, or POST-ARTIFACT — test plan mutation is QA-only via `/tacos-test-plans`
- When implementation cannot satisfy a committed `TC-*` row, **STOP** the slice — report the mismatch in chat; fix production code or escalate QA to re-invoke `/tacos-test-plans` on a QA branch with replace approval — do not rewrite scenario markdown to match code

Red authoring detail: [red-green-refactor.md](red-green-refactor.md) **Red from committed scenarios**.

## Done when

Each behavioral slice shows failing-test output in chat before Green; Green and refactor assessment recorded per [red-green-refactor.md](red-green-refactor.md); user waiver recorded before Green when applicable.
