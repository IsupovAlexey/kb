# Task stage contract (canonical)

Canonical rules for `tasks.md` when `orchestration.staged_apply_enabled` is true are in the tacos schema **`tasks` artifact instruction** (`openspec instructions tasks`). This file is the canonical **staged apply runtime** together with [read-graphs/apply.md](read-graphs/apply.md) MUST read when `orchestration.enabled` is true (schema `apply.instruction` is the gate header only — FIRST ACTION, FORBIDDEN, and pointers).

Hosts MAY add extra requirements in `openspec/config.yaml` `context` or optional `rules.tasks` (e.g. backend-before-frontend ordering) — do not duplicate the schema MUST bullets there.

When `orchestration.staged_apply_enabled` is **false**, terminal Apply review / Human review lines are optional; stock checkbox tracking still applies.

## Stage grill gate (canonical)

Mandatory `Stage grill:` checklist lines and stage-start apply grill applies only when `orchestration.grill_enabled`, `orchestration.staged_apply_enabled`, and `grill.per_task_stage` are all true. Notation rules (short vs qualified keys): [config-notation.md](config-notation.md). When `grill.per_task_stage` is absent from `openspec/tacos.yaml`, treat as **true**. When any of the three is false, omit `Stage grill:` lines and skip mandatory stage-start grill (triggered apply grill still follows `grill.triggers.*` when `orchestration.grill_enabled` is true).

## Stage shape

Use numbered level-2 headings: `## N. Title`.

- Each stage MUST map to `design.md` and specs — one primary outcome per stage, traceable to requirements and design decisions
- Implementation checkboxes on non-trivial stages SHOULD cite `specs/...` ### Requirement and/or design **Decision N** in the task text (traceability only; does not override stock `contextFiles` reads)
- If a stage mixes unrelated outcomes or is too large for one focused review pass, split into additional stages or separate OpenSpec changes
- Before tasks that introduce a new service or major abstraction, include a checkpoint confirming `design.md` already justifies it (or add the design decision first)
- Roughly **eight or fewer** implementation checkboxes per stage (excluding Stage grill, **Verify Decision N**, Test, optional **Project overview:**, Apply review, Human review, and optional host tail lines)
- No orphan test-only stage — tests for behavior belong in the stage that introduces it

## Stage grill line

When the **stage grill gate** is true (tacos-doctor update should add `per_task_stage` explicitly), each `## N.` stage MUST start with this **first** checklist line before any implementation checkboxes:

```markdown
- [ ] Stage grill: **STOP** — mandatory parent `tacos-grill` **apply** before any implementation checkbox below (grill mode per Mandatory stage start in grill-prompts — structured prompt per [structured-gate-convention.md](structured-gate-convention.md) (plain-text only when tools absent)); **this stage only** (`## N` title + unchecked tasks below); **MUST NOT** check implementation tasks until stage grill completes or explicit **skip**/waive; append **User inputs** under `## apply` in `grill-summaries.md`; full-resync planning if scope changes
```

Substitute the stage number and title in the parent interview context; do not merge with Apply review or Human review. When `grill.per_task_stage` is false, omit `Stage grill:` lines. Interview detail: [tacos-grill/references/interview-prompt.md](../../tacos-grill/references/interview-prompt.md) **Mandatory stage apply** — the checkbox line is the apply gate, not a note. Grill mode offer alone does **not** complete `Stage grill:`; parent MUST run topic interview per ## Interview minimum (full / short / defaults) before checkoff (explicit **`skip`** / waive only exception).

Short template (schema `tasks.md`, dogfood `tasks.md`):

```markdown
- [ ] Stage grill: per [task-stage-contract.md](task-stage-contract.md) ## Stage grill line — **this stage only** (`## N` + unchecked below); **User inputs** under `## apply` ### Stage N in `grill-summaries.md`
```

## Apply review line

Each `## N.` stage MUST end implementation work with this **Apply review** checklist line (substitute stage number in the output path):

