partial class Program
{
    internal const string DefaultDistributionSource = "servicetitan/tacos";

    internal readonly record struct SkillsRefreshStep(
        string Channel,
        string SkillsPrefix,
        string[] NpxArgs,
        string? ManualHint
    );

    internal static IReadOnlyList<SkillsRefreshStep> BuildSkillsRefreshPlans(
        string repoRoot,
        string distributionSource,
        PackageManagerKind packageManager
    )
    {
        var source = string.IsNullOrWhiteSpace(distributionSource)
            ? DefaultDistributionSource
            : distributionSource.Trim();
        var prefixes = EnumerateHostTacosSkillsPrefixes(repoRoot);
        var steps = new List<SkillsRefreshStep>();

        foreach (var prefix in prefixes)
        {
            if (!TryMapSkillsPrefixToSkillsAgent(prefix, out var agent))
            {
                steps.Add(
                    new SkillsRefreshStep(
                        "manual",
                        prefix,
                        [],
                        SkillsInstallManualHint(packageManager, prefix)
                    )
                );
                continue;
            }

            steps.Add(
                new SkillsRefreshStep(
                    packageManager == PackageManagerKind.Pnpm ? "pnpm-dlx-skills" : "npx-skills",
                    prefix,
                    ["skills", "add", source, "--agent", agent, "-y"],
                    null
                )
            );
        }

        return steps;
    }

    static bool TryMapSkillsPrefixToSkillsAgent(string prefix, out string agent)
    {
        if (prefix.Equals(".agents/skills", StringComparison.Ordinal))
        {
            agent = "cursor";
            return true;
        }

        if (prefix.Equals(".cursor/skills", StringComparison.Ordinal))
        {
            agent = "cursor";
            return true;
        }

        if (prefix.Equals(".claude/skills", StringComparison.Ordinal))
        {
            agent = "claude-code";
            return true;
        }

        const string skillsSuffix = "/skills";
        if (prefix.StartsWith('.') && prefix.EndsWith(skillsSuffix, StringComparison.Ordinal))
        {
            var slash = prefix.IndexOf('/', StringComparison.Ordinal);
            if (slash > 1)
            {
                agent = prefix[1..slash];
                return true;
            }
        }

        agent = "";
        return false;
    }

    static int RunSkillRefresh(string distributionSource, LayoutContext layout)
    {
        var packageManager = ResolvePackageManager(layout);
        var nested = EnumerateNestedAccidentalSkillsInstalls(ResolveSkillsSearchRoot(layout));
        if (nested.Count > 0)
        {
            Console.Error.WriteLine("FAIL accidental nested skills install(s) detected:");
            foreach (var path in nested)
            {
                Console.Error.WriteLine($"  {path}/");
            }

            Console.Error.WriteLine(
                "Delete each nested tree above (including skills-lock.json there) before refresh."
            );
            Console.Error.WriteLine(SkillsInstallCwdReminder(packageManager));
            return 1;
        }

        var prefixes = EnumerateHostTacosSkillsPrefixes(ResolveSkillsSearchRoot(layout));
        if (prefixes.Count == 0)
        {
            Console.WriteLine(
                "No tacos skills roots found (expected tacos-orchestration/SKILL.md under a host skills path)."
            );
            Console.WriteLine("Install skills first, then run update.");
            return 1;
        }

        var plans = BuildSkillsRefreshPlans(
            ResolveSkillsSearchRoot(layout),
            distributionSource,
            packageManager
        );
        var projectRoot = ResolveSkillRefreshCwd(layout).Replace('\\', '/');
        var execPrefix = SkillsExecPrefix(packageManager);
        Console.WriteLine("--- Skills refresh plan ---");
        Console.WriteLine($"OpenSpec project root: {projectRoot}");
        Console.WriteLine($"Distribution source: {distributionSource.Trim()}");
        Console.WriteLine($"Package manager: {PackageManagerLabel(packageManager)}");
        Console.WriteLine(
            $"Detected install targets ({prefixes.Count}): {string.Join(", ", prefixes)}"
        );
        Console.WriteLine(SkillsRefreshRunReminder(packageManager));
        Console.WriteLine();

        foreach (var step in plans)
        {
            if (step.Channel == "manual")
            {
                Console.WriteLine($"MANUAL  updates {step.SkillsPrefix}/");
                Console.WriteLine($"        cwd={projectRoot}");
                Console.WriteLine($"        {step.ManualHint}");
                continue;
            }

            var cmd = execPrefix + " " + string.Join(' ', step.NpxArgs);
            Console.WriteLine($"RUN     cwd={projectRoot}");
            Console.WriteLine($"        {cmd}  # updates {step.SkillsPrefix}/");
        }

        return 0;
    }

    internal static string ResolveSkillsSearchRoot(LayoutContext layout) => layout.LayoutRoot;

    internal static string ResolveSkillRefreshCwd(LayoutContext layout) => layout.LayoutRoot;
}
