using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

partial class Program
{
    const string SpecReviewSkillsKey = "spec_review_additional_skills";
    const string ApplyReviewSkillsKey = "apply_review_additional_skills";

    static int RunMergeReviewSkills(
        string targetRepoRoot,
        string targetOpenspecDir,
        bool isDryRun,
        IReadOnlyList<string> specPaths,
        IReadOnlyList<string> applyPaths
    )
    {
        var tacosYamlPath = Path.Combine(targetOpenspecDir, "tacos.yaml");
        if (!File.Exists(tacosYamlPath))
        {
            Console.Error.WriteLine("openspec/tacos.yaml not found — run set-schema first.");
            return 1;
        }

        var yamlText = File.ReadAllText(tacosYamlPath, Utf8NoBom);
        var changed = false;
        var hadSuccess = false;

        var (specValid, specSkipped) = FilterValidReviewSkillPaths(targetRepoRoot, specPaths);
        var (applyValid, applySkipped) = FilterValidReviewSkillPaths(targetRepoRoot, applyPaths);

        foreach (var skipped in specSkipped.Concat(applySkipped).Distinct(StringComparer.Ordinal))
        {
            Console.WriteLine($"WARN review skills: skipped invalid path {skipped}");
        }

        var specState = GetReviewSkillsArrayState(yamlText, SpecReviewSkillsKey);
        if (specPaths.Count > 0)
        {
            if (specValid.Count > 0 && !specState.IsPopulated)
            {
                yamlText = SetReviewSkillsArray(yamlText, SpecReviewSkillsKey, specValid);
                changed = true;
                hadSuccess = true;
                Console.WriteLine(
                    $"OK review skills: populated spec array ({specValid.Count} path(s))"
                );
            }
            else if (specState.IsPopulated)
            {
                Console.WriteLine("WARN review skills: preserved (non-empty spec array unchanged)");
                hadSuccess = true;
            }
        }

        var applyState = GetReviewSkillsArrayState(yamlText, ApplyReviewSkillsKey);
        if (applyPaths.Count > 0)
        {
            if (applyValid.Count > 0 && !applyState.IsPopulated)
            {
                yamlText = SetReviewSkillsArray(yamlText, ApplyReviewSkillsKey, applyValid);
                changed = true;
                hadSuccess = true;
                Console.WriteLine(
                    $"OK review skills: populated apply array ({applyValid.Count} path(s))"
                );
            }
            else if (applyState.IsPopulated)
            {
                Console.WriteLine(
                    "WARN review skills: preserved (non-empty apply array unchanged)"
                );
                hadSuccess = true;
            }
        }

        if (!hadSuccess && specPaths.Count == 0 && applyPaths.Count == 0)
        {
            Console.WriteLine("WARN review skills: no paths provided");
            return 0;
        }

        if (!hadSuccess)
        {
            Console.WriteLine("WARN review skills: no valid paths to merge");
            return 0;
        }

        if (changed && !isDryRun)
        {
            File.WriteAllText(tacosYamlPath, yamlText, Utf8NoBom);
        }
        else if (changed && isDryRun)
        {
            Console.WriteLine("[dry-run] merge-review-skills would update openspec/tacos.yaml");
        }

        return 0;
    }

    static (IReadOnlyList<string> Valid, IReadOnlyList<string> Skipped) FilterValidReviewSkillPaths(
        string targetRepoRoot,
        IReadOnlyList<string> paths
    )
    {
        var valid = new List<string>();
        var skipped = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in paths.Distinct(StringComparer.Ordinal))
        {
            if (TryNormalizeReviewSkillPath(targetRepoRoot, path, out var normalized))
            {
                if (seen.Add(normalized))
                {
                    valid.Add(normalized);
                }
            }
            else
            {
                skipped.Add(path);
            }
        }

