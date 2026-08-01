using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    static readonly string[] DeprecatedTacosYamlPaths =
    [
        "pr.require_clean_worktree",
        "pr.warn_dirty_worktree",
        "slice_pr.require_clean_worktree",
        "slice_pr.require_clean_checkout",
    ];

    static (bool Applied, string Message) MergeConfigSchema(string configPath, bool dryRun)
    {
        if (!File.Exists(configPath))
        {
            return (false, "config.yaml missing");
        }

        var content = File.ReadAllText(configPath, Encoding.UTF8);
        var (merged, applied, message) = MergeConfigSchemaText(content);
        if (applied && !dryRun)
        {
            File.WriteAllText(configPath, merged, Utf8NoBom);
        }

        return (applied, message);
    }

    static (string Content, bool Applied, string Message) MergeConfigSchemaText(string content)
    {
        var schemaMatch = Regex.Match(content, @"^schema:\s*(\S+)\s*$", RegexOptions.Multiline);
        if (schemaMatch.Success)
        {
            var previous = schemaMatch.Groups[1].Value;
            if (previous is "tacos")
            {
                return (content, false, "schema already tacos");
            }

            if (previous is not "spec-driven")
            {
                return (
                    content,
                    false,
                    $"schema left unchanged ({previous}); edit config manually"
                );
            }

            var updated =
                content[..schemaMatch.Groups[1].Index]
                + "tacos"
                + content[(schemaMatch.Groups[1].Index + schemaMatch.Groups[1].Length)..];
            return (updated, true, $"set schema: tacos (was {previous})");
        }

        var withSchema = "schema: tacos\n" + content.TrimStart('\r', '\n');
        return (withSchema, true, "set schema: tacos (was unset)");
    }

    static void WriteMinimalConfig(string configPath, string repoRoot)
    {
        File.WriteAllText(
            configPath,
            "schema: tacos\n\n"
                + "context: |\n"
                + EnsureTrailingNewline(
                    IndentHookLines("  ", BuildOrchestrationContextHook(repoRoot))
                ),
            Utf8NoBom
        );
    }

    static (string Content, bool Applied, string Message) EnsureOrchestrationContextHookText(
        string content,
        bool forceRefresh,
        string repoRoot
    )
    {
        var (repaired, endRepaired) = RepairCorruptedManagedEndMarker(content, TacosEndMarker);
        if (endRepaired)
        {
            content = repaired;
        }

        var hook = IndentHookLines("  ", BuildOrchestrationContextHook(repoRoot));

        if (TryGetManagedBlockRange(content) is { } range)
        {
            var existingBlock = content[range.Start..range.EndExclusive];
            var contentIndent = DetectContentIndent(content, range.Start);
            var canonicalHook = IndentHookLines(
                contentIndent,
                BuildOrchestrationContextHook(repoRoot)
            );

            if (ManagedBlocksMatch(existingBlock, canonicalHook))
            {
                return (
                    content,
                    endRepaired,
                    endRepaired
                        ? "repaired corrupted tacos-config-end marker line"
                        : "context hook already current"
                );
            }

            if (!forceRefresh)
            {
                var (mergedBlock, hookAppended, appendMessage) = ApplyManagedBlockNonForceRefresh(
                    existingBlock,
                    canonicalHook,
                    TacosBeginMarker,
                    TacosEndMarker
                );
                if (!hookAppended)
                {
                    return (content, false, appendMessage);
                }

                var hookUpdated = SpliceManagedBlock(
                    content[..range.Start],
                    mergedBlock,
                    content[range.EndExclusive..]
                );
                return (
                    FinalizeOrchestrationContextHook(hookUpdated),
                    true,
                    endRepaired
                        ? $"repaired corrupted tacos-config-end marker line; {appendMessage}"
                        : appendMessage
                );
            }

            var forced = SpliceManagedBlock(
                content[..range.Start],
                canonicalHook,
                content[range.EndExclusive..]
            );
            return (
                FinalizeOrchestrationContextHook(forced),
                true,
                endRepaired
                    ? "repaired corrupted tacos-config-end marker line; updated tacos orchestration context hook (--force)"
                    : "updated tacos orchestration context hook (--force)"
            );
        }

        var contextHeader = Regex.Match(
            content,
            @"^(?<indent>\s*)context:\s*\|(?:\d+)?\s*$",
            RegexOptions.Multiline
        );
        if (contextHeader.Success)
        {
            var insertAt = contextHeader.Index + contextHeader.Length;
            while (insertAt < content.Length && content[insertAt] is '\r' or '\n')
            {
                insertAt++;
            }

            var contentIndent = "  ";
            if (insertAt < content.Length)
            {
                var lineStart = insertAt;
                var lineEnd = content.IndexOf('\n', insertAt);
                if (lineEnd < 0)
                {
                    lineEnd = content.Length;
                }

                var firstLine = content[lineStart..lineEnd];
                var leading = firstLine.TakeWhile(c => c is ' ' or '\t').Count();
                if (leading > 0)
                {
                    contentIndent = firstLine[..leading];
                    hook = IndentHookLines(contentIndent, BuildOrchestrationContextHook(repoRoot));
                }
            }

            var updated = content.Insert(insertAt, EnsureTrailingNewline(hook));
            return (
                FinalizeOrchestrationContextHook(updated),
                true,
                "prepended tacos orchestration context hook (custom context preserved)"
            );
        }

        var schemaLine = Regex.Match(content, @"^schema:\s*.+\s*$", RegexOptions.Multiline);
        var block = "\ncontext: |\n" + EnsureTrailingNewline(hook);
        if (schemaLine.Success)
        {
            var insertAt = schemaLine.Index + schemaLine.Length;
            var updated = content.Insert(insertAt, block);
            return (
                FinalizeOrchestrationContextHook(updated),
                true,
                "added context with tacos orchestration hook"
            );
        }

        var appended = content.TrimEnd('\r', '\n') + block;
        return (
            FinalizeOrchestrationContextHook(appended),
            true,
            "added context with tacos orchestration hook"
        );
    }

    static string FinalizeOrchestrationContextHook(string content)
    {
        var (repaired, _) = RepairCorruptedManagedEndMarker(content, TacosEndMarker);
        return repaired;
    }

    static (bool Applied, string Message) SyncTacosYamlVersion(
        string hostPath,
        string targetVersion,
        bool dryRun
    )
    {
        if (!File.Exists(hostPath))
        {
            return (false, "openspec/tacos.yaml missing; cannot sync version");
        }

        var normalized = targetVersion.Trim().Trim('"');
        var hostText = File.ReadAllText(hostPath, Encoding.UTF8);
        var match = Regex.Match(hostText, @"^version:\s*[^\r\n]*", RegexOptions.Multiline);
        if (!match.Success)
        {
            return (false, "openspec/tacos.yaml has no version key");
        }

        var quoted = $"version: \"{normalized}\"";
        if (match.Value == quoted)
        {
            return (false, $"openspec/tacos.yaml version already {normalized}");
        }

        var merged = hostText[..match.Index] + quoted + hostText[(match.Index + match.Length)..];
        if (!dryRun)
        {
            File.WriteAllText(hostPath, merged, Utf8NoBom);
        }

        return (true, $"sync openspec/tacos.yaml version -> {normalized}");
    }

    static (bool Applied, string Message) MergeTacosYamlFile(
        string hostPath,
        string templatePath,
        bool dryRun
    )
    {
        var hostText = File.ReadAllText(hostPath, Encoding.UTF8);
        var templateText = File.ReadAllText(templatePath, Encoding.UTF8);
        var (merged, added, removed) = YamlMergeAddOnly.Merge(
            hostText,
            templateText,
            removePaths: DeprecatedTacosYamlPaths
        );
        if (added.Count == 0 && removed.Count == 0)
        {
            return (false, "openspec/tacos.yaml up to date");
        }

        var parts = new List<string>();
        if (added.Count > 0)
        {
            parts.Add($"+{added.Count} ({string.Join(", ", added)})");
        }

        if (removed.Count > 0)
        {
            parts.Add($"-{removed.Count} ({string.Join(", ", removed)})");
        }

        if (!dryRun)
        {
            File.WriteAllText(hostPath, merged, Utf8NoBom);
        }

        return (true, $"synced openspec/tacos.yaml ({string.Join("; ", parts)})");
    }

    static bool WillModifyConfig(
        string configPath,
        bool configExisted,
        bool setSchemaKey,
        bool forceRefresh,
        string repoRoot
    )
    {
        if (!configExisted)
        {
            return true;
        }

        if (!File.Exists(configPath))
        {
            return false;
        }

        var content = File.ReadAllText(configPath, Encoding.UTF8);
        if (setSchemaKey)
        {
            var (_, schemaApplied, _) = MergeConfigSchemaText(content);
            if (schemaApplied)
            {
                return true;
            }
        }

        var (_, hookApplied, _) = EnsureOrchestrationContextHookText(
            content,
            forceRefresh,
            repoRoot
        );
        return hookApplied;
    }

    static (bool Applied, string Message) EnsureOrchestrationContextHook(
        string configPath,
        bool forceRefresh,
        bool dryRun,
        string repoRoot
    )
    {
        if (!File.Exists(configPath))
        {
            return (false, "config.yaml missing; cannot add context hook");
        }

        var content = File.ReadAllText(configPath, Encoding.UTF8);
        var (merged, applied, message) = EnsureOrchestrationContextHookText(
            content,
            forceRefresh,
            repoRoot
        );
        if (applied && !dryRun)
        {
            File.WriteAllText(configPath, merged, Utf8NoBom);
        }

        return (applied, message);
    }
}
