# Artifact prose quality

Planning artifacts (proposal, specs, design, tasks) SHALL use direct factual prose. Avoid LLM filler that adds no information.

Applies **additively** alongside four-layer distinctness, Goals-first design, and positive requirement framing — does not replace them.

## Chat vs planning

Session **chat** replies when orchestration is enabled follow [direct-output.md](../../tacos-direct-output/references/direct-output.md) (action-first, gate carve-outs). **This file** governs durable planning artifacts and skill reference bodies written during propose, ff, continue, and tacos-work planning — lead with the fact or obligation, not the next user action.

## Slop patterns

Canonical **pattern ids** for spec-review findings (use exactly):

|Pattern id|Pattern|
|-|-|
|`evaluative-lead`|Evaluative predicate leads|
|`imagined-contrast`|Imagined contrast|
|`false-agency`|False agency|
|`marketing-label`|Marketing quality labels|
|`nominalization`|Nominalization|
|`drama`|Drama|
|`sentence-stack`|Dense multi-idea sentences|

### 1. Evaluative predicate leads

Do not open with a vague quality claim before the fact.

|Avoid|Prefer|
|-|-|
|The retry control is explicit. The job table stores retry count…|The job table stores retry count, last failure reason, and next eligible run time.|
|The ownership model is clear. Each topic has a platform owner…|Each topic has a platform owner, data owner, and on-call rotation.|

**Test:** Delete the first sentence. If nothing is lost, delete it.

### 2. Imagined contrast

Do not argue against a claim nobody made.

|Avoid|Prefer|
|-|-|
|The field is not optional; it is required.|The field is required.|
|This is not just validation; it is a comprehensive trust layer.|Validation checks X before Y; failures reject with Z.|

**Exception:** design **Decisions** MAY contrast **real** alternatives considered (X over Y) with concrete trade-offs — not throat-clearing negation.

### 3. False agency

Systems and documents do not want, know, decide, or understand.

|Avoid|Prefer|
|-|-|
|The scheduler wants to keep the queue empty.|The scheduler drains the queue when depth exceeds N.|
|The dashboard understands which incidents are risky.|The dashboard ranks incidents by severity and age.|

### 4. Marketing quality labels

Drop empty adjectives: robust, scalable, resilient, flexible, extensible, enterprise-grade, production-ready, secure by design, seamless, powerful, best-in-class.

|Avoid|Prefer|
|-|-|
|The ingestion layer provides a robust and scalable foundation.|The ingestion layer accepts events on topic T with at-least-once delivery.|

### 5. Nominalization

Prefer verbs over noun piles.

|Avoid|Prefer|
|-|-|
|Validation of the payload is performed prior to confirmation of message reception.|Validate the payload before acknowledging the message.|

### 6. Drama

Drop intensifiers without concrete risk: significant, crucial, critical, highly, deeply, tremendous, unavoidable, dangerous (unless citing a specific failure mode).

|Avoid|Prefer|
|-|-|
|Missing this ownership model creates a dangerous operational blind spot.|Without an owner per topic, on-call routing has no default assignee.|

## Sentence discipline

STE-inspired clarity for planning prose — complements the slop patterns above; does not replace SHALL/MUST scenario structure.

- One main idea per sentence in requirement bodies, design **Goals**, and **Decisions** (except enumerated lists and WHEN/THEN blocks).
- Prefer active voice and verbs over noun piles (see `nominalization`); **tasks** use imperatives with paths and done-when.
- Use literal verbs — avoid idioms (`circle back`, `leverage`, `land`) when a concrete action fits.
- **Exception:** design **Decisions** MAY use multi-sentence trade-off prose when contrasting real alternatives (X over Y) with concrete consequences.

Canonical pattern id for spec-review: `sentence-stack` — sentence stacks multiple unrelated obligations, qualifiers, or topics without a list structure.

|Avoid|Prefer|
|-|-|
|The service validates input, persists the record, emits an event, and returns the id in one breath when each step is a separate obligation.|Split into bullets or separate sentences; one obligation per sentence in spec bodies.|
|The handler is responsible for the validation of requests prior to their acceptance by the system.|The handler validates each request before acceptance.|

## Scope naming

Canonical pattern id for spec-review: `scope-version-shorthand`.

