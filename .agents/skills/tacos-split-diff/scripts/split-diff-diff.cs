using System.Text.RegularExpressions;

sealed class Hunk
{
    public required int Index { get; init; }
    public required int OldStart { get; init; }
    public required int OldCount { get; init; }
    public required int NewStart { get; init; }
    public required int NewCount { get; init; }
    public required string HeaderContext { get; init; }
    public required string HeaderLine { get; init; }
    public required List<string> Lines { get; init; }
}

sealed class FileDiff
{
    public required string OldPath { get; set; }
    public required string NewPath { get; set; }
    public string Path => string.IsNullOrEmpty(NewPath) ? OldPath : NewPath;
    public string ChangeType { get; set; } = "M";
    public bool IsBinary { get; set; }
    public List<Hunk> Hunks { get; } = [];
}

static class DiffParser
{
    static readonly Regex DiffFileRe = new(@"^diff --git a/(.*) b/(.*)", RegexOptions.Compiled);
    static readonly Regex StatusRe = new(@"^(new|deleted) file mode", RegexOptions.Compiled);
    static readonly Regex BinaryRe = new(@"^Binary files", RegexOptions.Compiled);
    static readonly Regex RenameFromRe = new(@"^rename from (.*)", RegexOptions.Compiled);
    static readonly Regex RenameToRe = new(@"^rename to (.*)", RegexOptions.Compiled);
    static readonly Regex HunkHeaderRe = new(
        @"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@(.*)",
        RegexOptions.Compiled
    );

    public static List<FileDiff> Parse(string diffText)
    {
        var files = new List<FileDiff>();
        FileDiff? currentFile = null;
        Hunk? currentHunk = null;
        var hunkIndex = 0;

        foreach (var rawLine in diffText.Split(['\r', '\n'], StringSplitOptions.None))
        {
            var line = rawLine;

            var dm = DiffFileRe.Match(line);
            if (dm.Success)
            {
                if (currentHunk is not null && currentFile is not null)
                {
                    currentFile.Hunks.Add(currentHunk);
                    currentHunk = null;
                }

                currentFile = new FileDiff
                {
                    OldPath = dm.Groups[1].Value,
                    NewPath = dm.Groups[2].Value,
                };
                files.Add(currentFile);
                hunkIndex = 0;
                continue;
            }

            if (currentFile is null)
            {
                continue;
            }

            var sm = StatusRe.Match(line);
            if (sm.Success)
            {
                currentFile.ChangeType = sm.Groups[1].Value == "new" ? "A" : "D";
                continue;
            }

            if (BinaryRe.IsMatch(line))
            {
                currentFile.IsBinary = true;
                continue;
            }

            var rfm = RenameFromRe.Match(line);
            if (rfm.Success)
            {
                currentFile.ChangeType = "R";
                currentFile.OldPath = rfm.Groups[1].Value;
                continue;
            }

            var rtm = RenameToRe.Match(line);
            if (rtm.Success)
            {
                currentFile.NewPath = rtm.Groups[1].Value;
                continue;
            }

            var hm = HunkHeaderRe.Match(line);
            if (hm.Success)
            {
                if (currentHunk is not null)
                {
                    currentFile.Hunks.Add(currentHunk);
                }

                var oldStart = int.Parse(hm.Groups[1].Value);
                var oldCount = hm.Groups[2].Success ? int.Parse(hm.Groups[2].Value) : 1;
                var newStart = int.Parse(hm.Groups[3].Value);
                var newCount = hm.Groups[4].Success ? int.Parse(hm.Groups[4].Value) : 1;

                currentHunk = new Hunk
                {
                    Index = hunkIndex++,
                    OldStart = oldStart,
                    OldCount = oldCount,
                    NewStart = newStart,
                    NewCount = newCount,
                    HeaderContext = hm.Groups[5].Value.Trim(),
                    HeaderLine = line,
                    Lines = [],
                };
                continue;
            }

            if (
                currentHunk is not null
                && (
                    line.StartsWith('+')
                    || line.StartsWith('-')
                    || line.StartsWith(' ')
                    || line == "\\ No newline at end of file"
                )
            )
            {
                currentHunk.Lines.Add(line);
            }
        }

        if (currentHunk is not null && currentFile is not null)
        {
            currentFile.Hunks.Add(currentHunk);
        }

        return files;
    }
}

