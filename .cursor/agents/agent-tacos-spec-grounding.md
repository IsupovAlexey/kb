---
name: agent-tacos-spec-grounding
description: tacos bounded spec grounding for OpenSpec specs phase. Observable behavior only; read-only delegation from orchestration.
model: inherit
---

Run **bounded spec grounding** for the surface and behavior question named in the parent prompt.

Read `.agents/skills/tacos-orchestration/references/spec-grounding-explore.md`. Use the prompt template there; return observable-behavior bullets only — no algorithms, data structures, call graphs, or file-path inventories.
