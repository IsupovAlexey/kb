using System.Text.Json;

static class AnalyzeCommands
{
    public static void RunAnalyze(string? diffFilePath)
    {
        var diffText = SplitDiffPaths.ReadDiffText(diffFilePath);
        var files = DiffParser.Parse(diffText);

        var output = new AnalyzeOutput(
            files
                .Select(f => new AnalyzeFile(
                    f.Path,
                    f.ChangeType,
                    f.Hunks.Select(h => new AnalyzeHunk(h.Index, h.HeaderLine)).ToList()
                ))
                .ToList()
        );

        var json = JsonSerializer.Serialize(
            output,
            new JsonSerializerOptions { PropertyNamingPolicy = null, WriteIndented = true }
        );
        Console.Write(json);
        Console.WriteLine();
    }

    public static void RunReconstruct(
        string baseFilePath,
        string diffFilePath,
        string? hunks,
        string? output
    )
    {
        if (!File.Exists(baseFilePath))
        {
            throw new SplitDiffException($"cannot read base file '{baseFilePath}'");
        }

        var rawBase = File.ReadAllText(baseFilePath, SplitDiffEncoding.Utf8NoBom);
        var baseLines = SplitDiffPaths.ReadLinesLikePython(rawBase);

        var diffText = File.ReadAllText(diffFilePath, SplitDiffEncoding.Utf8NoBom);
        var files = DiffParser.Parse(diffText);
        if (files.Count == 0)
        {
            throw new SplitDiffException("no file diffs found in diff file");
        }

        if (files.Count > 1)
        {
            Console.Error.WriteLine(
                $"Warning: diff contains {files.Count} files, using only the first: {files[0].Path}"
            );
        }

        var fileDiff = files[0];
        HashSet<int> selectedIndices;

        if (string.IsNullOrWhiteSpace(hunks))
        {
            selectedIndices = fileDiff.Hunks.Select(h => h.Index).ToHashSet();
        }
        else
        {
            try
            {
                selectedIndices = hunks
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .Select(int.Parse)
                    .ToHashSet();
            }
            catch (FormatException)
            {
                throw new SplitDiffException(
                    $"invalid block indices '{hunks}'. Expected comma-separated integers (0-based)."
                );
            }
        }

        var available = fileDiff.Hunks.Select(h => h.Index).ToHashSet();
        var invalid = selectedIndices.Except(available).OrderBy(x => x).ToList();
        if (invalid.Count > 0)
        {
            throw new SplitDiffException(
                $"block indices [{string.Join(", ", invalid)}] out of range. "
                    + $"Available: 0-{fileDiff.Hunks.Count - 1} ({fileDiff.Hunks.Count} blocks total)."
            );
        }

        var selectedHunks = fileDiff.Hunks.Where(h => selectedIndices.Contains(h.Index)).ToList();
        var result =
            selectedHunks.Count == 0
                ? rawBase
                : string.Concat(HunkApplier.Apply(baseLines, selectedHunks));

        SplitDiffPaths.WriteOutput(result, output);
    }
}
