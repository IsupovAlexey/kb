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
#:include package-manager.cs
#:include process-probe.cs

using System.Diagnostics;
using System.Text;
using System.Text.Json;

const string OpenspecPackage = "@fission-ai/openspec@latest";

Environment.Exit(await RunEnsureOpenspecAsync(args));

async Task<int> RunEnsureOpenspecAsync(string[] args)
{
    var installCli = false;
    var initProject = false;
    var syncHost = false;

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--install":
                installCli = true;
                initProject = true;
                syncHost = true;
                break;
            case "--install-cli":
                installCli = true;
                break;
            case "--init":
                initProject = true;
                break;
            case "--sync-host":
                syncHost = true;
                break;
            case "--help" or "-h":
                PrintUsage();
                return 0;
            default:
                Console.Error.WriteLine($"Unknown argument: {args[i]}");
                PrintUsage();
                return 1;
        }
    }

    if (!installCli && !initProject && !syncHost)
    {
        Console.Error.WriteLine(
            "Specify at least one of --install, --install-cli, --init, or --sync-host."
        );
        PrintUsage();
        return 1;
    }

    var cwd = Directory.GetCurrentDirectory();
    var layout = ResolveLayoutContext(cwd);
    if (layout.IsAmbiguous)
    {
        Console.Error.WriteLine($"FAIL layout resolution: {layout.AmbiguityError}");
        return 1;
    }

    if (
        layout.GitRoot is not null
        && TryRemoveMistakenGitRootOpenspecProject(layout.GitRoot, out var cleanupMessage)
    )
    {
        Console.WriteLine($"OK   {cleanupMessage}");
    }

    var projectRoot = ResolveOpenspecProjectRoot(layout);
    if (projectRoot is null)
    {
        if (initProject || syncHost)
        {
            var islandCount = layout.GitRoot is not null
                ? EnumerateTeamIslandLayoutRoots(layout.GitRoot).Count
                : 0;
            if (islandCount == 0)
            {
                Console.Error.WriteLine(
                    "FAIL OpenSpec project init at git root is not used for team island layouts — "
                        + "run workspace-init (/tacos-doctor install workspace) first, then ensure-openspec from tacos-workspaces/<island>/"
                );
            }
            else
            {
                Console.Error.WriteLine(
                    "FAIL OpenSpec project belongs at tacos-workspaces/<island>/ — "
                        + "cd to the island layout root and re-run ensure-openspec.cs"
                );
            }

            return 1;
        }

        return 0;
    }

    var hostAnchor = ResolveHostSkillsAnchor(layout);
    var hostArtifactsRoot = projectRoot;

    if (FindHostRepoRoot(cwd) is null)
    {
        Console.Error.WriteLine(
            "Host repo root not found. Run from a git repo with tacos skills or OpenSpec "
                + "(walk up for openspec/, AGENTS.md, or host skills roots)."
        );
        return 1;
    }

    var openspecDir = Path.Combine(projectRoot, "openspec");
    var packageManager = ResolvePackageManager(layout);
    var packageManagerLabel = PackageManagerLabel(packageManager);
    var openspecProbe = await TryRunAsync("openspec", ["--version"]);
    var cliPresent = openspecProbe.Found;
    var openspecInitialized = IsOpenspecProjectInitialized(projectRoot);
    var deliveryMode = cliPresent
        ? await TryGetOpenspecGlobalDeliveryAsync(projectRoot)
        : OpenspecDeliveryBoth;
    var hostArtifactsPresent = HasCompleteOpenspecHostArtifacts(hostArtifactsRoot, deliveryMode);

    var needsInit = initProject && !openspecInitialized;
    var needsHostSync = syncHost && openspecInitialized && !hostArtifactsPresent;

    if (installCli && cliPresent)
    {
        var versionLine = openspecProbe.VersionLine.Trim();
        Console.WriteLine(
            string.IsNullOrWhiteSpace(versionLine)
                ? $"OK   openspec CLI already on PATH — skipping {packageManagerLabel} global install"
                : $"OK   openspec CLI already on PATH ({versionLine}) — skipping {packageManagerLabel} global install"
        );
    }

    if (initProject && openspecInitialized && hostArtifactsPresent)
    {
        Console.WriteLine(
            $"OK   OpenSpec project and host artifacts present (delivery: {deliveryMode})"
        );
    }
    else if (initProject && openspecInitialized && !hostArtifactsPresent && !syncHost)
    {
        Console.WriteLine(
            "OK   openspec/config.yaml present — run ensure-openspec.cs --sync-host "
                + $"to install OpenSpec host artifacts ({DescribeOpenspecDeliveryArtifacts(deliveryMode)})"
        );
    }
    else if (initProject && !openspecInitialized && Directory.Exists(openspecDir))
    {
        Console.WriteLine(
            "WARN partial openspec/ tree without openspec/config.yaml — running openspec init"
        );
    }

    if ((!installCli || cliPresent) && !needsInit && !needsHostSync)
    {
        return 0;
    }

    if (installCli && !cliPresent)
    {
        var installCommand = FormatGlobalInstallCommand(packageManager, OpenspecPackage);
        Console.WriteLine(
            $"Installing OpenSpec CLI globally ({packageManagerLabel}): {installCommand}"
        );
        var (installExecutable, installArgs) = GetGlobalInstallProcess(
            packageManager,
            OpenspecPackage
        );
        var installCode = await RunProcessAsync(
            installExecutable,
            installArgs,
            projectRoot,
            TimeSpan.FromMinutes(15)
        );
        if (installCode != 0)
        {
            Console.Error.WriteLine(
                $"FAIL {installCommand} exited {installCode}. "
                    + $"Fix {packageManagerLabel} permissions or install OpenSpec manually."
            );
            return installCode;
        }

        openspecProbe = await TryRunAsync("openspec", ["--version"]);
        cliPresent = openspecProbe.Found;
        if (!cliPresent)
        {
            Console.Error.WriteLine(
                $"FAIL openspec CLI still not on PATH after {packageManagerLabel} global install. "
                    + $"Re-open the shell or verify {packageManagerLabel} global bin is on PATH "
                    + "(Windows: openspec.cmd via PATHEXT)."
            );
            return 1;
        }

        var versionLine = openspecProbe.VersionLine.Trim();
        Console.WriteLine(
            string.IsNullOrWhiteSpace(versionLine)
                ? "OK   openspec CLI installed"
                : $"OK   openspec CLI installed ({versionLine})"
        );
    }

    if (needsInit)
    {
        if (!cliPresent)
        {
            Console.Error.WriteLine(
                "FAIL openspec CLI not available — install CLI before init "
                    + "(ensure-openspec.cs --install-cli or --install)."
            );
            return 1;
        }

        var tools = ResolveOpenspecInitTools(hostAnchor);
        if (tools.Length == 0)
        {
            Console.Error.WriteLine(
                "FAIL no host skills roots detected for openspec init --tools. "
                    + $"Install tacos skills first ({SkillsExecPrefix(packageManager)} skills add …)."
            );
            return 1;
        }

        Console.WriteLine($"Running openspec init --tools {tools} --force");
        var initCode = await RunProcessAsync(
            "openspec",
            ["init", "--tools", tools, "--force"],
            projectRoot
        );
        if (initCode != 0)
        {
            Console.WriteLine(
                $"openspec init --force exited {initCode}; retrying without --force for older CLIs"
            );
            initCode = await RunProcessAsync("openspec", ["init", "--tools", tools], projectRoot);
        }

        if (initCode != 0)
        {
            Console.Error.WriteLine($"FAIL openspec init exited {initCode}. See stderr above.");
            return initCode;
        }

        if (!IsOpenspecProjectInitialized(projectRoot))
        {
            Console.Error.WriteLine(
                "FAIL openspec init completed but openspec/config.yaml was not created."
            );
            return 1;
        }

        Console.WriteLine($"OK   openspec init configured project (--tools {tools})");

        hostArtifactsPresent = HasCompleteOpenspecHostArtifacts(hostArtifactsRoot, deliveryMode);
        needsHostSync = syncHost && !hostArtifactsPresent;
    }

    if (needsHostSync)
    {
        if (!cliPresent)
        {
            Console.Error.WriteLine(
                "FAIL openspec CLI not available — install CLI before sync "
                    + "(ensure-openspec.cs --install-cli or --install)."
            );
            return 1;
        }

        if (!IsOpenspecProjectInitialized(projectRoot))
        {
            Console.Error.WriteLine(
                "FAIL openspec/config.yaml missing — run ensure-openspec.cs --init or --install first."
            );
            return 1;
        }

        var ensureWorkflowCode = await EnsureUpdateWorkflowInGlobalConfigAsync(projectRoot);
        if (ensureWorkflowCode != 0)
        {
            return ensureWorkflowCode;
        }

        Console.WriteLine(
            $"Running openspec update --force (delivery: {deliveryMode}; missing "
                + $"{DescribeOpenspecDeliveryArtifacts(deliveryMode)})"
        );
        var updateCode = await RunProcessAsync("openspec", ["update", "--force"], projectRoot);
        if (updateCode != 0)
        {
            Console.WriteLine(
                $"openspec update --force exited {updateCode}; retrying without --force"
            );
            updateCode = await RunProcessAsync("openspec", ["update"], projectRoot);
        }

        if (updateCode != 0)
        {
            Console.Error.WriteLine($"FAIL openspec update exited {updateCode}. See stderr above.");
            return updateCode;
        }

        if (!HasCompleteOpenspecHostArtifacts(hostArtifactsRoot, deliveryMode))
        {
            Console.Error.WriteLine(
                "FAIL openspec update completed but required host artifacts are incomplete "
                    + $"(delivery: {deliveryMode}; expected {DescribeOpenspecDeliveryArtifacts(deliveryMode)})."
            );
            return 1;
        }

        Console.WriteLine(
            $"OK   openspec update installed host artifacts (delivery: {deliveryMode})"
        );
    }

    return 0;
}

