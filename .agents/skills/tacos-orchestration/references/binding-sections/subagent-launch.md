# Named subagent launch (non-normative examples)

**Reference hosts:** Cursor and Claude Code (see [runtime-delegation.md](../runtime-delegation.md) **MUST-delegate matrix**). `*_models` in yaml; **tacos-doctor** syncs installed agent frontmatter — [../../../tacos-doctor/references/config.md](../../../tacos-doctor/references/config.md). The parent **MUST NOT** pass `model` from yaml on Task.

**Cursor**

```text
Task({
  description: "spec-review core",
  subagent_type: "agent-tacos-spec-review",
  prompt: "Run tacos-spec-review for change <name>; scope: planning bundle; do not load review.spec_review_additional_skills — parent merges parallel children."
})
```

**Apply review (staged `Apply review:` line, additional skills configured)** — launch core and each additional child in one turn when supported:

```text
Task({
  description: "apply-review core stage 2",
  subagent_type: "agent-tacos-apply-review",
  prompt: "Run tacos-apply-review for change <name> stage 2; bounded diff <paths>; do not load review.apply_review_additional_skills — parent merges parallel children."
})
Task({
  description: "apply-review host skill",
  subagent_type: "agent-tacos-additional-apply-review",
  prompt: "Apply host skill <repo-relative-path> to change <name> stage <N> diff; cite skill path on each finding."
})
```

**Claude Code** — spawn the same agent `name:`; model comes from the installed agent file for that name.

Hosts without named subagents MAY use slash commands (`/tacos-spec-review`) or inline execution per lightweight-host bypass.

More examples (additional spec/apply children, parallel rules): [runtime-delegation.md](../runtime-delegation.md) **Task launch**.