```markdown
- [ ] Apply review: parallel Task — `tacos-apply-review` + `tacos-additional-apply-review` when applicable (parent merge; **MUST NOT** core-only); write `artifacts/openspec-reviews/<change>/apply-review-<N>.md`
```

When `review.apply_review_additional_skills` is empty, orchestration still uses the same line — parallel launch collapses to one core child. The checkbox text names both skills so apply orchestrators do not treat "Invoke `tacos-apply-review`" as core-only. Runtime detail (inline-only waiver, Task unavailable, re-run checks): [post-artifact-signoff.md](post-artifact-signoff.md) **Apply review — parallel launch and parent merge**.

Review floor only (insufficient for generated `tasks.md` — agents skipped parallel reviewers when using this alone):

```markdown
- [ ] Apply review: parallel Task per [task-stage-contract.md](task-stage-contract.md) ## Apply review line — write `artifacts/openspec-reviews/<change>/apply-review-<N>.md`
```

Generation (`openspec instructions tasks`, schema template) MUST emit the **medium** checklist line in the first fenced block above — not the one-line pointer.

## Project overview line (optional)

When `project_overview.enabled` is true and the stage ships **user-visible surface** (commands, config keys for hosts, public API/docs sections, install/layout, slash commands — not internal refactors or maintainer-only scripts), include an optional checklist line before **Apply review**:

```markdown
- [ ] Project overview: update {project_overview.path} — <sections scoped to this stage's user-visible delta>
```

When no user-visible surface ships in the stage, omit the line. When only the final stage ships user-visible surface, emit the line only there. Completing the line (preview + approval + write per tacos-project-overview) satisfies overview updates for that scope; sync/archive hooks skip when the line is checked per [project-overview-hooks.md](project-overview-hooks.md) **Skip when overview task complete**.

When `orchestration.staged_apply_enabled` is **true**, emit the line only in stages that ship user-visible surface (before **Apply review** in that stage). When **false**, planning MAY place a single optional `Project overview:` line anywhere user-visible work appears in `tasks.md`, or omit it — sync/archive project-overview hooks remain the backstop when the line is absent or unchecked.

## Per-stage checklist order