static async Task<int> EnsureUpdateWorkflowInGlobalConfigAsync(string projectRoot)
{
    var coreWorkflows = new HashSet<string>(
        ["propose", "explore", "apply", "sync", "archive"],
        StringComparer.Ordinal
    );

    var executable = ResolveExecutable("openspec");
    if (executable is null)
    {
        Console.Error.WriteLine("FAIL openspec not found on PATH — cannot ensure update workflow");
        return 1;
    }

    var (getCode, stdout, _) = await RunProcessCaptureAsync(
        executable,
        ["config", "get", "workflows"],
        projectRoot
    );
    if (getCode != 0)
    {
        Console.Error.WriteLine($"FAIL openspec config get workflows exited {getCode}");
        return getCode;
    }

    List<string> workflows;
    try
    {
        workflows = JsonSerializer.Deserialize<List<string>>(stdout.Trim()) ?? [];
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"FAIL could not parse openspec config workflows: {ex.Message}");
        return 1;
    }

    var needsUpdate = !workflows.Contains("update", StringComparer.Ordinal);
    if (needsUpdate)
    {
        workflows.Add("update");
        workflows.Sort(StringComparer.Ordinal);
    }

    var beyondCore = workflows.Any(w => !coreWorkflows.Contains(w));
    var (profileGetCode, profileStdout, _) = await RunProcessCaptureAsync(
        executable,
        ["config", "get", "profile"],
        projectRoot
    );
    var currentProfile = profileGetCode == 0 ? profileStdout.Trim() : "";
    var needsCustomProfile =
        beyondCore && !currentProfile.Equals("custom", StringComparison.OrdinalIgnoreCase);

    if (!needsUpdate && !needsCustomProfile)
    {
        return 0;
    }

    if (needsCustomProfile)
    {
        var profileCode = await RunProcessAsync(
            executable,
            ["config", "set", "profile", "custom"],
            projectRoot
        );
        if (profileCode != 0)
        {
            return profileCode;
        }
    }

    if (needsUpdate)
    {
        var json = JsonSerializer.Serialize(workflows);
        var setCode = await RunProcessAsync(
            executable,
            ["config", "set", "workflows", json],
            projectRoot
        );
        if (setCode != 0)
        {
            return setCode;
        }
    }

    Console.WriteLine("OK   ensured update workflow in OpenSpec global config");
    return 0;
}

