---
name: kb
description: >-
  Ingest URLs, pastes, files, and short notes into the personal LLM wiki at wiki/;
  classify content, summarize, cross-link lightly, and publish via git. Invoke via /kb.
user-invocable: true
argument-hint: >-
  URL, paste, file path, short note, or query; flags --integrate, --file, --no-commit, --no-push, --dry-run
---

# kb

Personal wiki for `q:\source\kb`. Router skill — load one workflow bundle per invoke.

## Entry

1. Resolve mode from user input:

|Invoke|Mode|Bundle|
|-|-|-|
|URL, paste, file path, short note (default)|ingest|[ingest.md](references/ingest.md)|
|Natural-language wiki question (e.g. "what do I know about X?")|query|[query.md](references/query.md)|
|`/kb query …`|query|[query.md](references/query.md)|
|`/kb lint` or health-check ask|lint|[lint.md](references/lint.md)|

2. Read `wiki/SCHEMA.md` before any wiki write.
3. Load the active bundle (one hop from `references/`).
4. Follow bundle procedure and done-when.

## Quick start

|User says|Action|
|-|-|
|`/kb <url>`|ingest → [ingest.md](references/ingest.md)|
|`/kb <paste or note>`|ingest → [ingest.md](references/ingest.md)|
|`/kb --integrate <url>`|ingest (deep) → [ingest.md](references/ingest.md)|
|Wiki question in natural language|query → [query.md](references/query.md)|
|`/kb query "…"`|query → [query.md](references/query.md)|
|`/kb lint`|lint → [lint.md](references/lint.md)|

## References

[ingest](references/ingest.md) · [query](references/query.md) · [lint](references/lint.md)