0. **Stage grill** (when the **stage grill gate** is true) — **STOP** at stage start; parent runs mandatory `tacos-grill` **apply** (grill mode offer — structured prompt per [structured-gate-convention.md](structured-gate-convention.md) (plain-text only when tools absent)) before any implementation checkbox; check off only after stage grill and `grill-summaries.md` **User inputs** (or explicit skip/waiver in chat)
1. Implementation work for the stage outcome
2. **Mocks (optional)** — mock/fixture tasks when the host documents them; otherwise omit
3. **Verify Decision N** (when design decisions require) — `- [ ] Verify Decision N: <done-when>` rows per tacos schema `tasks` instruction **Decision verification**; omit when no material decisions require verification
4. **Tests** — explicit test line(s), or `Tests: N/A` with one-line reason (non-behavioral only)
5. **Project overview (optional)** — when `project_overview.enabled` and this stage ships user-visible surface; separate `- [ ]` line matching [## Project overview line (optional)](#project-overview-line-optional)
6. **Apply review** — separate `- [ ]` line matching [## Apply review line](#apply-review-line) (parallel launch + parent merge; **MUST NOT** core-only when applicable additional paths remain)
7. **Human review** — separate `- [ ]` line starting with `Human review:` or `Human:` requiring pause before the next stage

Do **not** merge (6) and (7) into one “Review” line.

### Verify Decision — separate rows (FORBIDDEN embed)

Implementation rows MAY cite design **Decision N** for traceability. Verification MUST be its own checklist row.

|Forbidden (embed)|Required (separate)|
|-|-|
|`- [ ] 1.1 Add endpoint — **Verify Decision 1**`|`- [ ] 1.1 Add endpoint` then `- [ ] Verify Decision 1: <done-when>`|
|`- [ ] 2.3 Wire publish (Verify Decision 2)`|`- [ ] 2.3 Wire publish` then `- [ ] Verify Decision 2: <named test or trace>`|

### Apply review — FORBIDDEN shortened text

Generated `tasks.md` MUST use the **medium** checklist line under [## Apply review line](#apply-review-line) (first fenced block). **FORBIDDEN:** the one-line pointer (`parallel Task per task-stage-contract.md ## Apply review line` only); `Invoke tacos-apply-review` alone; omitting `tacos-additional-apply-review`, `parent merge`, or `**MUST NOT** core-only`.

## Example

```markdown
## 1. Core scaffolding

- [ ] Stage grill: **STOP** — mandatory parent `tacos-grill` **apply** before any implementation checkbox below (grill mode per Mandatory stage start in grill-prompts — structured prompt per [structured-gate-convention.md](structured-gate-convention.md) (plain-text only when tools absent)); **this stage only** (`## 1` title + unchecked tasks below); **MUST NOT** check implementation tasks until stage grill completes or explicit **skip**/waive; append **User inputs** under `## apply` in `grill-summaries.md`; full-resync planning if scope changes
- [ ] 1.1 Add SKILL.md
- [ ] Verify Decision 1: <!-- example — match design **Decision N**; separate row; done-when names test, trace step, or command -->
- [ ] Tests: N/A — markdown only
- [ ] Apply review: parallel Task — `tacos-apply-review` + `tacos-additional-apply-review` when applicable (parent merge; **MUST NOT** core-only); write `artifacts/openspec-reviews/<change>/apply-review-1.md`
- [ ] Human review: Pause before stage 2
```

## Apply (apply)

When the **stage grill gate** is true:

1. **Stage grill** — at each `## N` stage start, **STOP** and run mandatory parent `tacos-grill` **apply** for **that stage only** before any implementation checkbox — offer grill mode (structured prompt per [structured-gate-convention.md](structured-gate-convention.md); plain-text only when tools absent); **forbidden:** checking off implementation tasks, inferring answers, treating silence as skip, or completing stage grill without user input when structured tools are unavailable. Prompts: [tacos-grill/references/grill-prompts/apply-mandatory.md](../../tacos-grill/references/grill-prompts/apply-mandatory.md); steps: [tacos-grill/references/interview-prompt.md](../../tacos-grill/references/interview-prompt.md) **Mandatory stage apply**. Gates: [grill-gates.md](grill-gates.md) §8. Check off `Stage grill:` only after stage grill or explicit skip/waiver in chat + **User inputs** under `## apply` in `grill-summaries.md`.
2. **Full-resync** — when stage grill changes requirements, scope, or task breakdown: update `grill-summaries.md` ## apply, affected planning artifacts (`proposal.md`, `design.md`, `tasks.md`, …), and change delta specs under `openspec/changes/<name>/specs/` **before** checking off implementation work in that stage.
3. **Triggered apply** — when `orchestration.grill_enabled` is true: **before** checking off each **implementation** checkbox, run the signal checklist in [tacos-grill/references/triggered-grill.md](../../tacos-grill/references/triggered-grill.md). If a signal matches and the yaml trigger is `true`, **STOP** and run tacos-grill for phase `apply` ([grill-prompts/apply-triggered.md](../../tacos-grill/references/grill-prompts/apply-triggered.md)) before continuing that task. Stage grill does **not** satisfy or replace triggered checks — see [grill-gates.md](grill-gates.md) §7–§8.

When the **stage grill gate** is false, skip steps 1–2; step 3 unchanged when `orchestration.grill_enabled` is true.

Per stage: complete **Stage grill** (when enabled) → implementation → Mocks (when host documents them) → **Verify Decision N** (when design decisions require) → **Tests** (or TDD Red/Green/Refactor per `tdd-apply-contract.md` when `tdd.md` is present) → optional **Project overview** (when `project_overview.enabled` and user-visible surface) → **Apply review** → **Human review**.

When implementation through **Tests** (and **Project overview:** when present) completes for the active stage, parent **MUST** continue in the **same turn** — delegate **Apply review** per [post-artifact-signoff.md](post-artifact-signoff.md) **Apply review — parallel launch and parent merge**. **Forbidden:** ending the turn after implementation with **Apply review:** unchecked. Only **Human review:** is the required human pause before the next stage.

Then apply review (parent orchestrator): follow [post-artifact-signoff.md](post-artifact-signoff.md) **Apply review — parallel launch and parent merge** (launch named host subagents per [runtime-delegation.md](runtime-delegation.md); do **not** pass `model` from yaml on Task; parent merge into `artifacts/openspec-reviews/<name>/apply-review-<stage>.md`; sequential fallback when parallel spawn is unavailable). When implement ran in a worktree, parent **MUST** merge before apply review per [## Worktree merge after implement](#worktree-merge-after-implement). Read merged artifact for [review-gate-pass.md](review-gate-pass.md) **before** checking off **Apply review:**. On fail: append **Re-review after fixes** per review-gate-pass **Dynamic re-review checkbox** → **`agent-tacos-orchestrator-fixes`** when Task supported → same-turn STOP → fresh **Re-review after fixes** Task per post-artifact-signoff; **FORBIDDEN** parent-authored r2; repeat until pass or human waiver. On pass: gate-runner when skip does not apply per [## Gate-runner skip](#gate-runner-skip) → when **## Main-spec drift** reports stale spec or code violation, parent offers reconcile/conform/skip per [orchestration-dedrift-pass.md](../../tacos-dedrift/references/orchestration-dedrift-pass.md) § Staged apply → **Human review** (human sign-off before next stage).

### Apply review gate pass

**Pass** when merged `apply-review-<stage>.md` satisfies [review-gate-pass.md](review-gate-pass.md) (`APPROVE` + `Ready`; zero open **BLOCKER**/**CRITICAL**/**MAJOR**).

**Fail** when Summary is not **`APPROVE` + `Ready`**, or blocking **BLOCKER**/**CRITICAL**/**MAJOR** rows remain — **STOP**, do not check off **Apply review:** or open **Human review:**; run remediation loop in that file.

### Re-runs checks (orchestrator, per stage)

After apply review gate pass and before checking off **Human review**, the orchestrator **MUST** re-run local dev checks when the host `AGENTS.md` implementation-gates managed block (`<!-- tacos-implementation-gates-begin -->` … `<!-- tacos-implementation-gates-end -->`) has **non-empty** `## Commands` prose — unless **Gate-runner skip** (below) applies. When checks run and Task spawn is supported, **MUST** delegate to **`agent-tacos-gate-runner`** per [runtime-delegation.md](runtime-delegation.md). Use the commands listed in the host block (format → build → test per gates contract) or the **Scoped gate-runner profile** when only a subset of paths changed. Redirect **build** and **test** output to `artifacts/outputs/` (or the host's documented artifacts path); read saved logs for failures — do not re-run solely to re-display output.

When the implementation-gates block is empty or `<!-- tacos-doctor-discovery: empty -->`, no local gates re-runs are required for stage completion.

#### Gate-runner skip

Skip **`agent-tacos-gate-runner`** (and parent inline gate re-runs) when **all** of the following hold:

- Merged apply-review **Summary** is **`APPROVE` + `Ready`**
- Summary explicitly states **no implementation changes** in the stage (docs-only or procedural stages included)
- No **`agent-tacos-orchestrator-fixes`** ran for the stage
- Mechanical MINOR polish did not run for the stage

Parent **MUST NOT** run `git diff` or equivalent solely to decide gate-runner skip. When skip does not apply and Task is supported, delegate gate-runner; when Task is unavailable, parent runs gates inline and notes inline execution.

#### Scoped gate-runner profile

When the host documents changed-path or scoped gate commands outside the full **Commands** block (e.g. filtered test subsets, single verify script), orchestration MAY document a **scoped profile** here — reference host command strings only; **MUST NOT** edit AGENTS.md **Commands** prose in the tacos maintainer repo.

Example (tacos dogfood — illustrative; hosts document their own scoped commands):

- **Skills/schema touch only:** `npm ci --prefix dev/prettier` + prettier `--check` on `**/*.md`; `openspec validate --all --strict --no-interactive`; `dotnet run dev/schema/verify-host-agents.cs` when host-agent files changed
- **C# touch only:** `dotnet csharpier check .` on changed `*.cs` paths; targeted `dotnet run dev/...` verify scripts matching changed paths

Gate-runner child prompt cites this profile when the stage diff is scoped; otherwise runs the full host **Commands** block.

#### Worktree merge after implement

When **`agent-tacos-apply-implement`** returns in a worktree, parent **MUST** merge the worktree into the session branch **before** apply review. Apply review evaluates the merged tree. On merge conflict, parent resolves or re-spawns implement — no extra human gate before apply review. Detail: [subagent-return-contracts.md](subagent-return-contracts.md) ## Implement.

#### Within-stage parallel slices

When a stage lists **independent** implementation task rows with **disjoint file ownership**, orchestration **SHOULD** pipeline implement → gate-runner → apply review **per slice** within the same stage — subject to [orchestrator-context-budget.md](orchestrator-context-budget.md) (2–4 children per wave). Default **sequential** per stage checklist order when ownership overlaps or parallelism is unclear.

**Cross-stage guard:** MUST NOT start apply review or implementation for stage N+1 while stage N **Human review:** is pending. Within-stage parallel does not relax the single human-review boundary per stage.

#### Parent review merge caps

When parallel core and additional review children return, parent writes one merged artifact and **MUST** cap merged finding bullets per [review-format.md](../../tacos-apply-review/references/review-format.md) ## Finding order and cap (~15 actionable items; overflow in **Deferred**). Orchestration **MUST NOT** require **`agent-tacos-review-merge`** — parent merge with caps is canonical; defer a named review-merge subagent unless parent merge still pollutes after ship.

### Mechanical MINOR sweep (orchestrator, per stage)

When merged apply-review **Status** is **`APPROVE` + `Ready`** but **Optional (MINOR)** lists fixable cohesion or style nits, the orchestrator **MUST** run [../../tacos-work/references/minor-polish-gate.md](../../tacos-work/references/minor-polish-gate.md) — conservative auto-fix, structured multi-select for complicated remainder, **Polish outcome** recording — and re-run checks (above) **before** checking off **Human review:** — unless the human waives specific MINOR rows in chat. Record waived rows in the turn summary. This sweep does not replace **MAJOR** remediation (gate pass still requires zero open **MAJOR**).

Apply-review **BLOCKER** for failed gates applies only when the block is non-empty: cite the **failing command** and a **log path** under `artifacts/outputs/` — see [tacos-apply-review](../../tacos-apply-review/references/checklist-pass2.md) ## Host standards.

Do not check off the `Apply review:` line until the merged artifact exists **and** [review-gate-pass.md](review-gate-pass.md) pass (or inline execution is noted per lightweight-host bypass, or human waiver in chat).

Do not check off Human review before Apply review gate pass and orchestrator fixes finish.

## Parent discovery discipline

When Task is supported during **apply** discovery (locating files, dependencies, call sites, or unfamiliar modules):

- Parent MUST delegate per [proactive-explore-delegation.md](proactive-explore-delegation.md) before serial Grep/Read on implementation paths
- Parent merges child output per [subagent-return-contracts.md](subagent-return-contracts.md) — discovery ≤12 bullets in chat (router: [explore-return-contract.md](explore-return-contract.md))
- Parent FORBIDDEN: paste file contents, full review artifacts, or unbounded tool output in turn summaries
- Parent SHOULD Grep before full Read with a line limit when reading a known path for edit
- **Exempt:** paths explicitly named in the active stage implementation checkbox when the read is for edit, not discovery

Cross-links: [read-graphs/apply.md](read-graphs/apply.md) MUST read; [review-gate-pass.md](review-gate-pass.md) ## Turn-summary delegation record.

## Binding

- **Source of truth for generation:** tacos schema `tasks` instruction (via `openspec instructions tasks`)
- **Staged apply runtime:** this file + [read-graphs/apply.md](read-graphs/apply.md) MUST read when `orchestration.enabled` (schema `apply.instruction` is gate header only)
- **Host extensions:** optional `openspec/config.yaml` `rules.tasks` or `context` only for host-specific additions