static string ResolveOpenspecInitTools(string repoRoot)
{
    var tools = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var prefix in EnumerateHostTacosSkillsPrefixes(repoRoot))
    {
        if (TryMapSkillsPrefixToOpenspecTool(prefix, out var tool))
        {
            tools.Add(tool);
        }
    }

    return string.Join(",", tools.OrderBy(static t => t, StringComparer.Ordinal));
}

static bool TryMapSkillsPrefixToOpenspecTool(string prefix, out string tool)
{
    if (prefix.Equals(".agents/skills", StringComparison.Ordinal))
    {
        tool = "cursor";
        return true;
    }

    if (prefix.Equals(".cursor/skills", StringComparison.Ordinal))
    {
        tool = "cursor";
        return true;
    }

    if (prefix.Equals(".claude/skills", StringComparison.Ordinal))
    {
        tool = "claude";
        return true;
    }

    if (prefix.Equals(".github/skills", StringComparison.Ordinal))
    {
        tool = "github-copilot";
        return true;
    }

    const string skillsSuffix = "/skills";
    if (prefix.StartsWith('.') && prefix.EndsWith(skillsSuffix, StringComparison.Ordinal))
    {
        var slash = prefix.IndexOf('/', StringComparison.Ordinal);
        if (slash > 1)
        {
            tool = prefix[1..slash] switch
            {
                "agent" => "antigravity",
                "amazonq" => "amazon-q",
                "augment" => "auggie",
                "cospec" => "costrict",
                "forge" => "forgecode",
                "roo" => "roocode",
                _ => prefix[1..slash],
            };
            return IsKnownOpenspecTool(tool);
        }
    }

    tool = "";
    return false;
}

