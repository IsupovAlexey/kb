using System.Text.RegularExpressions;

sealed record SlicePlanEntry(int Ordinal, IReadOnlyList<string> Files);

static class SlicePlanParser
{
    static readonly Regex PlanFileLineRe = new(@"^\s+-\s+`([^`]+)`", RegexOptions.Compiled);

    static readonly Regex SliceHeaderRe = new(
        @"^###\s+Slice\s+(\d+)\s+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    static readonly Regex BaseBranchFrontmatterRe = new(
        @"^base_branch:\s*(.+)$",
        RegexOptions.Multiline | RegexOptions.Compiled
    );

    public static string? TryGetBaseBranch(string markdown)
    {
        var match = BaseBranchFrontmatterRe.Match(markdown);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups[1].Value.Trim();
        if (value.StartsWith('`') && value.EndsWith('`') && value.Length >= 2)
        {
            value = value[1..^1];
        }

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static IReadOnlyList<SlicePlanEntry> ParseSlices(string markdown)
    {
        var slices = new List<SlicePlanEntry>();
        int? ordinal = null;
        var files = new List<string>();
        var inFilesSection = false;

        void Flush()
        {
            if (ordinal is null)
            {
                return;
            }

            slices.Add(new SlicePlanEntry(ordinal.Value, files.ToList()));
            ordinal = null;
            files.Clear();
            inFilesSection = false;
        }

        foreach (var line in markdown.Split(['\r', '\n'], StringSplitOptions.None))
        {
            var sliceMatch = SliceHeaderRe.Match(line);
            if (sliceMatch.Success)
            {
                Flush();
                ordinal = int.Parse(sliceMatch.Groups[1].Value);
                continue;
            }

            if (ordinal is null)
            {
                continue;
            }

            if (line.Contains("**Files:**", StringComparison.Ordinal))
            {
                inFilesSection = true;
                continue;
            }

            if (inFilesSection)
            {
                if (
                    line.StartsWith("### ", StringComparison.Ordinal)
                    || (
                        line.StartsWith("## ", StringComparison.Ordinal)
                        && !line.StartsWith("## Slices", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    inFilesSection = false;
                }
                else if (line.StartsWith("- **", StringComparison.Ordinal))
                {
                    inFilesSection = false;
                }
            }

            if (inFilesSection)
            {
                var fileMatch = PlanFileLineRe.Match(line);
                if (fileMatch.Success)
                {
                    files.Add(fileMatch.Groups[1].Value);
                }
            }
        }

        Flush();
        return slices;
    }

    public static IEnumerable<string> ParseFilePaths(string markdown)
    {
        var inFilesSection = false;
        foreach (var line in markdown.Split(['\r', '\n'], StringSplitOptions.None))
        {
            if (line.Contains("**Files:**", StringComparison.Ordinal))
            {
                inFilesSection = true;
                continue;
            }

            if (inFilesSection)
            {
                if (
                    line.StartsWith("### ", StringComparison.Ordinal)
                    || (
                        line.StartsWith("## ", StringComparison.Ordinal)
                        && !line.StartsWith("## Slices", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    inFilesSection = false;
                }
                else if (line.StartsWith("- **", StringComparison.Ordinal))
                {
                    inFilesSection = false;
                }
            }

            if (!inFilesSection)
            {
                continue;
            }

            var match = PlanFileLineRe.Match(line);
            if (match.Success)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
