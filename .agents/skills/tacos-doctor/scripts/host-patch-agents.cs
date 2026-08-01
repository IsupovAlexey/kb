using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    static string? LoadHostAgentsSnippetTemplate(string skillRoot, string targetRepoRoot)
    {
        var path = Path.Combine(skillRoot, "templates", "host", "AGENTS-openspec-snippet.md");
        if (!File.Exists(path))
        {
            return null;
        }

        var text = File.ReadAllText(path, Encoding.UTF8).TrimEnd() + "\n";
        return ApplySkillsPrefixSubstitution(text, ResolveSkillsPrefix(targetRepoRoot));
    }

    static (int Start, int EndExclusive)? TryGetAgentsManagedBlockRange(string content)
    {
        var beginIdx = content.IndexOf(TacosAgentsBeginMarker, StringComparison.Ordinal);
        if (beginIdx < 0)
        {
            return null;
        }

        var lineStart = content.LastIndexOf('\n', beginIdx);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;

        var endIdx = content.IndexOf(TacosAgentsEndMarker, beginIdx, StringComparison.Ordinal);
        if (endIdx < 0)
        {
            return null;
        }

        var endLineEnd = content.IndexOf('\n', endIdx);
        var endExclusive = endLineEnd >= 0 ? endLineEnd + 1 : content.Length;
        return (lineStart, endExclusive);
    }

    static (string Content, bool Applied, string Message) EnsureHostAgentsSnippetText(
        string content,
        string canonicalSnippet,
        bool forceRefresh
    )
    {
        if (TryGetAgentsManagedBlockRange(content) is { } range)
        {
            var relocatedToStart = false;
            var leadingTrimCount = content
                .TakeWhile(static c => c is ' ' or '\t' or '\r' or '\n')
                .Count();
            if (range.Start > leadingTrimCount)
            {
                var block = content[range.Start..range.EndExclusive];
                var remainder = (content[..range.Start] + content[range.EndExclusive..]).TrimStart(
                    '\r',
                    '\n'
                );
                content =
                    block.TrimEnd('\r', '\n') + (remainder.Length > 0 ? "\n\n" + remainder : "");
                if (TryGetAgentsManagedBlockRange(content) is not { } relocated)
                {
                    return (content, false, "AGENTS.md managed block relocation failed");
                }

                range = relocated;
                relocatedToStart = true;
            }

            var existingBlock = content[range.Start..range.EndExclusive];
            if (ManagedBlocksMatch(existingBlock, canonicalSnippet))
            {
                if (relocatedToStart)
                {
                    return (
                        content,
                        true,
                        "moved OpenSpec snippet to start of AGENTS.md (tacos-agents-begin block)"
                    );
                }

                return (content, false, "AGENTS.md OpenSpec snippet already current");
            }

            if (!forceRefresh)
            {
                var (mergedBlock, snippetAppended, appendMessage) =
                    ApplyManagedBlockNonForceRefresh(
                        existingBlock,
                        canonicalSnippet,
                        TacosAgentsBeginMarker,
                        TacosAgentsEndMarker
                    );
                if (!snippetAppended)
                {
                    return (content, false, appendMessage);
                }

                var snippetUpdated = SpliceManagedBlock(
                    content[..range.Start],
                    mergedBlock,
                    content[range.EndExclusive..]
                );
                return (snippetUpdated, true, appendMessage);
            }

            var forced = SpliceManagedBlock(
                content[..range.Start],
                canonicalSnippet,
                content[range.EndExclusive..]
            );
            return (
                forced,
                true,
                "updated AGENTS.md OpenSpec snippet (tacos-agents-begin block, --force)"
            );
        }

        var body = content.TrimStart('\r', '\n');
        if (body.Length == 0)
        {
            return (
                canonicalSnippet,
                true,
                "inserted OpenSpec snippet at start of AGENTS.md (tacos-agents-begin block)"
            );
        }

        var prependedSnippet = canonicalSnippet.TrimEnd('\r', '\n') + "\n\n" + body;
        return (
            prependedSnippet,
            true,
            "prepended OpenSpec snippet to AGENTS.md (tacos-agents-begin block)"
        );
    }

    static (bool Applied, string Message) EnsureHostAgentsSnippet(
        string targetRepoRoot,
        string skillRoot,
        bool forceRefresh,
        bool dryRun
    )
    {
        var snippet = LoadHostAgentsSnippetTemplate(skillRoot, targetRepoRoot);
        if (snippet is null)
        {
            return (false, "AGENTS snippet template missing in tacos-doctor bundle");
        }

        var agentsPath = Path.Combine(targetRepoRoot, "AGENTS.md");
        string content;
        if (File.Exists(agentsPath))
        {
            content = File.ReadAllText(agentsPath, Encoding.UTF8);
        }
        else
        {
            content = "# AGENTS\n\n";
        }

        var (merged, applied, message) = EnsureHostAgentsSnippetText(
            content,
            snippet,
            forceRefresh
        );
        if (applied && !dryRun)
        {
            File.WriteAllText(agentsPath, merged, Utf8NoBom);
        }

        return (applied, message);
    }

    static void PrintProjectOverviewInstallHint(string targetRepoRoot)
    {
        var skillPath = ResolveTacosSkillPath(targetRepoRoot, "tacos-project-overview");
        if (skillPath is null || !File.Exists(skillPath))
        {
            return;
        }

        Console.WriteLine(
            "Project overview: enable project_overview in openspec/tacos.yaml; set prompt_after_sync "
                + "/ prompt_after_archive for post-sync/archive prompts, or run /tacos-project-overview "
                + "with add/update/remove scope (preview + approve)."
        );
    }
}
