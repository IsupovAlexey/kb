using System.Text;
using System.Text.RegularExpressions;

internal sealed record ArchiveSessionResult(string TargetRelativePath, string SessionMarkdown);

internal static class TacosWorkSessionArchiver
{
    private const int IntentSummaryThreshold = 600;

    public static ArchiveSessionResult Create(
        string tasksMarkdown,
        string slug,
        DateOnly archiveDate,
        string repoPath
    )
    {
        var document = TacosWorkTasksDocument.Parse(tasksMarkdown);
        var archiveRoot = Path.Combine(repoPath, "openspec", "changes", "archive");
        var targetRelativePath = ResolveTargetRelativePath(slug, archiveDate, archiveRoot);
        var sessionMarkdown = SecretRedaction.RedactSecrets(
            BuildSessionMarkdown(document, slug, archiveDate)
        );
        return new ArchiveSessionResult(targetRelativePath, sessionMarkdown);
    }

    public static string ResolveTargetRelativePath(
        string slug,
        DateOnly archiveDate,
        string archiveRootFullPath
    )
    {
        var datePrefix = archiveDate.ToString("yyyy-MM-dd");
        var baseName = $"{datePrefix}-{slug}";
        var folderName = baseName;
        var candidateDir = Path.Combine(archiveRootFullPath, folderName);
        if (Directory.Exists(candidateDir))
        {
            for (var suffix = 2; suffix < 100; suffix++)
            {
                folderName = $"{baseName}-{suffix}";
                candidateDir = Path.Combine(archiveRootFullPath, folderName);
                if (!Directory.Exists(candidateDir))
                {
                    break;
                }
            }
        }

        if (Directory.Exists(candidateDir))
        {
            throw new InvalidOperationException(
                $"Archive suffix exhausted: folders {baseName} through {baseName}-99 already exist under openspec/changes/archive/."
            );
        }

        return Path.Combine("openspec", "changes", "archive", folderName, "session.md")
            .Replace('\\', '/');
    }

