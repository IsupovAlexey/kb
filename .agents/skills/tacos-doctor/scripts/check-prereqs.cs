#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:property ExperimentalFileBasedProgramEnableTransitiveDirectives=true
#:package YamlDotNet@16.3.0
#:include repo-root.cs
#:include shared-core.cs
#:include yaml-merge.cs
#:include yaml-merge-fragments.cs
#:include repo-bundle.cs
#:include host-layout.cs
#:include repo-skills.cs
#:include repo-mcp.cs
#:include package-manager.cs
#:include process-probe.cs

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

Environment.Exit(await RunCheckPrereqsAsync(args));

async Task<int> RunCheckPrereqsAsync(string[] args)
{
    if (args.Any(static a => a is "--help" or "-h"))
    {
        PrintUsage();
        return 0;
    }

    if (args.Length > 0)
    {
        Console.Error.WriteLine($"Unknown argument: {args[0]}");
        PrintUsage();
        return 1;
    }

    var repoRoot = FindHostRepoRoot(Directory.GetCurrentDirectory());
    var ok = true;
    var bootstrapNeeded = false;
    var initNeeded = false;

    void Check(bool pass, string msg)
    {
        Console.WriteLine(pass ? $"OK   {msg}" : $"FAIL {msg}");
        if (!pass)
        {
            ok = false;
        }
    }

    void Warn(string msg) => Console.WriteLine($"WARN {msg}");
    void Skip(string msg) => Console.WriteLine($"SKIP {msg}");

    Console.WriteLine("--- Host tools ---");

    await CheckToolAsync("dotnet", ["--version"], required: true);
    await CheckOpenspecAsync();
    await CheckToolAsync("git", ["--version"], required: true);
    var ghResult = await CheckToolAsync("gh", ["--version"], required: false);
    CheckGhForPrFeatures(repoRoot, ghResult.Found);

    await CheckJiraAsync(repoRoot);

    Console.WriteLine();
    Console.WriteLine("--- Tacos skills ---");

    if (repoRoot is null)
    {
        Check(
            false,
            "repo root: not found (walk up for openspec/, AGENTS.md, or host skills roots)"
        );
        Check(false, "tacos-orchestration: repo root unknown");
        Check(false, "tacos-grill: repo root unknown");
        Check(false, "tacos-jira-sync: repo root unknown");
    }
    else
    {
        CheckSkill(repoRoot, "tacos-orchestration");
        CheckSkill(repoRoot, "tacos-grill");
        if (IsJiraEnabled(repoRoot))
        {
            CheckSkill(repoRoot, "tacos-jira-sync");
        }
        else
        {
            Skip("tacos-jira-sync: jira.enabled is false");
        }

        foreach (
            var optional in new[]
            {
                "tacos-implementation-conventions",
                "tacos-direct-output",
                "tacos-spec-review",
                "tacos-apply-review",
                "tacos-e2e-scenarios",
                "tacos-test-plans",
                "tacos-split-diff",
                "tacos-slice-pr",
                "tacos-pr",
                "tacos-pr-triage",
                "tacos-project-overview",
                "tacos-handoff",
                "tacos-lens",
                "tacos-assisted-review",
                "tacos-host-skill",
            }
        )
        {
            CheckOptionalSkill(repoRoot, optional);
        }
    }

    return ResolveExitCode();

    int ResolveExitCode()
    {
        if (!ok)
        {
            return 1;
        }

        if (bootstrapNeeded)
        {
            return 2;
        }

        if (initNeeded)
        {
            return 3;
        }

        return 0;
    }

    async Task CheckOpenspecAsync()
    {
        var (found, versionLine, failureDetail) = await TryRunAsync("openspec", ["--version"]);
        if (found)
        {
            var detail = string.IsNullOrWhiteSpace(versionLine) ? "on PATH" : versionLine.Trim();
            Check(true, $"openspec: {detail}");
            if (repoRoot is not null)
            {
                var layout = ResolveLayoutContext(Directory.GetCurrentDirectory());
                if (
                    layout.GitRoot is not null
                    && TryRemoveMistakenGitRootOpenspecProject(
                        layout.GitRoot,
                        out var cleanupMessage
                    )
                )
                {
                    Console.WriteLine($"OK   {cleanupMessage}");
                }

                var projectRoot = ResolveOpenspecProjectRoot(layout);
                var projectLayout = projectRoot is not null
                    ? ResolveLayoutContext(projectRoot)
                    : layout;
                var hostArtifactsRoot = projectRoot;

                if (projectRoot is null)
                {
                    initNeeded = true;
                    var islandCount = layout.GitRoot is not null
                        ? EnumerateTeamIslandLayoutRoots(layout.GitRoot).Count
                        : 0;
                    Console.WriteLine(
                        islandCount == 0
                            ? "NEEDS workspace-init then OpenSpec init at tacos-workspaces/<island>/ — "
                                + "git root is not an OpenSpec project for team island layouts"
                            : "NEEDS OpenSpec init at tacos-workspaces/<island>/ — "
                                + "cd to the island layout root; git root is not an OpenSpec project"
                    );
                }
                else if (!IsOpenspecProjectInitialized(projectRoot))
                {
                    initNeeded = true;
                    Console.WriteLine(
                        "NEEDS init openspec/ project tree (openspec/config.yaml missing) — "
                            + "/tacos-doctor install or update runs ensure-openspec.cs --install"
                    );
                }
                else if (hostArtifactsRoot is not null)
                {
                    var deliveryMode = await TryGetOpenspecGlobalDeliveryAsync(projectRoot);
                    if (!HasCompleteOpenspecHostArtifacts(hostArtifactsRoot, deliveryMode))
                    {
                        Console.WriteLine(
                            "NEEDS sync OpenSpec host artifacts (delivery: "
                                + deliveryMode
                                + "; expected "
                                + DescribeOpenspecDeliveryArtifacts(deliveryMode)
                                + ") — /tacos-doctor install or update runs ensure-openspec.cs --sync-host"
                        );
                    }
                }
            }

            return;
        }

        bootstrapNeeded = true;
        Console.WriteLine(
            $"NEEDS bootstrap openspec CLI: {failureDetail} "
                + "(/tacos-doctor install or update runs ensure-openspec.cs --install)"
        );
        await CheckBootstrapDepsAsync();
    }

    async Task CheckBootstrapDepsAsync()
    {
        var layout = ResolveLayoutContext(Directory.GetCurrentDirectory());
        var packageManager = ResolvePackageManager(layout);
        var packageManagerLabel = PackageManagerLabel(packageManager);

        var (nodeFound, nodeLine, nodeFail) = await TryRunAsync("node", ["--version"]);
        if (nodeFound)
        {
            var detail = string.IsNullOrWhiteSpace(nodeLine) ? "on PATH" : nodeLine.Trim();
            Check(true, $"node: {detail}");
        }
        else
        {
            Check(false, $"node: {nodeFail} (required for OpenSpec bootstrap)");
        }

        var (pmFound, pmLine, pmFail) = await TryRunAsync(packageManagerLabel, ["--version"]);
        if (pmFound)
        {
            var detail = string.IsNullOrWhiteSpace(pmLine) ? "on PATH" : pmLine.Trim();
            Check(true, $"{packageManagerLabel}: {detail}");
        }
        else
        {
            Check(
                false,
                $"{packageManagerLabel}: {pmFail} (required for OpenSpec bootstrap — detected from pnpm-lock.yaml scan)"
            );
        }
    }

    async Task<(bool Found, string VersionLine, string FailureDetail)> CheckToolAsync(
        string command,
        string[] toolArgs,
        bool required
    )
    {
        var (found, versionLine, failureDetail) = await TryRunAsync(command, toolArgs);
        if (found)
        {
            var detail = string.IsNullOrWhiteSpace(versionLine) ? "on PATH" : versionLine.Trim();
            Check(true, $"{command}: {detail}");
            return (true, versionLine, "");
        }

        if (required)
        {
            Check(false, $"{command}: {failureDetail}");
        }
        else
        {
            Warn($"{command}: {failureDetail} (optional until GH integration)");
        }

        return (found, versionLine, failureDetail);
    }

    async Task CheckJiraAsync(string? root)
    {
        if (root is null)
        {
            Skip("jira: repo root unknown");
            return;
        }

        var tacosYamlPath = Path.Combine(root, "openspec", "tacos.yaml");
        if (!File.Exists(tacosYamlPath))
        {
            Skip("jira: not configured (openspec/tacos.yaml missing)");
            return;
        }

        var text = File.ReadAllText(tacosYamlPath, Encoding.UTF8);
        if (!Regex.IsMatch(text, @"^\s*jira\s*:", RegexOptions.Multiline))
        {
            Skip("jira: not configured");
            return;
        }

        var enabledMatch = Regex.Match(
            text,
            @"^\s*jira\s*:\s*(?:\r?\n(?!\S+:)[^\S\r\n]*.*)*?^\s*enabled\s*:\s*(true|yes|1)\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        if (!enabledMatch.Success)
        {
            Skip("jira: disabled");
            return;
        }

        var acli = await TryRunAsync("acli", ["--version"]);
        var atlassian = await TryRunAsync("atlassian", ["--version"]);
        var hasCli = acli.Found || atlassian.Found;
        var hasMcp = TryDetectAtlassianMcp(root, out var mcpDetail);

        if (hasMcp)
        {
            Check(true, $"jira MCP: {mcpDetail}");
        }
        else
        {
            Skip("jira MCP: not detected (enable Atlassian plugin or open agent with MCP once)");
        }

        if (hasCli)
        {
            var which = acli.Found ? "acli" : "atlassian";
            var cliDetail = (acli.Found ? acli.VersionLine : atlassian.VersionLine).Trim();
            Check(
                true,
                $"jira CLI: {which} {(string.IsNullOrWhiteSpace(cliDetail) ? "on PATH" : cliDetail)}"
            );
        }
        else
        {
            Skip("jira CLI: not on PATH");
        }

        if (!hasCli && !hasMcp)
        {
            Warn(
                "jira: enabled but no transport — enable Atlassian MCP (plugin-atlassian-atlassian) or install acli/atlassian CLI on PATH"
            );
        }
    }

    void CheckGhForPrFeatures(string? root, bool ghFound)
    {
        if (root is null || ghFound)
        {
            return;
        }

        var prTriageInstalled =
            ResolveTacosSkillPath(root, "tacos-pr-triage") is { } triagePath
            && File.Exists(triagePath);

        if (
            !IsYamlSectionEnabled(root, "slice_pr")
            && !IsYamlSectionEnabled(root, "pr")
            && !prTriageInstalled
        )
        {
            return;
        }

        var features = new List<string>();
        if (IsYamlSectionEnabled(root, "slice_pr"))
        {
            features.Add("slice_pr");
        }

        if (IsYamlSectionEnabled(root, "pr"))
        {
            features.Add("pr");
        }

        if (prTriageInstalled)
        {
            features.Add("tacos-pr-triage");
        }

        var ghCommands = features
            .Select(feature =>
                feature switch
                {
                    "slice_pr" => "/tacos-slice-pr",
                    "pr" => "/tacos-pr",
                    "tacos-pr-triage" => "/tacos-pr-triage",
                    _ => feature,
                }
            )
            .ToList();
        var featureList = string.Join(" and ", features);
        var requiresGh = features.Count == 1 ? "requires" : "require";
        var ghCommandList = ghCommands.Count switch
        {
            1 => ghCommands[0],
            2 => $"{ghCommands[0]} and {ghCommands[1]}",
            _ => string.Join(", ", ghCommands.Take(ghCommands.Count - 1))
                + $", and {ghCommands[^1]}",
        };
        Warn(
            $"gh: not on PATH — {featureList} {requiresGh} gh; "
                + $"install/authenticate gh for {ghCommandList} GitHub steps"
        );
    }

    void CheckSkill(string root, string skillName)
    {
        var skillPath = ResolveTacosSkillPath(root, skillName);
        if (skillPath is not null && File.Exists(skillPath))
        {
            var rel = Path.GetRelativePath(root, skillPath).Replace('\\', '/');
            Check(true, $"{skillName}: {rel}");
        }
        else
        {
            var expected = $"{ResolveSkillsPrefix(root)}/{skillName}/SKILL.md";
            Check(false, $"{skillName}: not found at {expected}");
        }
    }

    void CheckOptionalSkill(string root, string skillName)
    {
        var skillPath = ResolveTacosSkillPath(root, skillName);
        if (skillPath is not null && File.Exists(skillPath))
        {
            var rel = Path.GetRelativePath(root, skillPath).Replace('\\', '/');
            Check(true, $"{skillName}: {rel}");
        }
        else
        {
            Skip($"{skillName}: not installed");
        }
    }

    bool IsJiraEnabled(string root) => IsYamlSectionEnabled(root, "jira");

    bool IsYamlSectionEnabled(string root, string section)
    {
        var tacosYamlPath = Path.Combine(root, "openspec", "tacos.yaml");
        if (!File.Exists(tacosYamlPath))
        {
            return false;
        }

        var text = File.ReadAllText(tacosYamlPath, Encoding.UTF8);
        if (!Regex.IsMatch(text, $@"^\s*{Regex.Escape(section)}\s*:", RegexOptions.Multiline))
        {
            return false;
        }

        var enabledMatch = Regex.Match(
            text,
            $@"^\s*{Regex.Escape(section)}\s*:\s*(?:\r?\n(?!\S+:)[^\S\r\n]*.*)*?^\s*enabled\s*:\s*(true|yes|1)\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        return enabledMatch.Success;
    }
}

void PrintUsage()
{
    Console.WriteLine(
        """
        check-prereqs.cs — verify host tools and tacos skills

        Usage:
          dotnet scripts/check-prereqs.cs

        Checks (exit codes below):
          Host tools: dotnet, git (required); openspec (exit 2/3 with NEEDS bootstrap/init when only OpenSpec setup is missing and node/package manager are on PATH; exit 1 when node/package manager missing); gh (warn; extra warn when slice_pr/pr enabled)
          Jira: skip until jira.enabled in openspec/tacos.yaml
          Skills: tacos-orchestration, tacos-grill (required); quality-gate and PR skills when present

        Exit codes:
          0  All required checks passed (warnings allowed)
          1  One or more required checks failed (including missing node/package manager when OpenSpec bootstrap is needed)
          2  OpenSpec CLI missing with node/package manager on PATH — bootstrap needed (ensure-openspec.cs --install-cli or --install)
          3  OpenSpec CLI present but openspec/ missing — init needed (ensure-openspec.cs --init or --install)

        Examples:
          dotnet scripts/check-prereqs.cs
        """
    );
}
