# Dedup rules (strict)

Handoff content MUST be **net-new for the next session** — pointer or omit when a fresh agent can load the source from the repo or host context.

## Change artifacts (when slug ≠ `_no-change`)

Pointer + one-line guidance only for durable change artifacts and generated reviews (no section bodies).

|Path pattern|Handoff use|
|-|-|
|`openspec/changes/<slug>/proposal.md`|Pointer + one-line guidance|
|`openspec/changes/<slug>/design.md`|Pointer|
|`openspec/changes/<slug>/tasks.md`|Pointer|
|`openspec/changes/<slug>/specs/**/*.md`|Pointer to `specs/` or per-capability files|
|`openspec/changes/<slug>/grill-summaries.md`|Pointer|
|`openspec/changes/<slug>/jira.md`|Pointer (when file exists)|
|`openspec/changes/<slug>/e2e-scenarios.md`|Pointer (when file exists)|
|`artifacts/openspec-reviews/<slug>/**`|Pointer to folder or specific review files|

## Session essentials (allowed content)

Chat-only content not already in change artifacts or readily available repo docs:

- Debugging steps tried and outcomes
- Explicit user waivers or grill skip decisions not yet in `grill-summaries.md`
- Environment-specific quirks (paths, flags) without secret values
- Immediate next actions the user stated in chat
- Inline tradeoffs not written to `design.md`

## Readily available repo context (all handoffs, including `_no-change`)

Pointer or omit for durable host and repo docs; session essentials only for chat-unique interpretation.

|Typical sources|Default|
|-|-|
|`AGENTS.md`, `README.md`, `openspec/config.yaml`|Pointer only, or omit if obvious for this project|
|`openspec/tacos.yaml`, `openspec/specs/**` (main tree)|Pointer when relevant to **Next session**|
|Installed tacos skill trees (`tacos-*/` under the host skills install root)|Pointer to the skill or reference file when needed|
|Stock OpenSpec / host workflow docs|Omit unless **Next session** needs a path|

**Judgment:** Prefer `read <path>` over repeating text. One short bullet in **Session essentials** when the thread adds a unique interpretation, waiver, or constraint not in those files.

## Pointers section (change resolved)

List every artifact file on disk from the change table above (including `jira.md`, `e2e-scenarios.md`, and `artifacts/openspec-reviews/<slug>/` when present). One bullet per path; one-line “read for …” guidance. Repo-wide docs: pointer or omit per **Readily available repo context**.

## No active change (`_no-change`)

**Session essentials** holds the thread narrative. **Pointers** lists repo paths only when **Next session** needs them. Apply **Readily available repo context** the same as when a change is resolved.
