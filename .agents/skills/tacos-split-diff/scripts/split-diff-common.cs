using System.Text;

sealed class SplitDiffException(string message) : Exception(message);

record AnalyzeOutput(List<AnalyzeFile> files);

record AnalyzeFile(string path, string status, List<AnalyzeHunk> hunks);

record AnalyzeHunk(int index, string header);

static class SplitDiffEncoding
{
    public static UTF8Encoding Utf8NoBom { get; } = new(encoderShouldEmitUTF8Identifier: false);
}

static class SplitDiffPaths
{
    public static string NormalizeRepoPath(string path) => path.Replace('\\', '/');

    public static string ReadDiffText(string? diffFilePath)
    {
        if (diffFilePath is not null)
        {
            if (!File.Exists(diffFilePath))
            {
                throw new SplitDiffException($"diff file not found: {diffFilePath}");
            }

            return File.ReadAllText(diffFilePath, SplitDiffEncoding.Utf8NoBom);
        }

        using var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin, SplitDiffEncoding.Utf8NoBom);
        return reader.ReadToEnd();
    }

    public static List<string> ReadLinesLikePython(string text)
    {
        if (text.Length == 0)
        {
            return [];
        }

        var lines = new List<string>();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is not '\n' and not '\r')
            {
                continue;
            }

            if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                i++;
            }

            lines.Add(text[start..i] + "\n");
            start = i + 1;
        }

        if (start < text.Length)
        {
            lines.Add(text[start..]);
        }

        return lines;
    }

    public static void WriteOutput(string content, string? outputPath)
    {
        if (outputPath is not null)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(outputPath, content, SplitDiffEncoding.Utf8NoBom);
            return;
        }

        Console.Write(content);
    }
}
