using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    internal sealed record WorkspaceFolderEntry(string Name, string Path);

    internal sealed record WorkspaceConfig(IReadOnlyList<WorkspaceFolderEntry> Folders);

    internal enum LayoutModeKind
    {
        Single,
        Workspace,
    }

    internal sealed record LayoutContext(
        string LayoutRoot,
        string OpenSpecDir,
        string? GitRoot,
        LayoutModeKind Mode,
        bool HasWorkspace,
        WorkspaceConfig? Workspace,
        string? AmbiguityError
    )
    {
        public bool IsAmbiguous => !string.IsNullOrWhiteSpace(AmbiguityError);

        public string ModeLabel =>
            Mode switch
            {
                LayoutModeKind.Single => "single",
                LayoutModeKind.Workspace => "workspace",
                _ => "unknown",
            };

        public bool IsTeamIslandWorkspace => HasWorkspace && IsTeamIslandLayoutRoot(LayoutRoot);
    }

    static readonly Regex TeamIslandLayoutRootRegex = new(
        @"(?:^|[/\\])tacos-workspaces[/\\][^/\\]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    static readonly Regex TeamIslandPathRegex = new(
        @"(?:^|[/\\])tacos-workspaces[/\\][^/\\]+[/\\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    internal static LayoutContext ResolveLayoutContext(string start)
    {
        var startFull = Path.GetFullPath(start);
        var gitRoot = FindGitRepositoryRoot(startFull);
        var candidates = CollectWorkspaceCandidates(startFull, gitRoot);

        foreach (var candidate in candidates)
        {
            if (
                !IsTeamIslandLayoutRoot(candidate.Directory)
                && IsNestedUnderTeamIsland(candidate.Directory)
            )
            {
                return BuildAmbiguous(
                    gitRoot,
                    "workspace layout MUST NOT nest under tacos-workspaces/<island>/ — use an off-repo path or the island layout root",
                    candidate.Directory
                );
            }

            if (candidate.LegacySkillsAtRoot)
            {
                return BuildAmbiguous(
                    gitRoot,
                    "workspace.skills_at: root is no longer supported — use workspace team island or single at git root",
                    candidate.Directory
                );
            }
        }

        if (candidates.Count > 0)
        {
            var chosen = candidates.OrderByDescending(c => PathDepth(c.Directory)).First();
            if (chosen.Workspace.Folders.Count == 0)
            {
                return BuildAmbiguous(
                    gitRoot,
                    "workspace.folders must be a non-empty array",
                    chosen.Directory
                );
            }

            return BuildWorkspaceLayout(chosen.Directory, chosen.Workspace, gitRoot);
        }

        if (gitRoot is not null)
        {
            return BuildSingleLayout(gitRoot, gitRoot);
        }

        var openspecRoot = FindOpenSpecRepoRoot(startFull);
        if (openspecRoot is not null)
        {
            return BuildSingleLayout(openspecRoot, gitRoot: null);
        }

        return new LayoutContext(
            LayoutRoot: startFull,
            OpenSpecDir: Path.Combine(startFull, "openspec"),
            GitRoot: null,
            Mode: LayoutModeKind.Single,
            HasWorkspace: false,
            Workspace: null,
            AmbiguityError: null
        );
    }

    static LayoutContext BuildAmbiguous(string? gitRoot, string message, string? anchorDir = null)
    {
        var layoutRoot = gitRoot ?? anchorDir ?? Directory.GetCurrentDirectory();
        var error = anchorDir is not null
            ? $"{message} (layout: {anchorDir.Replace('\\', '/')})"
            : message;
        return new(
            LayoutRoot: layoutRoot,
            OpenSpecDir: gitRoot is not null
                ? Path.Combine(gitRoot, "openspec")
                : Path.Combine(layoutRoot, "openspec"),
            GitRoot: gitRoot,
            Mode: LayoutModeKind.Single,
            HasWorkspace: false,
            Workspace: null,
            AmbiguityError: error
        );
    }

    static LayoutContext BuildSingleLayout(string layoutRoot, string? gitRoot) =>
        new(
            LayoutRoot: layoutRoot,
            OpenSpecDir: Path.Combine(layoutRoot, "openspec"),
            GitRoot: gitRoot,
            Mode: LayoutModeKind.Single,
            HasWorkspace: false,
            Workspace: null,
            AmbiguityError: null
        );

    static LayoutContext BuildWorkspaceLayout(
        string layoutRoot,
        WorkspaceConfig workspace,
        string? gitRoot
    )
    {
        var mode = ResolveLayoutMode(layoutRoot, gitRoot);
        return new LayoutContext(
            LayoutRoot: layoutRoot,
            OpenSpecDir: Path.Combine(layoutRoot, "openspec"),
            GitRoot: gitRoot,
            Mode: mode,
            HasWorkspace: true,
            Workspace: workspace,
            AmbiguityError: null
        );
    }

    static LayoutModeKind ResolveLayoutMode(string layoutRoot, string? gitRoot)
    {
        if (IsSingleGitRootWorkspace(layoutRoot, gitRoot))
        {
            return LayoutModeKind.Single;
        }

        return LayoutModeKind.Workspace;
    }

    internal static bool IsSingleGitRootWorkspace(string layoutRoot, string? gitRoot) =>
        gitRoot is not null
        && layoutRoot.Equals(gitRoot, StringComparison.OrdinalIgnoreCase)
        && !IsTeamIslandLayoutRoot(layoutRoot);

    internal static bool IsTeamIslandLayoutRoot(string layoutRoot)
    {
        var normalized = layoutRoot.Replace('\\', '/').TrimEnd('/');
        return TeamIslandLayoutRootRegex.IsMatch(normalized);
    }

    internal static bool ShouldGenerateEntryArtifacts(string layoutRoot, string? gitRoot) =>
        !IsSingleGitRootWorkspace(layoutRoot, gitRoot ?? FindGitRepositoryRoot(layoutRoot));

    internal static string ResolveHostSkillsAnchor(LayoutContext layout) => layout.LayoutRoot;

    internal static string ResolveHostSubagentsAnchor(LayoutContext layout) => layout.LayoutRoot;

    internal static bool IsTeamIslandRepository(string? gitRoot)
    {
        if (string.IsNullOrWhiteSpace(gitRoot))
        {
            return false;
        }

        var workspacesDir = Path.Combine(gitRoot, "tacos-workspaces");
        if (!Directory.Exists(workspacesDir))
        {
            return false;
        }

        return Directory
            .EnumerateDirectories(workspacesDir)
            .Any(d =>
            {
                var name = Path.GetFileName(d);
                if (string.IsNullOrWhiteSpace(name) || name.StartsWith('.'))
                {
                    return false;
                }

                var islandTacos = Path.Combine(d, "openspec", "tacos.yaml");
                return File.Exists(islandTacos) && TryParseWorkspaceConfig(islandTacos) is not null;
            });
    }

    internal static bool ProhibitsGitRootOpenspecProject(LayoutContext layout) =>
        layout.Mode == LayoutModeKind.Single
        && layout.GitRoot is not null
        && layout.LayoutRoot.Equals(layout.GitRoot, StringComparison.OrdinalIgnoreCase)
        && IsTeamIslandRepository(layout.GitRoot);

    internal static IReadOnlyList<string> EnumerateTeamIslandLayoutRoots(string gitRoot)
    {
        var list = new List<string>();
        var workspacesDir = Path.Combine(gitRoot, "tacos-workspaces");
        if (!Directory.Exists(workspacesDir))
        {
            return list;
        }

        foreach (var dir in Directory.EnumerateDirectories(workspacesDir))
        {
            var name = Path.GetFileName(dir);
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith('.'))
            {
                continue;
            }

            list.Add(dir);
        }

        return list.OrderBy(static p => p, StringComparer.OrdinalIgnoreCase).ToList();
    }

    internal static string? ResolveOpenspecProjectRoot(LayoutContext layout)
    {
        if (layout.Mode == LayoutModeKind.Workspace)
        {
            return layout.LayoutRoot;
        }

        if (ProhibitsGitRootOpenspecProject(layout))
        {
            var islands = EnumerateTeamIslandLayoutRoots(layout.GitRoot!);
            return islands.Count == 1 ? islands[0] : null;
        }

        return layout.LayoutRoot;
    }

    internal static bool TryRemoveMistakenGitRootOpenspecProject(string gitRoot, out string message)
    {
        message = "";
        if (!IsTeamIslandRepository(gitRoot))
        {
            return false;
        }

        var rootConfig = Path.Combine(gitRoot, "openspec", "config.yaml");
        if (!File.Exists(rootConfig))
        {
            return false;
        }

        var rootTacos = Path.Combine(gitRoot, "openspec", "tacos.yaml");
        if (
            File.Exists(rootTacos)
            && TryParseWorkspaceConfig(rootTacos) is not null
            && !IsTeamIslandLayoutRoot(gitRoot)
        )
        {
            return false;
        }

        File.Delete(rootConfig);
        message =
            "removed mistaken openspec/config.yaml from git root (OpenSpec project belongs at tacos-workspaces/<island>/)";
        return true;
    }

    static bool IsNestedUnderTeamIsland(string layoutRoot)
    {
        var normalized = layoutRoot.Replace('\\', '/').TrimEnd('/');
        return TeamIslandPathRegex.IsMatch(normalized + "/");
    }

    static int PathDepth(string fullPath) =>
        fullPath
            .Replace('\\', '/')
            .TrimEnd('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Length;

    sealed record WorkspaceCandidate(
        string Directory,
        WorkspaceConfig Workspace,
        bool LegacySkillsAtRoot
    );

    static List<WorkspaceCandidate> CollectWorkspaceCandidates(
        string startFull,
        string? gitRootBoundary
    )
    {
        var candidates = new List<WorkspaceCandidate>();
        var dir = new DirectoryInfo(startFull);
        while (dir is not null)
        {
            var tacosYamlPath = Path.Combine(dir.FullName, "openspec", "tacos.yaml");
            if (File.Exists(tacosYamlPath))
            {
                var parsed = TryParseWorkspaceFromTacosYaml(tacosYamlPath);
                if (parsed is not null)
                {
                    candidates.Add(
                        new WorkspaceCandidate(
                            dir.FullName,
                            parsed.Value.Workspace,
                            parsed.Value.LegacySkillsAtRoot
                        )
                    );
                }
            }

            if (
                gitRootBoundary is not null
                && dir.FullName.Equals(gitRootBoundary, StringComparison.OrdinalIgnoreCase)
            )
            {
                break;
            }

            dir = dir.Parent;
        }

        return candidates;
    }

    internal static WorkspaceConfig? TryParseWorkspaceConfig(string tacosYamlPath)
    {
        var parsed = TryParseWorkspaceFromTacosYaml(tacosYamlPath);
        return parsed?.Workspace;
    }

    static (WorkspaceConfig Workspace, bool LegacySkillsAtRoot)? TryParseWorkspaceFromTacosYaml(
        string tacosYamlPath
    )
    {
        Dictionary<string, object?> root;
        try
        {
            root = YamlMergeAddOnly.ParseRoot(File.ReadAllText(tacosYamlPath, Encoding.UTF8));
        }
        catch
        {
            return null;
        }

        if (!TryGetMapping(root, "workspace", out var workspaceMap))
        {
            return null;
        }

        var skillsAtRaw = TryGetString(workspaceMap, "skills_at")?.Trim().ToLowerInvariant();
        var legacySkillsAtRoot = skillsAtRaw == "root";
        if (skillsAtRaw is not null and not "" and not "island" and not "root")
        {
            return null;
        }

        var folders = new List<WorkspaceFolderEntry>();
        if (TryGetSequence(workspaceMap, "folders", out var folderItems))
        {
            foreach (var item in folderItems)
            {
                if (item is not Dictionary<string, object?> folderMap)
                {
                    continue;
                }

                var name = TryGetString(folderMap, "name");
                var path = TryGetString(folderMap, "path");
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                folders.Add(new WorkspaceFolderEntry(name.Trim(), path.Trim()));
            }
        }

        return (new WorkspaceConfig(folders), legacySkillsAtRoot);
    }

    static string? FindGitRepositoryRoot(string startFull)
    {
        var dir = new DirectoryInfo(startFull);
        while (dir is not null)
        {
            var gitPath = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    static bool TryGetMapping(
        Dictionary<string, object?> map,
        string key,
        out Dictionary<string, object?> value
    )
    {
        value = new Dictionary<string, object?>();
        if (!map.TryGetValue(key, out var raw) || raw is not Dictionary<string, object?> nested)
        {
            return false;
        }

        value = nested;
        return true;
    }

    static bool TryGetSequence(Dictionary<string, object?> map, string key, out List<object?> items)
    {
        items = new List<object?>();
        if (!map.TryGetValue(key, out var raw) || raw is not List<object?> list)
        {
            return false;
        }

        items = list;
        return true;
    }

    static string? TryGetString(Dictionary<string, object?> map, string key) =>
        map.TryGetValue(key, out var raw) && raw is not null ? raw.ToString() : null;

    static bool? TryGetBool(Dictionary<string, object?> map, string key)
    {
        if (!map.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        return raw switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            _ => null,
        };
    }
}
