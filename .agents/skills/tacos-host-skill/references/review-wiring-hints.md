# Review wiring hints

For **review-oriented** host skills only. Install and update wire yaml — this skill suggests entries; doctor discovery populates empty arrays on a later doctor run.

## Yaml keys

|Key|Purpose|
|-|-|
|`review.spec_review_additional_skills`|Parallel planning spec review (`agent-tacos-additional-spec-review` per path)|
|`review.apply_review_additional_skills`|Parallel apply review (`agent-tacos-additional-apply-review` per **applicable** path; parent evaluates applicability before spawn)|

Entries are **repo-relative path strings** to a host skill directory or `SKILL.md` — same shape as existing tacos docs.

## Default wiring

Match each array to its review pass — **spec** for planning artifacts; **apply** for implementation diffs.

|Cue|Suggest|
|-|-|
|Conventions, testing, lint, build, commands, code/style, ship-time checklist|`review.apply_review_additional_skills` only|
|Planning, architecture, spec, proposal, design (planning-artifact scope)|`review.spec_review_additional_skills` only|
|Explicit dual scope in description (planning artifacts **and** implementation diffs)|Both arrays|

When uncertain, suggest **apply only**.

Optional host override in generated `SKILL.md` frontmatter: `tacos-review-wiring: apply-only | spec-only | both`.

## Applicability inference (apply review only)

Planning spec review (`review.spec_review_additional_skills`) has **no** applicability shortcircuit — one `agent-tacos-additional-spec-review` per configured path.

For **apply** additional skills, rich `description` frontmatter and path-glob cues help the parent skip spawning `agent-tacos-additional-apply-review` when confident inference shows zero matching diff paths. **Do not duplicate** full rules — read [host-additional-skills.md](../../tacos-apply-review/references/host-additional-skills.md) **Apply review applicability**.

Encode WHEN triggers and path scope in the host skill `description` (path segments, file types, stack names — e.g. `**/*.tsx` under `apps/web/`, `src/Api/**` and `.cs` for backend). No new frontmatter keys — description prose only.

## After authoring

Remind the maintainer:

1. Skill files exist on disk under the detected host skills root.
2. Run `/tacos-doctor install` or update — agent review-skills discovery populates **empty** arrays only; non-empty arrays are preserved.
3. Run `/tacos-doctor diagnose` to confirm wiring OK/WARN lines.

Do **not** hand-edit `openspec/tacos.yaml` arrays in the same session unless the user explicitly requests it — prefer doctor merge for consistency with [review-skills-discovery.md](../../tacos-doctor/references/review-skills-discovery.md).

## Example suggest block (chat output)

```yaml
# Suggested after authoring backend-conventions (doctor run populates when apply array is empty):
review:
  apply_review_additional_skills:
    - <skills-root>/backend-conventions
```

Use the skills root prefix detected in [stack-detection](stack-detection.md) — do not hardcode a host install root in generated paths.
