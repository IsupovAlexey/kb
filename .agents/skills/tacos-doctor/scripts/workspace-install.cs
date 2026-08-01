using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

partial class Program
{
    static readonly JsonSerializerOptions FoldersJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    sealed class FolderJsonEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }
    }

    static string? workspaceInitIslandId;
    static string? workspaceInitLayoutRoot;
    static string? workspaceInitFoldersJson;

    static bool IsLayoutRootFolderPath(string path)
    {
        var normalized = path.Trim().Replace('\\', '/').TrimEnd('/');
        return normalized is "." or "./";
    }

    static List<WorkspaceFolderEntry> FoldersForEntryArtifacts(
        string layoutRoot,
        WorkspaceConfig workspace
    )
    {
        var folders = workspace.Folders.ToList();
        if (folders.Any(f => IsLayoutRootFolderPath(f.Path)))
        {
            return folders;
        }

        var layoutName = new DirectoryInfo(layoutRoot).Name;
        folders.Insert(0, new WorkspaceFolderEntry(layoutName, "."));
        return folders;
    }

    static bool TryParseWorkspaceInitArgs(string[] args, ref int i)
    {
        switch (args[i])
        {
            case "--island-id" when i + 1 < args.Length:
                workspaceInitIslandId = args[++i];
                return true;
            case "--layout-root" when i + 1 < args.Length:
                workspaceInitLayoutRoot = args[++i];
                return true;
            case "--folders-json" when i + 1 < args.Length:
                workspaceInitFoldersJson = args[++i];
                return true;
            default:
                return false;
        }
    }

    static IReadOnlyList<WorkspaceFolderEntry> ParseFoldersJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("--folders-json is required");
        }

        var entries = JsonSerializer.Deserialize<List<FolderJsonEntry>>(json, FoldersJsonOptions);
        if (entries is null || entries.Count == 0)
        {
            throw new ArgumentException("--folders-json must be a non-empty JSON array");
        }

        var folders = new List<WorkspaceFolderEntry>();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.Path))
            {
                throw new ArgumentException("each folder entry requires name and path");
            }

            folders.Add(new WorkspaceFolderEntry(entry.Name.Trim(), entry.Path.Trim()));
        }

        return folders;
    }

    static int RunWorkspaceInit((string SkillRoot, string SchemaSource) bundle, bool dryRun)
    {
        try
        {
            var hasIsland = !string.IsNullOrWhiteSpace(workspaceInitIslandId);
            var hasLayoutRoot = !string.IsNullOrWhiteSpace(workspaceInitLayoutRoot);
            if (hasIsland == hasLayoutRoot)
            {
                Console.Error.WriteLine(
                    "workspace-init requires exactly one of --island-id or --layout-root"
                );
                return 1;
            }

            var folders = ParseFoldersJson(workspaceInitFoldersJson).ToList();
            string layoutRoot;
            string? gitRoot;

            if (hasIsland)
            {
                var islandId = workspaceInitIslandId!.Trim();
                if (islandId.Contains('/') || islandId.Contains('\\') || islandId.Contains('.'))
                {
                    Console.Error.WriteLine("island-id must be a single directory name");
                    return 1;
                }

                gitRoot = FindGitRepositoryRoot(Directory.GetCurrentDirectory());
                if (gitRoot is null)
                {
                    Console.Error.WriteLine(
                        "workspace-init with --island-id requires a git repository root"
                    );
                    return 1;
                }

                layoutRoot = Path.Combine(gitRoot, "tacos-workspaces", islandId);
            }
            else
            {
                layoutRoot = ExpandUserPath(workspaceInitLayoutRoot!);
                if (IsNestedUnderTeamIsland(layoutRoot))
                {
                    Console.Error.WriteLine(
                        "workspace layout MUST NOT nest under tacos-workspaces/<island>/ — use an off-repo path or the island layout root"
                    );
                    return 1;
                }

                gitRoot = FindGitRepositoryRoot(layoutRoot);
            }

            var workspace = new WorkspaceConfig(folders);
            ScaffoldWorkspaceBundle(bundle.SkillRoot, layoutRoot, workspace, gitRoot, dryRun);

            if (hasIsland)
            {
                Console.WriteLine(
                    $"workspace-init: {Path.GetRelativePath(gitRoot!, layoutRoot).Replace('\\', '/')}"
                );
            }
            else
            {
                Console.WriteLine($"workspace-init: {layoutRoot.Replace('\\', '/')}");
            }

            Console.WriteLine(
                "Next: cd to layout root, install skills, then dotnet scripts/schema.cs set-schema"
            );
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"workspace-init failed: {ex.Message}");
            return 1;
        }
    }

    static void ScaffoldWorkspaceBundle(
        string skillRoot,
        string layoutRoot,
        WorkspaceConfig workspace,
        string? enclosingGitRoot,
        bool dryRun
    )
    {
        var openspecDir = Path.Combine(layoutRoot, "openspec");
        var tacosYamlPath = Path.Combine(openspecDir, "tacos.yaml");

        if (!dryRun)
        {
            Directory.CreateDirectory(openspecDir);
            Directory.CreateDirectory(Path.Combine(layoutRoot, "artifacts"));
        }

        WriteWorkspaceTacosYaml(tacosYamlPath, workspace, skillRoot, dryRun);
        GenerateEntryArtifacts(layoutRoot, workspace, dryRun);
        ScaffoldLayoutReadme(layoutRoot, dryRun);
        EnsureLayoutAgentsStub(layoutRoot, dryRun);

        foreach (var folder in workspace.Folders)
        {
            if (IsLayoutRootFolderPath(folder.Path))
            {
                continue;
            }

            var resolved = Path.GetFullPath(Path.Combine(layoutRoot, folder.Path));
            if (!Directory.Exists(resolved) && !dryRun)
            {
                Console.WriteLine(
                    $"WARN folder path does not resolve from layout root: {folder.Path}"
                );
            }
        }

        if (IsTeamIslandLayoutRoot(layoutRoot))
        {
            EnsureLayoutIslandTacosGitignore(layoutRoot, dryRun);
        }
    }

    static void WriteWorkspaceTacosYaml(
        string tacosYamlPath,
        WorkspaceConfig workspace,
        string skillRoot,
        bool dryRun
    )
    {
        var templatePath = Path.Combine(skillRoot, "templates", "openspec", "tacos.yaml");
        var baseYaml =
            File.Exists(tacosYamlPath) ? File.ReadAllText(tacosYamlPath, Utf8NoBom)
            : File.Exists(templatePath) ? File.ReadAllText(templatePath, Utf8NoBom)
            : "version: \"1.0.0\"\n";

        var workspaceYaml = BuildWorkspaceYamlBlock(workspace);
        var merged = MergeWorkspaceIntoTacosYaml(baseYaml, workspaceYaml);
        if (dryRun)
        {
            Console.WriteLine($"[dry-run] write {Path.GetFileName(tacosYamlPath)} workspace block");
            return;
        }

        File.WriteAllText(tacosYamlPath, merged, Utf8NoBom);
        Console.WriteLine($"write {tacosYamlPath.Replace('\\', '/')}");
    }

    static string BuildWorkspaceYamlBlock(WorkspaceConfig workspace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("workspace:");
        sb.AppendLine("  folders:");
        foreach (var folder in workspace.Folders)
        {
            sb.AppendLine($"    - name: {YamlQuote(folder.Name)}");
            sb.AppendLine($"      path: {YamlQuote(folder.Path)}");
        }

        return sb.ToString().TrimEnd();
    }

    static string YamlQuote(string value)
    {
        if (
            value.Contains(':', StringComparison.Ordinal)
            || value.Contains('#', StringComparison.Ordinal)
            || value.Contains('\\', StringComparison.Ordinal)
            || value.StartsWith(' ')
            || value.EndsWith(' ')
        )
        {
            return $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";
        }

        return value;
    }

    static string MergeWorkspaceIntoTacosYaml(string baseYaml, string workspaceYaml)
    {
        var lines = baseYaml.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
        var start = lines.FindIndex(l => l.StartsWith("workspace:", StringComparison.Ordinal));
        if (start >= 0)
        {
            var end = start + 1;
            while (end < lines.Count)
            {
                var trimmed = lines[end].TrimStart();
                if (trimmed.Length == 0)
                {
                    end++;
                    continue;
                }

                if (!lines[end].StartsWith(' ') && !lines[end].StartsWith('\t'))
                {
                    break;
                }

                end++;
            }

            lines.RemoveRange(start, end - start);
        }

        if (lines.Count > 0 && lines[^1].Length > 0)
        {
            lines.Add("");
        }

        lines.AddRange(workspaceYaml.Split('\n'));
        var merged = string.Join("\n", lines);
        if (!merged.EndsWith('\n'))
        {
            merged += "\n";
        }

        return merged;
    }

    static void GenerateEntryArtifacts(string layoutRoot, WorkspaceConfig workspace, bool dryRun)
    {
        var layoutName = new DirectoryInfo(layoutRoot).Name;
        var codeWorkspacePath = Path.Combine(layoutRoot, $"{layoutName}.code-workspace");
        var claudeSettingsPath = Path.Combine(layoutRoot, ".claude", "settings.json");
        var entryFolders = FoldersForEntryArtifacts(layoutRoot, workspace);

        var codeWorkspaceJson =
            "{\n"
            + "  \"folders\": [\n"
            + string.Join(
                ",\n",
                entryFolders.Select(f =>
                    $"    {{ \"name\": {JsonSerializer.Serialize(f.Name)}, \"path\": {JsonSerializer.Serialize(f.Path)} }}"
                )
            )
            + "\n  ]\n"
            + "}\n";

        var additionalDirs = entryFolders
            .Where(f => !IsLayoutRootFolderPath(f.Path))
            .Select(f => f.Path)
            .ToList();
        var claudeJson =
            "{\n"
            + "  \"permissions\": {\n"
            + "    \"additionalDirectories\": "
            + JsonSerializer.Serialize(additionalDirs)
            + "\n  }\n"
            + "}\n";

        if (dryRun)
        {
            Console.WriteLine($"[dry-run] generate {layoutName}.code-workspace");
            Console.WriteLine("[dry-run] generate .claude/settings.json");
            return;
        }

        Directory.CreateDirectory(Path.Combine(layoutRoot, ".claude"));
        File.WriteAllText(codeWorkspacePath, codeWorkspaceJson, Utf8NoBom);
        File.WriteAllText(claudeSettingsPath, claudeJson, Utf8NoBom);
        Console.WriteLine($"generate {Path.GetFileName(codeWorkspacePath)}");
        Console.WriteLine("generate .claude/settings.json");
    }

    static void ScaffoldLayoutReadme(string layoutRoot, bool dryRun)
    {
        var readmePath = Path.Combine(layoutRoot, "README.md");
        if (File.Exists(readmePath))
        {
            return;
        }

        var layoutName = new DirectoryInfo(layoutRoot).Name;
        var text = $"""
            # {layoutName}

            tacos workspace layout root.

            """;

        if (dryRun)
        {
            Console.WriteLine("[dry-run] scaffold README.md");
            return;
        }

        File.WriteAllText(readmePath, text, Utf8NoBom);
        Console.WriteLine("scaffold README.md");
    }

    static void EnsureLayoutAgentsStub(string layoutRoot, bool dryRun)
    {
        var agentsPath = Path.Combine(layoutRoot, "AGENTS.md");
        if (File.Exists(agentsPath))
        {
            return;
        }

        if (dryRun)
        {
            Console.WriteLine("[dry-run] scaffold AGENTS.md stub");
            return;
        }

        File.WriteAllText(agentsPath, "# AGENTS\n\n", Utf8NoBom);
        Console.WriteLine("scaffold AGENTS.md stub");
    }

    static readonly string[] LayoutIslandTacosGitignoreLines =
    [
        "*/skills/tacos-*",
        "*/agents/agent-tacos-*",
        "*/skills/openspec-*",
        "*/commands/opsx-*",
        "*/commands/opsx/*",
        "*/prompts/opsx-*",
        "*/workflows/opsx-*",
        "skills-lock.json",
        "global.json",
        "openspec/host/*.template",
        "openspec/host/README.md",
    ];

    static void EnsureLayoutIslandTacosGitignore(string layoutRoot, bool dryRun)
    {
        EnsureGitignoreLines(
            layoutRoot,
            LayoutIslandTacosGitignoreLines,
            dryRun,
            "island .gitignore for tacos host trees and local tooling files"
        );
    }

    static void EnsureGitignoreLines(
        string layoutRoot,
        string[] lines,
        bool dryRun,
        string actionLabel
    )
    {
        var gitignorePath = Path.Combine(layoutRoot, ".gitignore");
        var block = string.Join("\n", lines) + "\n\n";

        if (File.Exists(gitignorePath))
        {
            var existing = File.ReadAllText(gitignorePath, Utf8NoBom);
            var missing = lines
                .Where(line => !existing.Contains(line, StringComparison.Ordinal))
                .ToList();
            if (missing.Count == 0)
            {
                return;
            }

            if (dryRun)
            {
                Console.WriteLine(
                    $"[dry-run] append missing paths to island .gitignore ({actionLabel})"
                );
                return;
            }

            var separator = existing.Length > 0 && !existing.EndsWith('\n') ? "\n" : "";
            File.AppendAllText(
                gitignorePath,
                separator + string.Join("\n", missing) + "\n\n",
                Utf8NoBom
            );
            Console.WriteLine($"append island .gitignore ({actionLabel})");
            return;
        }

        if (dryRun)
        {
            Console.WriteLine("[dry-run] create island .gitignore");
            return;
        }

        Directory.CreateDirectory(layoutRoot);
        File.WriteAllText(gitignorePath, block, Utf8NoBom);
        Console.WriteLine("create island .gitignore");
    }

    static void RegenerateEntryArtifactsFromLayout(string layoutRoot, bool dryRun)
    {
        var tacosYamlPath = Path.Combine(layoutRoot, "openspec", "tacos.yaml");
        if (!File.Exists(tacosYamlPath))
        {
            return;
        }

        var workspace = TryParseWorkspaceConfig(tacosYamlPath);
        if (workspace is null)
        {
            return;
        }

        var gitRoot = FindGitRepositoryRoot(layoutRoot);
        if (ShouldGenerateEntryArtifacts(layoutRoot, gitRoot))
        {
            GenerateEntryArtifacts(layoutRoot, workspace, dryRun);
        }

        InjectWorkspaceFoldersIntoConfig(layoutRoot, workspace, dryRun);

        if (IsTeamIslandLayoutRoot(layoutRoot))
        {
            EnsureLayoutIslandTacosGitignore(layoutRoot, dryRun);
        }
    }

    static void InjectWorkspaceFoldersIntoConfig(
        string layoutRoot,
        WorkspaceConfig workspace,
        bool dryRun
    )
    {
        var configPath = Path.Combine(layoutRoot, "openspec", "config.yaml");
        if (!File.Exists(configPath))
        {
            return;
        }

        var (applied, message) = EnsureOrchestrationContextHook(
            configPath,
            forceRefresh: false,
            dryRun,
            layoutRoot
        );
        if (applied || dryRun)
        {
            Console.WriteLine(dryRun ? $"[dry-run] {message}" : message);
        }
    }

    static (bool Applied, string Message) MergeSingleDefaultWorkspace(
        string tacosYamlPath,
        string _,
        bool dryRun
    )
    {
        if (!File.Exists(tacosYamlPath))
        {
            return (false, "openspec/tacos.yaml missing");
        }

        var existing = TryParseWorkspaceConfig(tacosYamlPath);
        if (existing is not null)
        {
            return (false, "openspec/tacos.yaml already has workspace block");
        }

        var workspace = new WorkspaceConfig([new WorkspaceFolderEntry("repository", ".")]);
        var baseYaml = File.ReadAllText(tacosYamlPath, Utf8NoBom);
        var merged = MergeWorkspaceIntoTacosYaml(baseYaml, BuildWorkspaceYamlBlock(workspace));
        if (dryRun)
        {
            return (true, "merge default single workspace into openspec/tacos.yaml");
        }

        File.WriteAllText(tacosYamlPath, merged, Utf8NoBom);
        return (true, "merge default single workspace into openspec/tacos.yaml");
    }
}
