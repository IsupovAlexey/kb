using System.Text;

partial class Program
{
    static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    const string TacosBeginMarker = "<!-- tacos-config-begin -->";
    const string TacosEndMarker = "<!-- tacos-config-end -->";
    const string TacosAgentsBeginMarker = "<!-- tacos-agents-begin -->";
    const string TacosAgentsEndMarker = "<!-- tacos-agents-end -->";
    const string TacosImplementationGatesBeginMarker = "<!-- tacos-implementation-gates-begin -->";
    const string TacosImplementationGatesEndMarker = "<!-- tacos-implementation-gates-end -->";
    const string SandboxArtifactsSubdir = "schema";
    const string TacosBackupSubdir = "tacos-backup";
    const string SkillsPrefixPlaceholder = "{{SKILLS_PREFIX}}";
    const string DefaultSkillsPrefix = ".agents/skills";

    const string TacosOrchestrationContextHookTemplate = """
          <!-- tacos-config-begin -->
          OpenSpec workflow: When openspec/tacos.yaml has orchestration.enabled: true, read {{SKILLS_PREFIX}}/tacos-orchestration/SKILL.md Entry first (command read graph + conditional refs), then references/openspec-commands.md.
          Normative hub: {{SKILLS_PREFIX}}/tacos-orchestration/references/orchestration-binding.md (hub wins on conflict with read graphs).
          Read graphs: references/read-graphs/explore.md, propose.md, update.md, apply.md, sync.md, verify.md, archive.md, tacos-work.md.
          Six override invariants (compact): planning grill sequence; explore≠planning grill; POST-ARTIFACT when apply-ready; MUST-delegate matrix; OpenSpec validate stops on sync/archive/verify; stock-override mini-table+link (full matrix on demand for propose/apply).
          BINDING: Stock OpenSpec CLI and host opsx steps are incomplete for tacos. MUST NOT follow stock opsx when conflicting with tacos-orchestration or tacos-grill. MUST NOT use stock "prefer reasonable decisions" instead of tacos-grill when orchestration.grill_enabled is true. MUST load orchestration-binding.md § Stock overrides before inferring workflow from opsx. Canonical: references/stock-override-binding.md.
          CONTENT OWNERSHIP: proposal → specs → design → tasks — each artifact adds delta only; do not restate upstream content. Full matrix: {{SKILLS_PREFIX}}/tacos-orchestration/references/planning-artifact-loop.md ## Content ownership. Generation-time contract (proposal/specs/tasks gates): same file ### Generation-time contract.
          APPLY: schema apply.instruction overrides stock opsx-apply — Stage grill first when pending; staged apply per task-stage-contract.md.
          ARTIFACT REMOVAL: delete outright when user asks to remove openspec/changes/** content — read artifact-editing.md; no tombstones.
          Implementation gates: AGENTS.md <!-- tacos-implementation-gates-begin --> … <!-- tacos-implementation-gates-end -->.
          Project overview hooks: {{SKILLS_PREFIX}}/tacos-orchestration/references/project-overview-hooks.md when project_overview.enabled.
          DIRECT OUTPUT (ambient): Lead chat with answer or next action; cut zero-information preamble and closers; headings only when ≥2 parts. Carve-outs win: structured gates, grill/review/handoff templates, explore analysis — {{SKILLS_PREFIX}}/tacos-direct-output/references/direct-output.md. No per-turn workflow restatement. Planning artifacts: artifact-prose.md, not chat voice.
          <!-- tacos-config-end -->

        """;

    static string ApplySkillsPrefixSubstitution(string text, string skillsPrefix) =>
        text.Replace(SkillsPrefixPlaceholder, skillsPrefix, StringComparison.Ordinal);

    static string BuildOrchestrationContextHook(string repoRoot)
    {
        var hook = ApplySkillsPrefixSubstitution(
            TacosOrchestrationContextHookTemplate,
            ResolveSkillsPrefixForLayout(repoRoot)
        );
        var workspaceLine = BuildWorkspaceScopeContextLine(repoRoot);
        return workspaceLine is null
            ? hook
            : hook.Replace(
                TacosEndMarker,
                workspaceLine + "\n" + TacosEndMarker,
                StringComparison.Ordinal
            );
    }

    static string ResolveSkillsPrefixForLayout(string repoRoot) => ResolveSkillsPrefix(repoRoot);

    static string? BuildWorkspaceScopeContextLine(string repoRoot)
    {
        var tacosYamlPath = Path.Combine(repoRoot, "openspec", "tacos.yaml");
        if (!File.Exists(tacosYamlPath))
        {
            return null;
        }

        var workspace = TryParseWorkspaceConfig(tacosYamlPath);
        if (workspace is null || workspace.Folders.Count == 0)
        {
            return null;
        }

        var folderList = string.Join(", ", workspace.Folders.Select(f => $"{f.Name} ({f.Path})"));
        return $"Workspace scope (apply-review): {folderList} — resolve from layout-root openspec/tacos.yaml workspace.folders.";
    }

    static string ExpandUserPath(string path)
    {
        var trimmed = path.Trim();
        if (trimmed == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (
            trimmed.StartsWith("~/", StringComparison.Ordinal)
            || trimmed.StartsWith("~\\", StringComparison.Ordinal)
        )
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, trimmed[2..]);
        }

        return Path.GetFullPath(trimmed);
    }
}
