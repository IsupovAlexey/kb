static class VerifyCommands
{
    public static void RunVerifyTip(
        string featureRef,
        string presentationRef,
        string? mergeBaseRef,
        string? slicePlanPath
    )
    {
        GitRunner.EnsureRefExists(featureRef);
        GitRunner.EnsureRefExists(presentationRef);

        var treeDiff = GitRunner.Run("diff", "--no-ext-diff", featureRef, presentationRef);
        if (!string.IsNullOrWhiteSpace(treeDiff))
        {
            Console.Error.WriteLine(
                $"verify-tip: presentation '{presentationRef}' is not identical to feature '{featureRef}'."
            );
            Console.Error.WriteLine("Diff (feature → presentation):");
            Console.Error.WriteLine(treeDiff);
            Environment.Exit(1);
        }

        if (slicePlanPath is null)
        {
            Console.WriteLine($"OK: '{presentationRef}' matches '{featureRef}' (tree identical).");
            return;
        }

        if (!File.Exists(slicePlanPath))
        {
            throw new SplitDiffException($"slice plan not found: {slicePlanPath}");
        }

        var mergeBase =
            mergeBaseRef ?? GitRunner.Run("merge-base", featureRef, presentationRef).Trim();
        if (string.IsNullOrEmpty(mergeBase))
        {
            throw new SplitDiffException(
                $"cannot resolve merge base for {featureRef} and {presentationRef}"
            );
        }

        var featurePaths = GitRunner
            .RunLines("diff", "--name-only", $"{mergeBase}...{featureRef}")
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(SplitDiffPaths.NormalizeRepoPath)
            .ToHashSet(StringComparer.Ordinal);

        var planMarkdown = File.ReadAllText(slicePlanPath, SplitDiffEncoding.Utf8NoBom);
        var planPaths = SlicePlanParser
            .ParseFilePaths(planMarkdown)
            .Select(SplitDiffPaths.NormalizeRepoPath)
            .ToHashSet(StringComparer.Ordinal);

        var missingFromPlan = featurePaths
            .Except(planPaths)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
        var extraInPlan = planPaths
            .Except(featurePaths)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (missingFromPlan.Count > 0 || extraInPlan.Count > 0)
        {
            Console.Error.WriteLine(
                $"verify-tip: slice-plan file list does not match feature diff ({mergeBase}...{featureRef})."
            );
            if (missingFromPlan.Count > 0)
            {
                Console.Error.WriteLine("On feature branch but not in slice-plan:");
                foreach (var path in missingFromPlan)
                {
                    Console.Error.WriteLine($"  {path}");
                }
            }

            if (extraInPlan.Count > 0)
            {
                Console.Error.WriteLine("In slice-plan but not in feature diff:");
                foreach (var path in extraInPlan)
                {
                    Console.Error.WriteLine($"  {path}");
                }
            }

            Environment.Exit(1);
        }

        Console.WriteLine(
            $"OK: '{presentationRef}' matches '{featureRef}' (tree identical; {planPaths.Count} paths match slice-plan)."
        );
    }

    public static void RunVerifySlices(
        string slicePlanPath,
        string presentationRef,
        string? trunkRef
    )
    {
        if (!File.Exists(slicePlanPath))
        {
            throw new SplitDiffException($"slice plan not found: {slicePlanPath}");
        }

        GitRunner.EnsureRefExists(presentationRef);
        var markdown = File.ReadAllText(slicePlanPath, SplitDiffEncoding.Utf8NoBom);
        var slices = SlicePlanParser.ParseSlices(markdown);
        if (slices.Count == 0)
        {
            throw new SplitDiffException(
                "slice-plan contains no slices (### Slice N sections with Files)."
            );
        }

        var trunk = trunkRef ?? SlicePlanParser.TryGetBaseBranch(markdown);
        if (string.IsNullOrWhiteSpace(trunk))
        {
            throw new SplitDiffException(
                "verify-slices requires --trunk-ref or slice-plan frontmatter base_branch."
            );
        }

        GitRunner.EnsureRefExists(trunk);

        var commits = GitRunner
            .RunLines("rev-list", "--reverse", $"{trunk}..{presentationRef}")
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .ToList();

        if (commits.Count != slices.Count)
        {
            Console.Error.WriteLine(
                $"verify-slices: expected {slices.Count} commit(s) on '{presentationRef}' after '{trunk}', found {commits.Count}."
            );
            Environment.Exit(1);
        }

        var cumulativePaths = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < slices.Count; i++)
        {
            var slice = slices[i];
            if (slice.Ordinal != i + 1)
            {
                Console.Error.WriteLine(
                    $"verify-slices: slice at position {i + 1} has ordinal {slice.Ordinal}; expected {i + 1}."
                );
                Environment.Exit(1);
            }

            var commit = commits[i];
            var parentRef = i == 0 ? trunk : commits[i - 1];
            var parentTip = GitRunner.Run("rev-parse", parentRef).Trim();
            var commitParent = GitRunner.Run("rev-parse", $"{commit}^").Trim();

            if (!string.Equals(commitParent, parentTip, StringComparison.Ordinal))
            {
                Console.Error.WriteLine(
                    $"verify-slices: slice {slice.Ordinal} commit ({commit[..Math.Min(12, commit.Length)]}…) "
                        + $"parent != expected '{parentRef}' tip ({parentTip[..Math.Min(12, parentTip.Length)]}…)."
                );
                if (i == 0)
                {
                    Console.Error.WriteLine(
                        "Rebuild presentation branch from trunk after git fetch."
                    );
                }
                else
                {
                    Console.Error.WriteLine(
                        "Presentation branch commits must be linear (one commit per slice)."
                    );
                }

                Environment.Exit(1);
            }

            var incrementalPaths = GitRunner
                .RunLines("diff", "--name-only", $"{parentRef}..{commit}")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(SplitDiffPaths.NormalizeRepoPath)
                .ToHashSet(StringComparer.Ordinal);

            var expectedIncremental = slice
                .Files.Select(SplitDiffPaths.NormalizeRepoPath)
                .ToHashSet(StringComparer.Ordinal);
            ReportPathMismatch(
                $"verify-slices: slice {slice.Ordinal} commit ({parentRef}..{commit})",
                expectedIncremental,
                incrementalPaths
            );

            foreach (var path in expectedIncremental)
            {
                cumulativePaths.Add(path);
            }

            var actualCumulative = GitRunner
                .RunLines("diff", "--name-only", $"{trunk}..{commit}")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(SplitDiffPaths.NormalizeRepoPath)
                .ToHashSet(StringComparer.Ordinal);

            ReportPathMismatch(
                $"verify-slices: cumulative through slice {slice.Ordinal} ({trunk}..{commit})",
                cumulativePaths,
                actualCumulative
            );
        }

        Console.WriteLine(
            $"OK: {slices.Count} slice commit(s) on '{presentationRef}' match slice-plan; first commit parents '{trunk}'."
        );
    }

    static void ReportPathMismatch(string label, HashSet<string> expected, HashSet<string> actual)
    {
        var missing = expected.Except(actual).OrderBy(p => p, StringComparer.Ordinal).ToList();
        var extra = actual.Except(expected).OrderBy(p => p, StringComparer.Ordinal).ToList();

        if (missing.Count == 0 && extra.Count == 0)
        {
            return;
        }

        Console.Error.WriteLine($"{label} does not match plan Files.");
        if (missing.Count > 0)
        {
            Console.Error.WriteLine("Expected in diff but missing:");
            foreach (var path in missing)
            {
                Console.Error.WriteLine($"  {path}");
            }
        }

        if (extra.Count > 0)
        {
            Console.Error.WriteLine("In diff but not in plan:");
            foreach (var path in extra)
            {
                Console.Error.WriteLine($"  {path}");
            }
        }

        Environment.Exit(1);
    }
}