    private static string BuildSessionMarkdown(
        TacosWorkTasksDocument document,
        string slug,
        DateOnly archiveDate
    )
    {
        var builder = new StringBuilder();
        builder.AppendLine("# tacos-work session archive");
        builder.AppendLine();
        builder.AppendLine(
            $"Historical planning record from {archiveDate:yyyy-MM-dd}. Verify against `openspec/specs/**` and the codebase — not current contract."
        );
        builder.AppendLine();
        builder.AppendLine($"- Source: `artifacts/tacos-work/{slug}/tasks.md`");
        builder.AppendLine($"- Archived: {archiveDate:yyyy-MM-dd}");
        builder.AppendLine();
        builder.AppendLine("## Intent");
        builder.AppendLine();
        builder.AppendLine(SelectIntentBody(document).TrimEnd());
        builder.AppendLine();
        builder.AppendLine("## Planning");
        builder.AppendLine();
        AppendPlanningSubsection(builder, "Summary", document.PlanningSummary);
        AppendPlanningSubsection(builder, "Decisions", document.PlanningDecisions);
        AppendPlanningSubsection(builder, "User inputs", document.PlanningUserInputs);
        builder.AppendLine("## Outcome");
        builder.AppendLine();
        builder.AppendLine($"**Testable outcome:** {document.TestableOutcome}");
        builder.AppendLine();
        builder.AppendLine("## Spec touch");
        builder.AppendLine();
        builder.AppendLine(document.SpecTouch.TrimEnd());
        builder.AppendLine();
        builder.AppendLine("## Work completed");
        builder.AppendLine();
        foreach (var row in document.WorkCompletedRows)
        {
            builder.AppendLine($"- {row}");
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void AppendPlanningSubsection(StringBuilder builder, string title, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        builder.AppendLine($"### {title}");
        builder.AppendLine();
        builder.AppendLine(body.TrimEnd());
        builder.AppendLine();
    }

    private static string SelectIntentBody(TacosWorkTasksDocument document)
    {
        if (
            document.Intent.Length > IntentSummaryThreshold
            && !string.IsNullOrWhiteSpace(document.PlanningSummary)
        )
        {
            return document.PlanningSummary;
        }

        return document.Intent;
    }
}

internal sealed class TacosWorkTasksDocument
{
    private static readonly string[] WorkCompletedDenylistPrefixes =
    [
        "plan:",
        "apply review:",
        "human review:",
        "tests:",
        "verify decision",
    ];

    public string Intent { get; init; } = "";

    public string PlanningSummary { get; init; } = "";

    public string PlanningDecisions { get; init; } = "";

    public string PlanningUserInputs { get; init; } = "";

    public string TestableOutcome { get; init; } = "";

    public string SpecTouch { get; init; } = "N/A — not recorded in session checklist";

    public IReadOnlyList<string> WorkCompletedRows { get; init; } = [];

    public static TacosWorkTasksDocument Parse(string markdown)
    {
        var body = StripFrontmatter(markdown);
        var intent = ExtractSection(body, "Intent");
        var planning = ExtractSection(body, "Planning");
        var work = ExtractSection(body, "Work");
        var planningSummary = ExtractSubsection(planning, "Summary");
        var planningDecisions = ExtractSubsection(planning, "Decisions");
        var planningUserInputs = ExtractSubsection(planning, "User inputs");
        var testableOutcome = ExtractTestableOutcome(work);
        var specTouch = ExtractSpecTouch(work, planningUserInputs, planningDecisions);
        var workCompleted = ExtractWorkCompletedRows(work);
        return new TacosWorkTasksDocument
        {
            Intent = CleanBody(intent),
            PlanningSummary = CleanPlanningBody(planningSummary),
            PlanningDecisions = CleanPlanningBody(planningDecisions),
            PlanningUserInputs = CleanPlanningBody(planningUserInputs),
            TestableOutcome = testableOutcome,
            SpecTouch = specTouch,
            WorkCompletedRows = workCompleted,
        };
    }

    private static string StripFrontmatter(string markdown)
    {
        if (!markdown.StartsWith("---", StringComparison.Ordinal))
        {
            return markdown;
        }

        var second = markdown.IndexOf("\n---", StringComparison.Ordinal);
        if (second < 0)
        {
            return markdown;
        }

        var end = markdown.IndexOf('\n', second + 4);
        return end < 0 ? string.Empty : markdown[(end + 1)..];
    }

    private static string ExtractSection(string markdown, string title)
    {
        var pattern = $@"^##\s+{Regex.Escape(title)}\s*$";
        var match = Regex.Match(
            markdown,
            pattern,
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );
        if (!match.Success)
        {
            return string.Empty;
        }

        var start = match.Index + match.Length;
        var next = Regex.Match(markdown[start..], @"^##\s+", RegexOptions.Multiline);
        return next.Success
            ? markdown.Substring(start, next.Index).Trim()
            : markdown[start..].Trim();
    }

    private static string ExtractSubsection(string planningSection, string title)
    {
        if (string.IsNullOrWhiteSpace(planningSection))
        {
            return string.Empty;
        }

        var pattern = $@"^###\s+{Regex.Escape(title)}\s*$";
        var match = Regex.Match(
            planningSection,
            pattern,
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );
        if (!match.Success)
        {
            return string.Empty;
        }

        var start = match.Index + match.Length;
        var next = Regex.Match(planningSection[start..], @"^###\s+", RegexOptions.Multiline);
        return next.Success
            ? planningSection.Substring(start, next.Index).Trim()
            : planningSection[start..].Trim();
    }

    private static string CleanBody(string body) => RemovePendingGrillLines(body).Trim();

    private static string CleanPlanningBody(string body) => RemovePendingGrillLines(body).Trim();

    private static string RemovePendingGrillLines(string body) =>
        string.Join(
            '\n',
            body.Split('\n')
                .Where(line =>
                    !line.Contains("(pending grill)", StringComparison.OrdinalIgnoreCase)
                )
        );

    private static string ExtractTestableOutcome(string workSection)
    {
        var match = Regex.Match(
            workSection,
            @"^\*\*Testable outcome:\*\*\s*(.+)$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static string ExtractSpecTouch(string workSection, string userInputs, string decisions)
    {
        foreach (var line in workSection.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("- [", StringComparison.Ordinal))
            {
                continue;
            }

            var content = CheckboxContent(trimmed);
            if (content.StartsWith("spec touch:", StringComparison.OrdinalIgnoreCase))
            {
                return content["spec touch:".Length..].Trim();
            }
        }

        foreach (var source in new[] { userInputs, decisions })
        {
            foreach (var line in source.Split('\n'))
            {
                var trimmed = line.TrimStart('-', ' ', '\t');
                if (trimmed.StartsWith("spec_touch:", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed["spec_touch:".Length..].Trim();
                }

                if (trimmed.StartsWith("Spec touch:", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed["Spec touch:".Length..].Trim();
                }
            }
        }

        return "N/A — not recorded in session checklist";
    }

    private static List<string> ExtractWorkCompletedRows(string workSection)
    {
        var rows = new List<string>();
        foreach (var line in workSection.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!Regex.IsMatch(trimmed, @"^- \[[xX]\]"))
            {
                continue;
            }

            var content = CheckboxContent(trimmed);
            if (IsDeniedWorkRow(content))
            {
                continue;
            }

            rows.Add(content);
        }

        return rows;
    }

    private static string CheckboxContent(string line)
    {
        var match = Regex.Match(line, @"^- \[[xX ]\]\s*(.*)$");
        return match.Success ? match.Groups[1].Value.Trim() : line.Trim();
    }

    private static bool IsDeniedWorkRow(string content)
    {
        foreach (var prefix in WorkCompletedDenylistPrefixes)
        {
            if (content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