Do not suffix APIs, endpoints, skill references, host overlays, or delivery scope with `v1`, `v2`, or similar unless naming a **real external version** the product actually uses (e.g. a vendor path segment).

|Avoid|Prefer|
|-|-|
|API v1 surface: gate + issues only|API surface: `qualitygates/project_status` and `issues/search` only (~50 issue cap)|
|SonarQube Web API (v1)|SonarQube Web API|
|Endpoints (v1)|Endpoints|
|Conventional files (v1)|Conventional files|
|no yaml keys in v1|no new yaml keys in this change — deferred: orchestration hooks|

State **what is in or out of this change** in **Boundaries**, deferred **Alternatives**, or factual lists — not internal version labels that read like product or API versions.

**Exception:** Quote an external system's real version when documenting their surface (e.g. vendor `/api/v2/` in a path). Do not invent tacos scope labels as `v1`.

Skill references and `SKILL.md` MUST follow the same rule — planning jargon MUST NOT leak into shipped reference titles or section headings. Deferral and example prose in skill references follow the same host-generic rule: no named external repos, real identifiers, or import-from phrasing.

## Density check

Before finalizing each planning artifact, confirm the written file is at least as information-dense as the grill, discovery, or upstream material used to fill it. Do not compress tables, WHEN/THEN scenario blocks, decision lists, stage groupings, or enumerated checklists into thinner prose that drops facts.

## Session context fidelity

After writing a planning artifact and before dispatching spec-review or advancing to the next artifact:

1. Re-read the written file in full.
2. Scan the session for agreed substance: grill **User inputs**, resolved ambiguities, edge-case decisions, scenario discussions, and prior-phase chat that applies to this artifact.
3. For each requirement, constraint, clarification, or behavioral detail agreed for this artifact: verify it appears with original specificity intact.
4. Discarded alternatives from Q&A are intentionally absent — do not flag their omission.

**Pass:** all agreed context captured → proceed to review or next artifact.

**Fail:** list each missing item with its session source → fix the artifact inline → re-verify before review.

Complements [density check](#density-check): density guards grill→file compression; session fidelity guards chat→file drops.

## Discoverable facts

Before asking the human a planning question, check whether the answer already exists in `proposal.md`, delta or main specs, `design.md`, `tasks.md`, `grill-summaries.md`, or `artifacts/tacos-work/<slug>/tasks.md` **Planning**. When found, cite the source and proceed — do not re-prompt.

## Anti-inventory (specs)

Specs state durable behavioral rules — not per-instance enumerations or presentation tokens owned by external sources.

|Avoid|Prefer|
|-|-|
|List every specialist name, hex color, or grouping row|Render all items matching a documented rule|
|Figma-level layout detail in SHALL/MUST bodies|Defer presentation detail to design **External sources** or host conventions|
|One `### Requirement:` per grill bullet or file path|Merge related obligations into fewer broad requirements|

## Layer notes

|Layer|Focus|
|-|-|
|proposal|Outcome bullets — no quality-label padding on Why or What Changes|
|specs|Normative SHALL/MUST is fine; requirement bodies stay factual, not marketing|
|design|Goals and Decisions state facts and trade-offs; cite specs by Requirement title; Boundaries name included/excluded work plainly — no `v1` scope shorthand; optional **External sources** when specs defer presentation or inventory detail to Figma, Confluence, tickets, or host conventions|
|tasks|Terse verbs, paths, done-when — no evaluative wrappers or `v1` endpoint/API labels|
|grill-summaries|Decisions and User inputs use the same scope naming — no `v1` unless quoting an external version|
|skill references|Titles and headings name the capability — not `v1` delivery scope from planning|

## Review severity (spec-review)

|Pattern|Typical severity|
|-|-|
|Evaluative lead removable with zero loss|MAJOR|
|Marketing labels or imagined contrast in Goals/Decisions|MAJOR|
|False agency or drama obscuring facts|MAJOR|
|`sentence-stack` in requirement bodies, Goals, or Decisions (not WHEN/THEN)|MAJOR|
|Clustered patterns or slop hiding actionability|CRITICAL|
|`scope-version-shorthand` in design Boundaries/Decisions, tasks, grill-summaries, or skill references|MAJOR|

Tag findings with pattern id (e.g. `MAJOR | design.md | evaluative-lead`).