        return (valid, skipped);
    }

    static bool TryNormalizeReviewSkillPath(
        string targetRepoRoot,
        string path,
        out string normalized
    )
    {
        normalized = path.Trim().Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        normalized = normalized.TrimEnd('/');
        if (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        if (normalized.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(normalized))
        {
            return false;
        }

        if (normalized.EndsWith("/SKILL.md", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^"/SKILL.md".Length];
        }
        else if (normalized.EndsWith("SKILL.md", StringComparison.OrdinalIgnoreCase))
        {
            normalized = Path.GetDirectoryName(normalized)?.Replace('\\', '/') ?? "";
        }

        var skillMd = Path.Combine(
            targetRepoRoot,
            normalized.Replace('/', Path.DirectorySeparatorChar),
            "SKILL.md"
        );
        if (!File.Exists(skillMd))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(normalized);
    }

    sealed record ReviewSkillsArrayState(bool KeyPresent, IReadOnlyList<string> Paths)
    {
        public bool IsPopulated => Paths.Count > 0;
    }

    static ReviewSkillsArrayState GetReviewSkillsArrayState(string yamlText, string arrayKey)
    {
        if (!TryGetReviewSkillsArray(yamlText, arrayKey, out var paths))
        {
            return new ReviewSkillsArrayState(false, Array.Empty<string>());
        }

        return new ReviewSkillsArrayState(true, paths);
    }

    static bool TryGetReviewSkillsArray(
        string yamlText,
        string arrayKey,
        out IReadOnlyList<string> paths
    )
    {
        paths = Array.Empty<string>();
        var stream = new YamlStream();
        using var reader = new StringReader(yamlText);
        stream.Load(reader);
        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
        {
            return false;
        }

        if (!root.Children.TryGetValue(new YamlScalarNode("review"), out var reviewNode))
        {
            return false;
        }

        if (reviewNode is not YamlMappingNode reviewMap)
        {
            return false;
        }

        if (!reviewMap.Children.TryGetValue(new YamlScalarNode(arrayKey), out var arrayNode))
        {
            return false;
        }

        if (arrayNode is YamlSequenceNode sequence)
        {
            paths = sequence
                .Children.OfType<YamlScalarNode>()
                .Select(static n => n.Value ?? "")
                .Where(static v => !string.IsNullOrWhiteSpace(v))
                .Select(static v => v.Replace('\\', '/'))
                .ToList();
            return true;
        }

        return false;
    }

    static string SetReviewSkillsArray(
        string yamlText,
        string arrayKey,
        IReadOnlyList<string> paths
    )
    {
        var lines = yamlText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
        var reviewLine = FindReviewMappingLine(lines);
        if (reviewLine < 0)
        {
            var endsWithNewline = yamlText.EndsWith('\n');
            lines.Add("review:");
            lines.AddRange(BuildReviewArrayLines(arrayKey, paths, "  "));
            return JoinYamlLines(lines, endsWithNewline);
        }

        if (Regex.IsMatch(lines[reviewLine], @"^review:\s*\{\}\s*(#.*)?$", RegexOptions.None))
        {
            var commentMatch = Regex.Match(lines[reviewLine], @"(#.*)$", RegexOptions.None);
            lines[reviewLine] =
                "review:" + (commentMatch.Success ? " " + commentMatch.Groups[1].Value : "");
        }

        var keyPattern = new Regex(
            $@"^(?<indent> +){Regex.Escape(arrayKey)}:\s*(?<rest>.*)$",
            RegexOptions.None
        );
        for (var i = reviewLine + 1; i < lines.Count; i++)
        {
            var match = keyPattern.Match(lines[i]);
            if (!match.Success)
            {
                continue;
            }

            var indent = match.Groups["indent"].Value;
            var end = i + 1;
            while (end < lines.Count)
            {
                var next = lines[end];
                if (string.IsNullOrWhiteSpace(next))
                {
                    end++;
                    continue;
                }

                if (next.StartsWith(indent + "- ", StringComparison.Ordinal))
                {
                    end++;
                    continue;
                }

                if (
                    next.Length > indent.Length
                    && !char.IsWhiteSpace(next[indent.Length])
                    && !next.TrimStart().StartsWith('-')
                )
                {
                    break;
                }

                if (next.StartsWith(' ') && next.TrimStart().StartsWith('-'))
                {
                    end++;
                    continue;
                }

                break;
            }

            var replacement = BuildReviewArrayLines(arrayKey, paths, indent);
            lines.RemoveRange(i, end - i);
            lines.InsertRange(i, replacement);
            return JoinYamlLines(lines, yamlText.EndsWith('\n'));
        }

        var insertAt = reviewLine + 1;
        while (
            insertAt < lines.Count
            && (string.IsNullOrWhiteSpace(lines[insertAt]) || lines[insertAt].StartsWith(' '))
        )
        {
            insertAt++;
        }

        lines.InsertRange(insertAt, BuildReviewArrayLines(arrayKey, paths, "  "));
        return JoinYamlLines(lines, yamlText.EndsWith('\n'));
    }

    static int FindReviewMappingLine(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (Regex.IsMatch(lines[i], @"^review:\s*(\{\})?\s*(#.*)?$", RegexOptions.None))
            {
                return i;
            }
        }

        return -1;
    }

    static string JoinYamlLines(IReadOnlyList<string> lines, bool endsWithNewline)
    {
        var normalized = lines.ToList();
        if (endsWithNewline && normalized.Count > 0 && normalized[^1] == "")
        {
            normalized.RemoveAt(normalized.Count - 1);
        }

        var joined = string.Join("\n", normalized);
        return endsWithNewline ? joined + "\n" : joined;
    }

    static string BuildReviewArrayBlock(
        string arrayKey,
        IReadOnlyList<string> paths,
        string indent
    ) => string.Join("\n", BuildReviewArrayLines(arrayKey, paths, indent));

    static List<string> BuildReviewArrayLines(
        string arrayKey,
        IReadOnlyList<string> paths,
        string indent
    )
    {
        var lines = new List<string> { $"{indent}{arrayKey}:" };
        foreach (var path in paths)
        {
            lines.Add($"{indent}  - {path}");
        }

        if (paths.Count == 0)
        {
            lines[^1] = $"{indent}{arrayKey}: []";
        }

        return lines;
    }

    static IReadOnlyList<HostAgentInstall.DiagnosticLine> DiagnoseReviewSkillsWiring(
        string repoRoot,
        string openspecDir
    )
    {
        var lines = new List<HostAgentInstall.DiagnosticLine>();
        var tacosYamlPath = Path.Combine(openspecDir, "tacos.yaml");
        var tacosYamlLabel = Path.GetRelativePath(repoRoot, tacosYamlPath).Replace('\\', '/');
        if (!File.Exists(tacosYamlPath))
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    true,
                    $"review skills: {tacosYamlLabel} missing"
                )
            );
            return lines;
        }

        var yamlText = File.ReadAllText(tacosYamlPath, Utf8NoBom);
        var specState = GetReviewSkillsArrayState(yamlText, SpecReviewSkillsKey);
        var applyState = GetReviewSkillsArrayState(yamlText, ApplyReviewSkillsKey);

        if (specState.IsPopulated && applyState.IsPopulated)
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    false,
                    "review skills: both arrays populated"
                )
            );
        }
        else if (specState.IsPopulated)
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    false,
                    "review skills: spec array populated"
                )
            );
        }
        else if (applyState.IsPopulated)
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    false,
                    "review skills: apply array populated"
                )
            );
        }
        else
        {
            lines.Add(
                new HostAgentInstall.DiagnosticLine(
                    false,
                    false,
                    "review skills: empty — run install agent review-skills discovery or /tacos-host-skill"
                )
            );
        }

        return lines;
    }
}
