using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    const string AtlassianMcpServerFolder = "plugin-atlassian-atlassian";

    static readonly string[] AtlassianMcpConfigRelativePaths = [".cursor/mcp.json", ".mcp.json"];

    internal static bool TryDetectAtlassianMcp(string? repoRoot, out string detail)
    {
        detail = "";
        if (TryDetectCursorProjectAtlassianMcp(repoRoot, out detail))
        {
            return true;
        }

        if (TryDetectAtlassianMcpConfig(repoRoot, out detail))
        {
            return true;
        }

        if (TryDetectInstalledAtlassianCursorPlugin(out detail))
        {
            return true;
        }

        return false;
    }

    static string CursorHomeDirectory()
    {
        var fromEnv = Environment.GetEnvironmentVariable("TACOS_CURSOR_HOME");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.Trim();
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cursor"
        );
    }

    static bool TryDetectCursorProjectAtlassianMcp(string? repoRoot, out string detail)
    {
        detail = "";
        if (repoRoot is null)
        {
            return false;
        }

        var projectsDir = Path.Combine(CursorHomeDirectory(), "projects");
        if (!Directory.Exists(projectsDir))
        {
            return false;
        }

        var slug = ToCursorProjectSlug(repoRoot);
        if (TryGetAtlassianMcpServerDir(projectsDir, slug, out _))
        {
            detail = "Atlassian MCP (plugin-atlassian-atlassian)";
            return true;
        }

        foreach (var projectDir in Directory.EnumerateDirectories(projectsDir))
        {
            var name = Path.GetFileName(projectDir);
            if (!string.Equals(name, slug, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsAtlassianMcpServerDir(Path.Combine(projectDir, "mcps", AtlassianMcpServerFolder)))
            {
                detail = "Atlassian MCP (plugin-atlassian-atlassian)";
                return true;
            }
        }

        return false;
    }

    static bool TryGetAtlassianMcpServerDir(string projectsDir, string slug, out string serverDir)
    {
        serverDir = Path.Combine(projectsDir, slug, "mcps", AtlassianMcpServerFolder);
        return IsAtlassianMcpServerDir(serverDir);
    }

    static bool TryDetectInstalledAtlassianCursorPlugin(out string detail)
    {
        detail = "";
        var pluginsRoot = Path.Combine(
            CursorHomeDirectory(),
            "plugins",
            "cache",
            "cursor-public",
            "atlassian"
        );
        if (!Directory.Exists(pluginsRoot))
        {
            return false;
        }

        foreach (var versionDir in Directory.EnumerateDirectories(pluginsRoot))
        {
            var mcpJson = Path.Combine(versionDir, ".mcp.json");
            if (!File.Exists(mcpJson))
            {
                continue;
            }

            var text = File.ReadAllText(mcpJson, Utf8NoBom);
            if (!McpConfigDeclaresAtlassian(text))
            {
                continue;
            }

            detail = "Atlassian MCP (Cursor Atlassian plugin)";
            return true;
        }

        return false;
    }

    static string ToCursorProjectSlug(string path)
    {
        var full = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Regex.Replace(full, @"[:\\/]+", "-");
    }

    static bool IsAtlassianMcpServerDir(string serverDir)
    {
        if (!Directory.Exists(serverDir))
        {
            return false;
        }

        var metaPath = Path.Combine(serverDir, "SERVER_METADATA.json");
        if (File.Exists(metaPath))
        {
            var text = File.ReadAllText(metaPath, Utf8NoBom);
            if (text.Contains("atlassian", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return File.Exists(Path.Combine(serverDir, "tools", "getJiraIssue.json"));
    }

    static bool TryDetectAtlassianMcpConfig(string? repoRoot, out string detail)
    {
        detail = "";
        var configRoots = new List<string>();
        if (repoRoot is not null)
        {
            configRoots.Add(repoRoot);
        }

        configRoots.Add(CursorHomeDirectory());

        var appDataCursorUser = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Cursor",
            "User"
        );
        if (Directory.Exists(appDataCursorUser))
        {
            configRoots.Add(appDataCursorUser);
        }

        foreach (var root in configRoots)
        {
            foreach (var rel in AtlassianMcpConfigRelativePaths)
            {
                var path = Path.Combine(root, rel);
                if (!File.Exists(path))
                {
                    continue;
                }

                var text = File.ReadAllText(path, Utf8NoBom);
                if (!McpConfigDeclaresAtlassian(text))
                {
                    continue;
                }

                detail =
                    repoRoot is not null
                    && path.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase)
                        ? $"Atlassian MCP ({rel.Replace('\\', '/')})"
                        : "Atlassian MCP (host mcp.json)";
                return true;
            }
        }

        return false;
    }

    static bool McpConfigDeclaresAtlassian(string text) =>
        text.Contains("mcp.atlassian.com", StringComparison.OrdinalIgnoreCase)
        || (
            text.Contains("atlassian", StringComparison.OrdinalIgnoreCase)
            && text.Contains("mcpServers", StringComparison.OrdinalIgnoreCase)
        );
}
