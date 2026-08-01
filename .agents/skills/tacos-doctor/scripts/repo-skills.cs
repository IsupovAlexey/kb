using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    // Project-local skills roots aligned with OpenSpec supported-tools.md
    // (https://github.com/Fission-AI/OpenSpec/blob/main/docs/supported-tools.md)
    // plus `.agents/skills` (Cursor/Codex/OpenCode compatibility).
    // Probe order: common hosts first, then OpenSpec table (alphabetical by tool dir).
    static readonly string[] HostSkillsRootCandidates =
    [
        ".agents/skills",
        ".claude/skills",
        ".cursor/skills",
        ".github/skills",
        ".opencode/skills",
        ".codex/skills",
        ".agent/skills",
        ".amazonq/skills",
        ".augment/skills",
        ".bob/skills",
        ".cline/skills",
        ".codebuddy/skills",
        ".continue/skills",
        ".cospec/skills",
        ".crush/skills",
        ".factory/skills",
        ".forge/skills",
        ".gemini/skills",
        ".iflow/skills",
        ".junie/skills",
        ".kilocode/skills",
        ".kimi/skills",
        ".kiro/skills",
        ".lingma/skills",
        ".pi/skills",
        ".qoder/skills",
        ".qwen/skills",
        ".roo/skills",
        ".trae/skills",
        ".windsurf/skills",
    ];

    internal static bool HasOpenspecHostSkills(string repoRoot)
    {
        foreach (var candidate in HostSkillsRootCandidates)
        {
            var skillsDir = CombineRepoRelative(repoRoot, candidate);
            if (!Directory.Exists(skillsDir))
            {
                continue;
            }

            if (Directory.EnumerateDirectories(skillsDir, "openspec-*").Any())
            {
                return true;
            }
        }

        return false;
    }

    static readonly (string RelativeDir, string Glob)[] OpenspecHostCommandLocations =
    [
        (".cursor/commands", "opsx-*.md"),
        (".claude/commands/opsx", "*.md"),
        (".github/prompts", "opsx-*.prompt.md"),
        (".augment/commands", "opsx-*.md"),
        (".opencode/commands", "opsx-*.md"),
        (".windsurf/workflows", "opsx-*.md"),
        (".kilocode/workflows", "opsx-*.md"),
    ];

    internal static bool HasOpenspecHostCommands(string repoRoot)
    {
        foreach (var (relativeDir, glob) in OpenspecHostCommandLocations)
        {
            var commandDir = CombineRepoRelative(repoRoot, relativeDir);
            if (!Directory.Exists(commandDir))
            {
                continue;
            }

            if (Directory.EnumerateFiles(commandDir, glob).Any())
            {
                return true;
            }
        }

        return false;
    }

    static bool IsOpsxUpdateCommandFile(string relativeDir, string fileName) =>
        fileName.Equals("opsx-update.md", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("opsx-update.prompt.md", StringComparison.OrdinalIgnoreCase)
        || (
            relativeDir.Equals(".claude/commands/opsx", StringComparison.Ordinal)
            && fileName.Equals("update.md", StringComparison.OrdinalIgnoreCase)
        );

    internal static bool HasOpsxUpdateCommand(string repoRoot)
    {
        var anyActiveCommandDir = false;
        foreach (var (relativeDir, glob) in OpenspecHostCommandLocations)
        {
            var commandDir = CombineRepoRelative(repoRoot, relativeDir);
            if (!Directory.Exists(commandDir))
            {
                continue;
            }

            var files = Directory.EnumerateFiles(commandDir, glob).ToList();
            if (files.Count == 0)
            {
                continue;
            }

            anyActiveCommandDir = true;
            if (!files.Any(file => IsOpsxUpdateCommandFile(relativeDir, Path.GetFileName(file))))
            {
                return false;
            }
        }

        return anyActiveCommandDir;
    }

    internal static bool HasOpenspecHostArtifacts(string repoRoot) =>
        HasOpenspecHostSkills(repoRoot) || HasOpenspecHostCommands(repoRoot);

    internal const string OpenspecDeliveryBoth = "both";
    internal const string OpenspecDeliveryCommands = "commands";
    internal const string OpenspecDeliverySkills = "skills";

    internal static bool OpenspecDeliveryIncludesCommands(string deliveryMode) =>
        NormalizeOpenspecDeliveryMode(deliveryMode)
            is OpenspecDeliveryBoth
                or OpenspecDeliveryCommands;

    internal static bool OpenspecDeliveryIncludesSkills(string deliveryMode) =>
        NormalizeOpenspecDeliveryMode(deliveryMode)
            is OpenspecDeliveryBoth
                or OpenspecDeliverySkills;

    internal static string NormalizeOpenspecDeliveryMode(string? deliveryMode)
    {
        if (deliveryMode is null)
        {
            return OpenspecDeliveryBoth;
        }

        var normalized = deliveryMode.Trim();
        if (normalized.Length >= 2 && normalized[0] == '"' && normalized[^1] == '"')
        {
            normalized = normalized[1..^1];
        }

        return normalized.Equals(OpenspecDeliveryCommands, StringComparison.OrdinalIgnoreCase)
                ? OpenspecDeliveryCommands
            : normalized.Equals(OpenspecDeliverySkills, StringComparison.OrdinalIgnoreCase)
                ? OpenspecDeliverySkills
            : OpenspecDeliveryBoth;
    }

    internal static bool HasCompleteOpenspecHostArtifacts(
        string repoRoot,
        string deliveryMode = OpenspecDeliveryBoth
    )
    {
        var commandsRequired = OpenspecDeliveryIncludesCommands(deliveryMode);
        var skillsRequired = OpenspecDeliveryIncludesSkills(deliveryMode);
        var commandsOk = !commandsRequired || HasOpsxUpdateCommand(repoRoot);
        var skillsOk = !skillsRequired || HasOpenspecHostSkills(repoRoot);
        return commandsOk && skillsOk;
    }

    internal static string DescribeOpenspecDeliveryArtifacts(string deliveryMode)
    {
        var commands = OpenspecDeliveryIncludesCommands(deliveryMode);
        var skills = OpenspecDeliveryIncludesSkills(deliveryMode);
        if (commands && skills)
        {
            return "openspec-* host skills and opsx-update command";
        }

        if (commands)
        {
            return "opsx-update command";
        }

        if (skills)
        {
            return "openspec-* host skills";
        }

        return "OpenSpec host artifacts";
    }

    internal static void RelocateOpenspecHostArtifacts(string fromRoot, string toRoot)
    {
        if (
            Path.GetFullPath(fromRoot)
                .Equals(Path.GetFullPath(toRoot), StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        foreach (var candidate in HostSkillsRootCandidates)
        {
            var fromSkills = CombineRepoRelative(fromRoot, candidate);
            if (!Directory.Exists(fromSkills))
            {
                continue;
            }

            foreach (var dir in Directory.EnumerateDirectories(fromSkills, "openspec-*"))
            {
                var name = Path.GetFileName(dir);
                var toSkills = CombineRepoRelative(toRoot, candidate);
                Directory.CreateDirectory(toSkills);
                var dest = Path.Combine(toSkills, name);
                if (Directory.Exists(dest))
                {
                    Directory.Delete(dest, recursive: true);
                }

                Directory.Move(dir, dest);
            }
        }

        foreach (var (relativeDir, glob) in OpenspecHostCommandLocations)
        {
            var fromDir = CombineRepoRelative(fromRoot, relativeDir);
            if (!Directory.Exists(fromDir))
            {
                continue;
            }

            var toDir = CombineRepoRelative(toRoot, relativeDir);
            Directory.CreateDirectory(toDir);
            foreach (var file in Directory.EnumerateFiles(fromDir, glob))
            {
                var dest = Path.Combine(toDir, Path.GetFileName(file));
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                File.Move(file, dest);
            }
        }
    }

    internal static bool IsOpenspecProjectInitialized(string repoRoot) =>
        File.Exists(Path.Combine(repoRoot, "openspec", "config.yaml"));

    static string[] HostSkillContainerDirs =>
        HostSkillsRootCandidates
            .Select(c => c.Split('/')[0])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    static string[] ForbiddenHostSkillsRootPatterns =>
        HostSkillsRootCandidates.Select(c => c.Replace('\\', '/')).ToArray();

    const string ArtifactsGitignoreLine = "artifacts/";

    // Cursor loads project skills from .agents/skills/; .cursor/skills/ is redundant when both exist.
    internal const string CursorCanonicalSkillsPrefix = ".agents/skills";
    internal const string CursorLegacySkillsPrefix = ".cursor/skills";

    internal static bool HasTacosOrchestrationSkill(string repoRoot, string skillsPrefix) =>
        File.Exists(CombineRepoRelative(repoRoot, skillsPrefix, "tacos-orchestration", "SKILL.md"));

    internal static bool HasRedundantCursorSkillsDuplicate(string repoRoot) =>
        HasTacosOrchestrationSkill(repoRoot, CursorCanonicalSkillsPrefix)
        && HasTacosOrchestrationSkill(repoRoot, CursorLegacySkillsPrefix);

    // npx skills add installs relative to cwd; running inside a skills root nests {parent}/{candidate}/.
    internal static IReadOnlyList<string> EnumerateNestedAccidentalSkillsInstalls(string repoRoot)
    {
        var nested = new HashSet<string>(StringComparer.Ordinal);

        foreach (var parent in HostSkillsRootCandidates)
        {
            var parentNormalized = parent.Replace('\\', '/');
            var parentDir = CombineRepoRelative(repoRoot, parentNormalized);
            if (!Directory.Exists(parentDir))
            {
                continue;
            }

            foreach (var candidate in HostSkillsRootCandidates)
            {
                var candidateNormalized = candidate.Replace('\\', '/');
                var nestedPrefix = $"{parentNormalized}/{candidateNormalized}";
                if (!LooksLikeNestedAccidentalSkillsInstall(repoRoot, nestedPrefix))
                {
                    continue;
                }

                nested.Add(nestedPrefix);
            }
        }

        return nested.OrderBy(static x => x, StringComparer.Ordinal).ToList();
    }

    static bool LooksLikeNestedAccidentalSkillsInstall(string repoRoot, string nestedPrefix)
    {
        if (HasTacosOrchestrationSkill(repoRoot, nestedPrefix))
        {
            return true;
        }

        var nestedDir = CombineRepoRelative(repoRoot, nestedPrefix);
        if (!Directory.Exists(nestedDir))
        {
            return false;
        }

        if (File.Exists(Path.Combine(nestedDir, "skills-lock.json")))
        {
            return true;
        }

        return Directory.EnumerateDirectories(nestedDir, "tacos-*").Any();
    }

    internal static string FormatNestedAccidentalSkillsInstallMessage(string nestedPrefix) =>
        $"{nestedPrefix}/ is an accidental nested skills install (npx skills add was run inside a skills directory) "
        + $"— delete {nestedPrefix}/ then refresh from the OpenSpec project root";

    static bool IsRedundantCursorSkillsRoot(string repoRoot, string normalized) =>
        normalized.Equals(CursorLegacySkillsPrefix, StringComparison.Ordinal)
        && HasTacosOrchestrationSkill(repoRoot, CursorCanonicalSkillsPrefix);

    internal static IReadOnlyList<string> EnumerateHostTacosSkillsPrefixes(string repoRoot)
    {
        var found = new List<string>();
        foreach (var candidate in HostSkillsRootCandidates)
        {
            var normalized = candidate.Replace('\\', '/');
            if (IsRedundantCursorSkillsRoot(repoRoot, normalized))
            {
                continue;
            }

            if (HasTacosOrchestrationSkill(repoRoot, normalized))
            {
                found.Add(normalized);
            }
        }

        return found;
    }

    internal static bool HasGlobalTacosOrchestrationSkill()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(home))
        {
            return false;
        }

        foreach (var prefix in new[] { ".agents/skills", ".cursor/skills", ".claude/skills" })
        {
            if (HasTacosOrchestrationSkill(home, prefix))
            {
                return true;
            }
        }

        return false;
    }

    static string? FindHostTacosSkillsPrefix(string repoRoot) =>
        EnumerateHostTacosSkillsPrefixes(repoRoot).FirstOrDefault();

    internal static string? FindHostTacosSkillsPrefixForHost(string repoRoot, string hostDirName)
    {
        if (hostDirName.Equals(".cursor", StringComparison.Ordinal))
        {
            if (HasTacosOrchestrationSkill(repoRoot, CursorCanonicalSkillsPrefix))
            {
                return CursorCanonicalSkillsPrefix;
            }

            if (HasTacosOrchestrationSkill(repoRoot, CursorLegacySkillsPrefix))
            {
                return CursorLegacySkillsPrefix;
            }

            return FindHostTacosSkillsPrefix(repoRoot);
        }

        if (hostDirName.Equals(".claude", StringComparison.Ordinal))
        {
            const string claudeSkillsRoot = ".claude/skills";
            if (HasTacosOrchestrationSkill(repoRoot, claudeSkillsRoot))
            {
                return claudeSkillsRoot;
            }

            return FindHostTacosSkillsPrefix(repoRoot);
        }

        return FindHostTacosSkillsPrefix(repoRoot);
    }

    internal static string? FindHostRepoRoot(string start)
    {
        var layout = ResolveLayoutContext(start);
        if (layout.IsAmbiguous)
        {
            return null;
        }

        if (
            layout.HasWorkspace
            || layout.GitRoot is not null
            || Directory.Exists(layout.OpenSpecDir)
        )
        {
            return layout.LayoutRoot;
        }

        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (
                Directory.Exists(Path.Combine(dir.FullName, "openspec"))
                || File.Exists(Path.Combine(dir.FullName, "AGENTS.md"))
                || EnumerateHostTacosSkillsPrefixes(dir.FullName).Count > 0
            )
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    static string ResolveSkillsPrefix(string layoutRoot)
    {
        var layout = ResolveLayoutContext(layoutRoot);
        var skillsAnchor = layout.LayoutRoot;

        var prefix = FindHostTacosSkillsPrefix(skillsAnchor) ?? DefaultSkillsPrefix;
        return ToSkillsPrefixRelativeToLayout(layout.LayoutRoot, skillsAnchor, prefix);
    }

    internal static string ResolveSkillsPrefixForHost(string layoutRoot, string hostDirName)
    {
        var layout = ResolveLayoutContext(layoutRoot);
        var skillsAnchor = layout.LayoutRoot;

        var prefix =
            FindHostTacosSkillsPrefixForHost(skillsAnchor, hostDirName) ?? DefaultSkillsPrefix;
        return ToSkillsPrefixRelativeToLayout(layout.LayoutRoot, skillsAnchor, prefix);
    }

    static string ToSkillsPrefixRelativeToLayout(
        string layoutRoot,
        string skillsAnchor,
        string prefix
    )
    {
        if (
            Path.GetFullPath(skillsAnchor)
                .Equals(Path.GetFullPath(layoutRoot), StringComparison.OrdinalIgnoreCase)
        )
        {
            return prefix;
        }

        var skillsDir = CombineRepoRelative(skillsAnchor, prefix);
        return Path.GetRelativePath(layoutRoot, skillsDir).Replace('\\', '/');
    }

    static string? ResolveTacosSkillPath(string layoutRoot, string skillName)
    {
        var layout = ResolveLayoutContext(layoutRoot);
        var anchor = ResolveHostSkillsAnchor(layout);
        var prefix = FindHostTacosSkillsPrefix(anchor);
        if (prefix is null)
        {
            return null;
        }

        return CombineRepoRelative(anchor, prefix, skillName, "SKILL.md");
    }

    static string CombineRepoRelative(string repoRoot, string relativePrefix, params string[] rest)
    {
        var parts = new List<string> { repoRoot };
        parts.AddRange(relativePrefix.Split('/', '\\'));
        parts.AddRange(rest);
        return Path.Combine(parts.ToArray());
    }

    static int CountForbiddenHostRootsInSkillBodies(string repoRoot, out string? sample)
    {
        sample = null;
        var prefix = FindHostTacosSkillsPrefix(repoRoot);
        if (prefix is null)
        {
            return 0;
        }

        var skillsRoot = CombineRepoRelative(repoRoot, prefix);
        if (!Directory.Exists(skillsRoot))
        {
            return 0;
        }

        var count = 0;
        foreach (var skillDir in Directory.EnumerateDirectories(skillsRoot, "tacos-*"))
        {
            if (Path.GetFileName(skillDir).Equals("tacos-doctor", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (
                var file in Directory.EnumerateFiles(skillDir, "*", SearchOption.AllDirectories)
            )
            {
                if (
                    file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                    && file.Contains(
                        $"{Path.DirectorySeparatorChar}scripts{Path.DirectorySeparatorChar}",
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
                if (rel.Contains("/templates/host/", StringComparison.Ordinal))
                {
                    continue;
                }

                var text = File.ReadAllText(file, Utf8NoBom);
                foreach (var pattern in ForbiddenHostSkillsRootPatterns)
                {
                    if (text.Contains(pattern, StringComparison.Ordinal))
                    {
                        count++;
                        sample ??= $"{rel} ({pattern})";
                        break;
                    }
                }
            }
        }

        return count;
    }
}
