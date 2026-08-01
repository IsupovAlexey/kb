# Overview guidance

**Goal:** Keep the overview **compact and meaningful**. Most sync/archive changes need **no** overview edit. Update only when readers would miss something important without it.

## Inputs

- `project_overview.path` from config; existing overview file; repo layout.
- `openspec/specs/` when scope needs stable capability context.
- Sync/archive: that change’s `proposal`, `design`, `specs/**` — human summary only.
- Manual: user-listed sections or topics only.

Discover other sources (config files, skill/plugin catalogs, package READMEs) **only when scope asks** for those sections.

## Update rules

- **Major changes only** warrant new sections or long rewrites (new capability, breaking setup, new integration readers must configure). Routine or internal work → small edit or **no edit**.
- **Concise:** short bullets or a tight paragraph; no spec dumps, changelogs, or task lists.
- **Preserve** prose outside scope; do not bloat the file.
- Do not paste `openspec/changes/**` or raw delta specs into the overview.

## Typical sections (when in scope)

Match the file on disk: product summary, install, prerequisites, configuration, features, tools/plugins, contributing. Add a section only if scope requires it.

## Sync / archive scope

For Gate 2, propose a **scope plan** before draft: would an onboarding reader care? If not, plan **no overview change**. If yes, list the smallest sections/topics to add, update, or remove. Skip task stages, grill, review paths, and agent meta. Draft only what the user approved in the plan.