static class HunkApplier
{
    public static List<string> Apply(List<string> baseLines, List<Hunk> hunks)
    {
        var result = new List<string>();
        var baseIdx = 0;
        var baseLen = baseLines.Count;

        var sortedHunks = hunks.OrderBy(h => h.OldStart).ToList();
        for (var i = 0; i < sortedHunks.Count - 1; i++)
        {
            var curr = sortedHunks[i];
            var nxt = sortedHunks[i + 1];
            var currEnd = curr.OldStart + curr.OldCount;
            if (currEnd > nxt.OldStart)
            {
                throw new SplitDiffException(
                    $"blocks {curr.Index} and {nxt.Index} overlap in the base file "
                        + $"(lines {curr.OldStart}-{currEnd - 1} vs {nxt.OldStart}-"
                        + $"{nxt.OldStart + nxt.OldCount - 1}). "
                        + "Apply non-overlapping blocks only, or use a single block per reconstruct call."
                );
            }
        }

        foreach (var hunk in sortedHunks)
        {
            var hunkStart = hunk.OldStart - 1;

            while (baseIdx < hunkStart && baseIdx < baseLen)
            {
                result.Add(baseLines[baseIdx]);
                baseIdx++;
            }

            var noNewlinePending = false;
            foreach (var line in hunk.Lines)
            {
                if (line == "\\ No newline at end of file")
                {
                    if (result.Count > 0 && result[^1].EndsWith('\n'))
                    {
                        result[^1] = result[^1][..^1];
                    }

                    noNewlinePending = true;
                    continue;
                }

                if (line.StartsWith('-'))
                {
                    if (baseIdx >= baseLen)
                    {
                        throw new SplitDiffException(
                            $"block {hunk.Index} references line {baseIdx + 1} "
                                + $"but base file has only {baseLen} lines"
                        );
                    }

                    var expectedRemoval = baseLines[baseIdx].TrimEnd('\r', '\n');
                    var removed = line[1..];
                    if (!string.Equals(expectedRemoval, removed, StringComparison.Ordinal))
                    {
                        throw new SplitDiffException(
                            $"block {hunk.Index} removal mismatch at line {baseIdx + 1}: "
                                + $"expected `{TruncateForMessage(expectedRemoval)}` but diff removes `{TruncateForMessage(removed)}`"
                        );
                    }

                    baseIdx++;
                }
                else if (line.StartsWith('+'))
                {
                    result.Add(line[1..] + "\n");
                }
                else if (line.StartsWith(' '))
                {
                    if (baseIdx >= baseLen)
                    {
                        throw new SplitDiffException(
                            $"block {hunk.Index} references line {baseIdx + 1} "
                                + $"but base file has only {baseLen} lines"
                        );
                    }

                    var expected = baseLines[baseIdx].TrimEnd('\r', '\n');
                    var actual = line[1..];
                    if (!string.Equals(expected, actual, StringComparison.Ordinal))
                    {
                        throw new SplitDiffException(
                            $"block {hunk.Index} context mismatch at line {baseIdx + 1}: "
                                + $"expected `{TruncateForMessage(expected)}` but diff has `{TruncateForMessage(actual)}`"
                        );
                    }

                    result.Add(line[1..] + "\n");
                    baseIdx++;
                }
                else
                {
                    throw new SplitDiffException(
                        $"unexpected diff line format in block {hunk.Index}: "
                            + $"{(line.Length > 40 ? line[..40] : line)}"
                    );
                }
            }

            if (noNewlinePending && result.Count > 0 && result[^1].EndsWith('\n'))
            {
                result[^1] = result[^1][..^1];
            }
        }

        while (baseIdx < baseLen)
        {
            result.Add(baseLines[baseIdx]);
            baseIdx++;
        }

        return result;
    }

    static string TruncateForMessage(string value) => value.Length > 40 ? value[..40] + "…" : value;
}
