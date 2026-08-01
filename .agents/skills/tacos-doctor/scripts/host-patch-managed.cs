using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    static int FindManagedEndMarkerIndex(string content, int beginIdx, string endMarker)
    {
        var searchFrom = beginIdx;
        while (searchFrom < content.Length)
        {
            var endIdx = content.IndexOf(endMarker, searchFrom, StringComparison.Ordinal);
            if (endIdx < 0)
            {
                return -1;
            }

            var afterMarker = endIdx + endMarker.Length;
            if (
                afterMarker < content.Length
                && content[afterMarker] is not ('\r' or '\n' or ' ' or '\t')
            )
            {
                searchFrom = endIdx + 1;
                continue;
            }

            return endIdx;
        }

        return -1;
    }

    static bool IsManagedEndMarkerLine(string line, string endMarker) =>
        Regex.IsMatch(
            line.TrimEnd('\r'),
            @"^\s*" + Regex.Escape(endMarker) + @"\s*$",
            RegexOptions.CultureInvariant
        );

    static string NormalizeManagedBlock(string block)
    {
        var lines = block.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
        return string.Join("\n", lines);
    }

    static (int Start, int EndExclusive)? TryGetManagedBlockRange(string content)
    {
        var beginIdx = content.IndexOf(TacosBeginMarker, StringComparison.Ordinal);
        if (beginIdx < 0)
        {
            return null;
        }

        var lineStart = content.LastIndexOf('\n', beginIdx);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;

        var endIdx = FindManagedEndMarkerIndex(content, beginIdx, TacosEndMarker);
        if (endIdx < 0)
        {
            return null;
        }

        var endLineEnd = content.IndexOf('\n', endIdx);
        var endExclusive = endLineEnd >= 0 ? endLineEnd + 1 : content.Length;
        return (lineStart, endExclusive);
    }

    static string DetectContentIndent(string content, int blockStart)
    {
        var lineEnd = content.IndexOf('\n', blockStart);
        if (lineEnd < 0)
        {
            lineEnd = content.Length;
        }

        var firstLine = content[blockStart..lineEnd];
        var leading = firstLine.TakeWhile(c => c is ' ' or '\t').Count();
        return leading > 0 ? firstLine[..leading] : "  ";
    }

    static string IndentHookLines(string contentIndent, string hookText)
    {
        var lines = hookText.TrimEnd('\n').Split('\n');
        return string.Join(
            "\n",
            lines.Select(line =>
            {
                var body = line.TrimStart();
                return string.IsNullOrEmpty(body) ? "" : contentIndent + body;
            })
        );
    }

    static string EnsureTrailingNewline(string text) =>
        text.Length == 0 || text.EndsWith('\n') ? text : text + "\n";

    static string SpliceManagedBlock(string prefix, string block, string suffix) =>
        prefix + EnsureTrailingNewline(block) + suffix;

    static (string Content, bool Applied) RepairCorruptedManagedEndMarker(
        string content,
        string endMarker
    )
    {
        var lines = content.Replace("\r\n", "\n").Split('\n').ToList();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var idx = line.IndexOf(endMarker, StringComparison.Ordinal);
            if (idx < 0)
            {
                continue;
            }

            var after = idx + endMarker.Length;
            if (after >= line.Length || line[after] is '\r' or '\n' or ' ' or '\t')
            {
                continue;
            }

            lines[i] = line[..after];
            lines.Insert(i + 1, line[after..]);
            return (string.Join("\n", lines), true);
        }

        return (content, false);
    }

    static bool ManagedBlocksMatch(string existing, string canonical) =>
        string.Equals(
            NormalizeManagedBlock(existing),
            NormalizeManagedBlock(canonical),
            StringComparison.Ordinal
        );

    static HashSet<string> GetManagedBodyLineNorms(
        string block,
        string beginMarker,
        string endMarker
    )
    {
        var beginNorm = NormalizeManagedLine(beginMarker);
        var endNorm = NormalizeManagedLine(endMarker);
        return block
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(NormalizeManagedLine)
            .Where(l => l.Length > 0 && l != beginNorm && l != endNorm)
            .ToHashSet(StringComparer.Ordinal);
    }

    static bool ManagedBlockLineSetsEqual(
        string existing,
        string canonical,
        string beginMarker,
        string endMarker
    ) =>
        GetManagedBodyLineNorms(existing, beginMarker, endMarker)
            .SetEquals(GetManagedBodyLineNorms(canonical, beginMarker, endMarker));

    static List<string> ExtractCustomManagedLines(
        string existingBlock,
        string canonicalBlock,
        string beginMarker,
        string endMarker
    )
    {
        var canonicalNorms = GetManagedBodyLineNorms(canonicalBlock, beginMarker, endMarker);
        var beginNorm = NormalizeManagedLine(beginMarker);
        var endNorm = NormalizeManagedLine(endMarker);
        var custom = new List<string>();
        foreach (var line in existingBlock.Replace("\r\n", "\n").Split('\n'))
        {
            var norm = NormalizeManagedLine(line);
            if (
                norm.Length == 0
                || norm == beginNorm
                || norm == endNorm
                || canonicalNorms.Contains(norm)
            )
            {
                continue;
            }

            if (LooksLikeBundleInstructionLine(norm) || LooksLikeMarkdownTableLine(line))
            {
                continue;
            }

            custom.Add(line);
        }

        return custom;
    }

    static string InsertLinesBeforeManagedEnd(
        string block,
        IReadOnlyList<string> linesToInsert,
        string endMarker
    )
    {
        if (linesToInsert.Count == 0)
        {
            return block;
        }

        var kept = block.Replace("\r\n", "\n").Split('\n').ToList();
        var endIdx = kept.FindIndex(l => IsManagedEndMarkerLine(l, endMarker));
        if (endIdx < 0)
        {
            kept.AddRange(linesToInsert);
        }
        else
        {
            kept.InsertRange(endIdx, linesToInsert);
        }

        var merged = string.Join("\n", kept);
        if (!merged.EndsWith('\n'))
        {
            merged += "\n";
        }

        return merged;
    }

    static (string Block, bool Applied, string Message) TryReorderManagedBlockToCanonical(
        string existingBlock,
        string canonicalBlock,
        string beginMarker,
        string endMarker
    )
    {
        if (ManagedBlocksMatch(existingBlock, canonicalBlock))
        {
            return (existingBlock, false, "");
        }

        if (!ManagedBlockLineSetsEqual(existingBlock, canonicalBlock, beginMarker, endMarker))
        {
            return (existingBlock, false, "");
        }

        var custom = ExtractCustomManagedLines(
            existingBlock,
            canonicalBlock,
            beginMarker,
            endMarker
        );
        var reordered =
            custom.Count == 0
                ? canonicalBlock
                : InsertLinesBeforeManagedEnd(canonicalBlock, custom, endMarker);
        return (reordered, true, "reordered managed block to match bundle");
    }

    static (string Block, bool Applied, string Message) ApplyManagedBlockNonForceRefresh(
        string existingBlock,
        string canonicalBlock,
        string beginMarker,
        string endMarker
    )
    {
        var (reordered, reorderApplied, reorderMessage) = TryReorderManagedBlockToCanonical(
            existingBlock,
            canonicalBlock,
            beginMarker,
            endMarker
        );
        if (reorderApplied)
        {
            return (reordered, true, reorderMessage);
        }

        return MergeManagedBlockSync(existingBlock, canonicalBlock, beginMarker, endMarker);
    }

    static string NormalizeManagedLine(string line) =>
        string.Join(' ', line.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    static bool LooksLikeMarkdownTableLine(string line) => line.TrimStart().StartsWith('|');

    static bool LooksLikeBundleInstructionLine(string normalizedLine) =>
        normalizedLine.Contains("opsx", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("tacos-", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("OpenSpec", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("GRILL", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("POST-ARTIFACT", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("ARTIFACT REMOVAL", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("Artifact removal", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("Gather/summarize", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("BINDING", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("EXPLORE", StringComparison.OrdinalIgnoreCase)
        || normalizedLine.Contains("Command ids", StringComparison.OrdinalIgnoreCase);

    static (string Block, bool Applied, string Message) MergeManagedBlockSync(
        string existingBlock,
        string canonicalBlock,
        string beginMarker,
        string endMarker
    )
    {
        if (ManagedBlocksMatch(existingBlock, canonicalBlock))
        {
            return (existingBlock, false, "managed block up to date");
        }

        var custom = ExtractCustomManagedLines(
            existingBlock,
            canonicalBlock,
            beginMarker,
            endMarker
        );
        var synced =
            custom.Count == 0
                ? canonicalBlock
                : InsertLinesBeforeManagedEnd(canonicalBlock, custom, endMarker);

        if (ManagedBlocksMatch(synced, existingBlock))
        {
            return (existingBlock, false, "managed block up to date");
        }

        if (ManagedBlockLineSetsEqual(existingBlock, canonicalBlock, beginMarker, endMarker))
        {
            return (synced, true, "reordered managed block to match bundle");
        }

        var existingNorms = GetManagedBodyLineNorms(existingBlock, beginMarker, endMarker);
        var canonicalNorms = GetManagedBodyLineNorms(canonicalBlock, beginMarker, endMarker);
        var added = canonicalNorms.Except(existingNorms).Count();
        var removed = existingNorms.Except(canonicalNorms).Count(LooksLikeBundleInstructionLine);

        var parts = new List<string>();
        if (added > 0)
        {
            parts.Add($"added {added}");
        }

        if (removed > 0)
        {
            parts.Add($"removed {removed} obsolete");
        }

        var detail = parts.Count > 0 ? string.Join(", ", parts) : "reordered";
        return (synced, true, $"synced managed block ({detail})");
    }
}
