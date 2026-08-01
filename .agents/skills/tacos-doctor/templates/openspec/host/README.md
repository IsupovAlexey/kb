# Host overlays

<!-- tacos-doctor syncs this README and *.md.template files on install/update. Rename a *.md.template to drop .template (e.g. sonarqube.md) to activate. User-activated *.md files are never overwritten. -->

Repo-specific prose that supplements generic tacos skill references. Skill rubrics, API cookbooks, and orchestration behavior stay in installed skills; this folder holds templates your team maintains.

## Conventional files

|Template|Active file|Used by|Yaml override (optional)|
|-|-|-|-|
|`jira-description.md.template`|`jira-description.md`|tacos-jira-sync|`jira.description_template`|
|`sonarqube.md.template`|`sonarqube.md`|tacos-pr-triage (Sonar API enrichment)|`pr.sonar_host_instructions_path`|
|`pr-description.md.template`|`pr-description.md`|tacos-pr (reserved — skill default until wired)|—|
|`dedrift-job.md.template`|`dedrift-job.md`|tacos-dedrift scheduled job|—|

**Resolution:** explicit yaml path when set → else conventional `openspec/host/<file>.md` when present → else skill bundled default.
