using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    const string ImplementationGatesCommandsPlaceholder =
        "<!-- Repo-specific format, lint, build, and test commands are generated at install or maintained here. -->";

    static readonly Regex ImplementationGatesDiscoveryMetadataRegex = new(
        @"<!--\s*tacos-doctor-discovery:\s*(documented|inferred|empty)\s*-->",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    static IReadOnlyList<HostAgentInstall.DiagnosticLine> DiagnoseImplementationGatesBlock(
        string agentsText
    )
    {
        var lines = new List<HostAgentInstall.DiagnosticLine>();
        if (TryGetImplementationGatesManagedBlockRange(agentsText) is not { } range)
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    true,
                    "AGENTS.md implementation-gates block missing — run /tacos-doctor install or update"
                )
            );
            return lines;
        }

        var inner = agentsText[range.Start..range.EndExclusive];
        var metadata = ImplementationGatesDiscoveryMetadataRegex.Match(inner);
        if (!metadata.Success)
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    true,
                    "implementation-gates: no tacos-doctor-discovery metadata — run install agent discovery"
                )
            );
        }
        else
        {
            switch (metadata.Groups[1].Value.ToLowerInvariant())
            {
                case "documented":
                    lines.Add(
                        new HostAgentInstall.DiagnosticLine(
                            false,
                            false,
                            "implementation-gates: present (documented)"
                        )
                    );
                    break;
                case "inferred":
                    lines.Add(
                        new HostAgentInstall.DiagnosticLine(
                            false,
                            true,
                            "implementation-gates: gates content was inferred — verify in README/CONTRIBUTING"
                        )
                    );
                    break;
                case "empty":
                    lines.Add(
                        new HostAgentInstall.DiagnosticLine(
                            false,
                            false,
                            "implementation-gates: present (no local dev commands — OK for non-code or docs-only hosts)"
                        )
                    );
                    break;
            }
        }

        if (inner.Contains(ImplementationGatesCommandsPlaceholder, StringComparison.Ordinal))
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    true,
                    "implementation-gates: Commands still placeholder — run install agent discovery"
                )
            );
        }

        return lines;
    }

    static bool ImplementationGatesUpdatePreserveWarn(string message) =>
        message.Contains("inner body preserved", StringComparison.Ordinal)
        || message.Contains("block already current", StringComparison.Ordinal)
        || message.Contains("implementation-gates shell present", StringComparison.Ordinal);

    static string? LoadHostImplementationGatesSnippetTemplate(string skillRoot)
    {
        var path = Path.Combine(
            skillRoot,
            "templates",
            "host",
            "AGENTS-implementation-gates-snippet.md"
        );
        if (!File.Exists(path))
        {
            return null;
        }

        return File.ReadAllText(path, Encoding.UTF8).TrimEnd('\r', '\n') + "\n";
    }

    static (int Start, int EndExclusive)? TryGetImplementationGatesManagedBlockRange(string content)
    {
        var beginIdx = content.IndexOf(
            TacosImplementationGatesBeginMarker,
            StringComparison.Ordinal
        );
        if (beginIdx < 0)
        {
            return null;
        }

        var lineStart = content.LastIndexOf('\n', beginIdx);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;

        var endIdx = FindManagedEndMarkerIndex(
            content,
            beginIdx,
            TacosImplementationGatesEndMarker
        );
        if (endIdx < 0)
        {
            return null;
        }

        var endLineEnd = content.IndexOf('\n', endIdx);
        var endExclusive = endLineEnd >= 0 ? endLineEnd + 1 : content.Length;
        return (lineStart, endExclusive);
    }

    static int? FindInsertIndexAfterAgentsManagedBlock(string content)
    {
        if (TryGetAgentsManagedBlockRange(content) is not { } agentsRange)
        {
            return null;
        }

        return agentsRange.EndExclusive;
    }

    static bool IsImplementationGatesBlockAfterAgents(
        string content,
        (int Start, int EndExclusive) gatesRange
    )
    {
        if (TryGetAgentsManagedBlockRange(content) is not { } agentsRange)
        {
            return true;
        }

        if (gatesRange.Start < agentsRange.EndExclusive)
        {
            return false;
        }

        var between = content[agentsRange.EndExclusive..gatesRange.Start];
        return string.IsNullOrWhiteSpace(between);
    }

    static string RemoveContentRange(string content, (int Start, int EndExclusive) range) =>
        content[..range.Start] + content[range.EndExclusive..];

    static (string Content, bool Applied, string Message) EnsureHostImplementationGatesSnippetText(
        string content,
        string canonicalBlock
    )
    {
        if (TryGetImplementationGatesManagedBlockRange(content) is { } gatesRange)
        {
            var existingBlock = content[gatesRange.Start..gatesRange.EndExclusive];
            if (!IsImplementationGatesBlockAfterAgents(content, gatesRange))
            {
                var without = RemoveContentRange(content, gatesRange);
                var relocateAt = FindInsertIndexAfterAgentsManagedBlock(without);
                if (relocateAt is null)
                {
                    return (
                        content,
                        false,
                        "skipped moving implementation-gates block (tacos-agents managed block missing)"
                    );
                }

                var head = without[..relocateAt.Value].TrimEnd('\r', '\n');
                var tail = without[relocateAt.Value..].TrimStart('\r', '\n');
                var relocated =
                    head
                    + "\n\n"
                    + existingBlock.TrimEnd('\r', '\n')
                    + "\n"
                    + (tail.Length > 0 ? tail : "");
                if (!relocated.EndsWith('\n'))
                {
                    relocated += "\n";
                }

                return (
                    relocated,
                    true,
                    "moved AGENTS.md implementation-gates block after tacos-agents block (inner body preserved)"
                );
            }

            if (ManagedBlocksMatch(existingBlock, canonicalBlock))
            {
                if (
                    existingBlock.Contains(
                        ImplementationGatesCommandsPlaceholder,
                        StringComparison.Ordinal
                    )
                )
                {
                    return (
                        content,
                        false,
                        "AGENTS.md implementation-gates shell present — run install agent discovery for ## Commands and tacos-doctor-discovery metadata"
                    );
                }

                return (content, false, "AGENTS.md implementation-gates block already current");
            }

            return (
                content,
                false,
                "AGENTS.md implementation-gates block present (inner body preserved; verify Commands stay current; --force does not refresh gates)"
            );
        }

        var insertAt = FindInsertIndexAfterAgentsManagedBlock(content);
        if (insertAt is null)
        {
            return (
                content,
                false,
                "skipped implementation-gates block (tacos-agents managed block missing)"
            );
        }

        var before = content[..insertAt.Value].TrimEnd('\r', '\n');
        var after = content[insertAt.Value..].TrimStart('\r', '\n');
        var block = canonicalBlock.TrimEnd('\r', '\n');
        var inserted = before + "\n\n" + block + "\n" + (after.Length > 0 ? after : "");
        if (!inserted.EndsWith('\n'))
        {
            inserted += "\n";
        }

        return (
            inserted,
            true,
            "inserted AGENTS.md implementation-gates block after tacos-agents block"
        );
    }

    static (bool Applied, string Message) EnsureHostImplementationGatesSnippet(
        string targetRepoRoot,
        string skillRoot,
        bool dryRun
    )
    {
        var snippet = LoadHostImplementationGatesSnippetTemplate(skillRoot);
        if (snippet is null)
        {
            return (false, "implementation-gates snippet template missing in tacos-doctor bundle");
        }

        var agentsPath = Path.Combine(targetRepoRoot, "AGENTS.md");
        string content;
        if (File.Exists(agentsPath))
        {
            content = File.ReadAllText(agentsPath, Encoding.UTF8);
        }
        else if (dryRun)
        {
            content = """
                # AGENTS

                <!-- tacos-agents-begin -->
                stub
                <!-- tacos-agents-end -->

                """;
        }
        else
        {
            return (false, "AGENTS.md missing; cannot add implementation-gates block");
        }

        var (merged, applied, message) = EnsureHostImplementationGatesSnippetText(content, snippet);
        if (applied && !dryRun)
        {
            File.WriteAllText(agentsPath, merged, Utf8NoBom);
        }

        return (applied, message);
    }
}