static bool IsKnownOpenspecTool(string tool) =>
    tool
        is "amazon-q"
            or "antigravity"
            or "auggie"
            or "bob"
            or "claude"
            or "cline"
            or "codex"
            or "forgecode"
            or "codebuddy"
            or "continue"
            or "costrict"
            or "crush"
            or "cursor"
            or "factory"
            or "gemini"
            or "github-copilot"
            or "iflow"
            or "junie"
            or "kilocode"
            or "kimi"
            or "kiro"
            or "lingma"
            or "vibe"
            or "opencode"
            or "pi"
            or "qoder"
            or "qwen"
            or "roocode"
            or "trae"
            or "windsurf";

static async Task<int> RunProcessAsync(
    string command,
    string[] commandArgs,
    string workingDirectory,
    TimeSpan? timeout = null
)
{
    var wait = timeout ?? TimeSpan.FromMinutes(5);
    var executable = ResolveExecutable(command);
    if (executable is null)
    {
        Console.Error.WriteLine($"FAIL {command} not found on PATH");
        return 1;
    }

    var (exitCode, stdout, stderr) = await RunProcessCaptureAsync(
        executable,
        commandArgs,
        workingDirectory,
        wait
    );

    if (exitCode == -1)
    {
        var minutes = (int)Math.Ceiling(wait.TotalMinutes);
        Console.Error.WriteLine($"FAIL {command} timed out after {minutes} minutes");
        return 1;
    }

    if (!string.IsNullOrWhiteSpace(stdout))
    {
        Console.Write(stdout);
    }

    if (!string.IsNullOrWhiteSpace(stderr))
    {
        Console.Error.Write(stderr);
    }

    return exitCode;
}

void PrintUsage()
{
    Console.WriteLine(
        """
        ensure-openspec.cs — bootstrap OpenSpec CLI and project tree

        Usage:
          dotnet scripts/ensure-openspec.cs --install
          dotnet scripts/ensure-openspec.cs --install-cli
          dotnet scripts/ensure-openspec.cs --init
          dotnet scripts/ensure-openspec.cs --sync-host

        Flags:
          --install       Install CLI when missing; init when openspec/config.yaml absent;
                          sync host artifacts when incomplete for configured delivery mode
          --install-cli   Global install @fission-ai/openspec@latest when CLI missing (npm or pnpm per host lockfile)
          --init          openspec init --tools <detected-host> when openspec/config.yaml absent
          --sync-host     Ensure update workflow in global config; openspec update when
                          openspec/config.yaml exists but host artifacts are incomplete
                          for the configured OpenSpec delivery mode (commands, skills, or both)

        Exit codes:
          0  Success or nothing to do
          1  Failure (missing repo root, invalid args, timeout, or bootstrap could not complete)
          other  Propagated from package manager or openspec when those tools fail

        Examples:
          dotnet scripts/ensure-openspec.cs --install
          dotnet scripts/ensure-openspec.cs --sync-host
        """
    );
}
