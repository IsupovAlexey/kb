#!/usr/bin/env dotnet
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:property ExperimentalFileBasedProgramEnableTransitiveDirectives=true
#:package YamlDotNet@16.3.0
#:include repo-root.cs
#:include shared-core.cs
#:include yaml-merge.cs
#:include yaml-merge-fragments.cs
#:include host-agents.cs
#:include host-agents-diagnose.cs
#:include host-agents-install.cs
#:include host-patch-managed.cs
#:include host-patch.cs
#:include host-patch-agents.cs
#:include host-patch-gates.cs
#:include repo-bundle.cs
#:include host-layout.cs
#:include workspace-install.cs
#:include repo-skills.cs
#:include repo-review-skills.cs
#:include repo-gitignore.cs
#:include repo-host-overlays.cs
#:include repo-mcp.cs
#:include package-manager.cs
#:include skill-refresh.cs

using System.Text;
using System.Text.RegularExpressions;

var command = "diagnose";
var repoRoot = Directory.GetCurrentDirectory();
var sourceRoot = "";
var distributionSource = DefaultDistributionSource;
var targetVersionOverride = "";
var dryRun = false;
var force = false;
var setSchema = true;
var sandbox = false;
var mergeReviewSpecPaths = new List<string>();
var mergeReviewApplyPaths = new List<string>();

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "set-schema"
        or "diagnose"
        or "update"
        or "config"
        or "skill-refresh"
        or "merge-review-skills"
        or "workspace-init":
            command = args[i];
            break;
        case "--spec" when i + 1 < args.Length:
            mergeReviewSpecPaths.Add(args[++i]);
            break;
        case "--apply" when i + 1 < args.Length:
            mergeReviewApplyPaths.Add(args[++i]);
            break;
        case "--distribution-source" when i + 1 < args.Length:
            distributionSource = args[++i];
            break;
        case "--target-version" when i + 1 < args.Length:
            targetVersionOverride = args[++i];
            break;
        case "--source" when i + 1 < args.Length:
            sourceRoot = Path.GetFullPath(args[++i]);
            break;
        case "--dry-run":
            dryRun = true;
            break;
        case "--force":
            force = true;
            break;
        case "--no-set-schema":
            setSchema = false;
            break;
        case "--sandbox":
            sandbox = true;
            break;
        case "--help" or "-h":
            PrintUsage();
            Environment.Exit(0);
            break;
        default:
            if (command is "workspace-init" && TryParseWorkspaceInitArgs(args, ref i))
            {
                break;
            }

            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            PrintUsage();
            Environment.Exit(2);
            break;
    }
}

if (
    command != "merge-review-skills"
    && (mergeReviewSpecPaths.Count > 0 || mergeReviewApplyPaths.Count > 0)
)
{
    Console.Error.WriteLine("--spec and --apply are only valid with merge-review-skills");
    PrintUsage();
    Environment.Exit(2);
}

var layoutContext = ResolveLayoutContext(Directory.GetCurrentDirectory());
if (layoutContext.IsAmbiguous && command is not "diagnose" && command is not "workspace-init")
{
    Console.Error.WriteLine($"layout resolution failed: {layoutContext.AmbiguityError}");
    Environment.Exit(1);
}

var resolvedRepo =
    command is "workspace-init" && !string.IsNullOrWhiteSpace(workspaceInitLayoutRoot)
        ? ExpandUserPath(workspaceInitLayoutRoot)
    : command is "workspace-init"
        ? FindGitRepositoryRoot(Directory.GetCurrentDirectory())
            ?? FindTacosRepoRoot(Directory.GetCurrentDirectory())
    : layoutContext.HasWorkspace || Directory.Exists(layoutContext.OpenSpecDir)
        ? layoutContext.LayoutRoot
    : FindTacosRepoRoot(Directory.GetCurrentDirectory())
        ?? FindOpenSpecRepoRoot(Directory.GetCurrentDirectory());
if (resolvedRepo is null)
{
    Console.Error.WriteLine(
        "tacos repo root not found. Run from the tacos repo or a host repo with OpenSpec "
            + "(walk-up from current directory)."
    );
    Environment.Exit(1);
}

repoRoot = resolvedRepo;
var openspecDir = sandbox
    ? Path.Combine(repoRoot, "artifacts", SandboxArtifactsSubdir, "openspec")
    : layoutContext.OpenSpecDir;

if (command == "skill-refresh")
{
    Environment.Exit(RunSkillRefresh(distributionSource, layoutContext));
}

if (command is not "workspace-init" && !sandbox && !Directory.Exists(openspecDir))
{
    Console.Error.WriteLine(
        "openspec/ not found. Run from a repo with OpenSpec initialized "
            + $"or use --sandbox to target artifacts/{SandboxArtifactsSubdir}/openspec/."
    );
    Environment.Exit(1);
}

var bundle = ResolveSchemaBundle(repoRoot, sourceRoot);
if (bundle is null)
{
    Console.Error.WriteLine(
        "Could not locate tacos-doctor/schemas/tacos. "
            + "Pass --source <path-to-tacos-doctor-skill-or-repo>."
    );
    Environment.Exit(1);
}

if (sandbox)
{
    EnsureSandboxOpenspec(openspecDir, repoRoot, dryRun);
    Console.WriteLine(
        "Sandbox: using "
            + Path.GetRelativePath(repoRoot, openspecDir).Replace('\\', '/')
            + " (repo openspec/ unchanged)"
    );
}

var exitCode = command switch
{
    "set-schema" => RunSetSchema(bundle.Value),
    "update" => RunUpdate(bundle.Value),
    "config" => RunConfig(bundle.Value),
    "diagnose" => RunDiagnose(bundle.Value),
    "workspace-init" => RunWorkspaceInit(bundle.Value, dryRun),
    "merge-review-skills" => RunMergeReviewSkills(
        repoRoot,
        openspecDir,
        dryRun,
        mergeReviewSpecPaths,
        mergeReviewApplyPaths
    ),
    _ => 1,
};
Environment.Exit(exitCode);

string? ResolveTargetVersion(string skillRoot)
{
    if (!string.IsNullOrWhiteSpace(targetVersionOverride))
    {
        return targetVersionOverride.Trim();
    }

    return TryGetBundleVersion(skillRoot, out var version) ? version : null;
}

int RunUpdate((string SkillRoot, string SchemaSource) bundle)
{
    var targetVersion = ResolveTargetVersion(bundle.SkillRoot);
    if (targetVersion is null)
    {
        Console.Error.WriteLine(
            "Could not resolve target version from bundle template. "
                + "Pass --target-version <semver> or fix templates/openspec/tacos.yaml."
        );
        return 1;
    }

    var tacosYamlTemplatePath = Path.Combine(
        bundle.SkillRoot,
        "templates",
        "openspec",
        "tacos.yaml"
    );
    var setExit = RunSetSchema(
        bundle,
        syncHostSubagents: false,
        refreshSchemaFromBundle: true,
        skipTacosYamlSync: true
    );
    if (setExit != 0)
    {
        return setExit;
    }

    var tacosYamlPath = Path.Combine(openspecDir, "tacos.yaml");
    if (!File.Exists(tacosYamlPath))
    {
        Console.WriteLine(
            dryRun
                ? "[dry-run] write openspec/tacos.yaml from tacos-doctor template"
                : "write openspec/tacos.yaml from tacos-doctor template"
        );
        if (!dryRun)
        {
            File.Copy(tacosYamlTemplatePath, tacosYamlPath, overwrite: false);
        }
    }
    else if (force)
    {
        Console.WriteLine(
            dryRun
                ? "[dry-run] overwrite openspec/tacos.yaml from tacos-doctor template (--force)"
                : "overwrite openspec/tacos.yaml from tacos-doctor template (--force)"
        );
        if (!dryRun)
        {
            File.Copy(tacosYamlTemplatePath, tacosYamlPath, overwrite: true);
        }
    }
    else
    {
        var (applied, message) = MergeTacosYamlFile(tacosYamlPath, tacosYamlTemplatePath, dryRun);
        if (applied || dryRun)
        {
            var label = applied ? message : "merge openspec/tacos.yaml from bundle template";
            Console.WriteLine(dryRun ? $"[dry-run] {label}" : label);
        }
    }

    if (File.Exists(tacosYamlPath))
    {
        var (applied, message) = SyncTacosYamlVersion(tacosYamlPath, targetVersion, dryRun);
        if (applied || dryRun)
        {
            Console.WriteLine(dryRun ? $"[dry-run] {message}" : message);
        }

        var layoutCtx = ResolveLayoutContext(repoRoot);
        if (layoutCtx.Mode == LayoutModeKind.Single && !layoutCtx.HasWorkspace)
        {
            var (wsApplied, wsMessage) = MergeSingleDefaultWorkspace(
                tacosYamlPath,
                bundle.SkillRoot,
                dryRun
            );
            if (wsApplied || dryRun)
            {
                Console.WriteLine(dryRun ? $"[dry-run] {wsMessage}" : wsMessage);
            }
        }

        RegenerateEntryArtifactsFromLayout(repoRoot, dryRun);
    }
    else if (dryRun)
    {
        Console.WriteLine(
            $"[dry-run] sync openspec/tacos.yaml version -> {targetVersion.Trim().Trim('"')}"
        );
    }

    return 0;
}

int RunConfig((string SkillRoot, string SchemaSource) bundle)
{
    if (sandbox)
    {
        Console.Error.WriteLine("config: host subagents require a host repo (omit --sandbox).");
        return 1;
    }

    if (!EnsureHostSubagentBundleValid(bundle.SkillRoot))
    {
        return 1;
    }

    var (_, hostAgentsMessage) = HostAgentInstall.InstallHostSubagentTemplates(
        repoRoot,
        ResolveHostSubagentsInstallRoot(repoRoot),
        bundle.SkillRoot,
        dryRun
    );
    Console.WriteLine(dryRun ? $"[dry-run] {hostAgentsMessage}" : hostAgentsMessage);
    if (!dryRun)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Synced from openspec/tacos.yaml *_models. Toolkit upgrade: /tacos-doctor update"
        );
    }

    return 0;
}

static string ResolveHostSubagentsInstallRoot(string layoutRoot) =>
    ResolveHostSubagentsAnchor(ResolveLayoutContext(layoutRoot));

int RunSetSchema(
    (string SkillRoot, string SchemaSource) bundle,
    bool syncHostSubagents = true,
    bool refreshSchemaFromBundle = false,
    bool skipTacosYamlSync = false,
    bool skipConfigBackup = false
)
{
    var actions = new List<string>();
    var skillRoot = bundle.SkillRoot;
    var schemaSource = bundle.SchemaSource;
    var tacosYamlTemplatePath = Path.Combine(skillRoot, "templates", "openspec", "tacos.yaml");

    if (!Directory.Exists(schemaSource))
    {
        Console.Error.WriteLine($"Missing schema: {schemaSource}");
        return 1;
    }

    var configPath = Path.Combine(openspecDir, "config.yaml");
    var tacosYamlPath = Path.Combine(openspecDir, "tacos.yaml");
    var schemaDest = Path.Combine(openspecDir, "schemas", "tacos");
    var configExisted = File.Exists(configPath);
    var configWillChange = WillModifyConfig(configPath, configExisted, setSchema, force, repoRoot);
    string? backupStamp = null;

    void MaybeBackupConfig(string absolutePath, bool needed)
    {
        if (!needed || sandbox)
        {
            return;
        }

        backupStamp ??= CreateTacosBackupStamp();
        var destRel = BackupRepoPath(repoRoot, absolutePath, backupStamp, dryRun);
        if (destRel is null)
        {
            return;
        }

        var sourceRel = Path.GetRelativePath(repoRoot, absolutePath).Replace('\\', '/');
        actions.Add($"backup {sourceRel} -> {destRel}");
    }

    if (configExisted && configWillChange && !skipConfigBackup)
    {
        MaybeBackupConfig(configPath, needed: true);
    }
    else if (configExisted && !configWillChange)
    {
        actions.Add("openspec/config.yaml unchanged (host customizations preserved)");
    }

    if (!configExisted)
    {
        actions.Add("create openspec/config.yaml with schema: tacos and context hook");
        if (!dryRun)
        {
            WriteMinimalConfig(configPath, repoRoot);
        }
    }

    if (configExisted && setSchema)
    {
        var (applied, message) = MergeConfigSchema(configPath, dryRun);
        actions.Add(message);
        if (!applied && message.Contains("left unchanged", StringComparison.Ordinal))
        {
            Console.WriteLine($"Note: {message}");
        }
    }

    if (configExisted || !dryRun)
    {
        var (_, contextMessage) = EnsureOrchestrationContextHook(
            configPath,
            force,
            dryRun,
            repoRoot
        );
        actions.Add(contextMessage);
    }
    else
    {
        actions.Add("prepend tacos orchestration context hook (included in new config)");
    }

    if (skipTacosYamlSync)
    {
        actions.Add("openspec/tacos.yaml sync deferred to update (additive merge + version)");
    }
    else if (!File.Exists(tacosYamlPath))
    {
        actions.Add("write openspec/tacos.yaml from tacos-doctor template");
        if (!dryRun)
        {
            File.Copy(tacosYamlTemplatePath, tacosYamlPath, overwrite: false);
        }
    }
    else if (force)
    {
        actions.Add("overwrite openspec/tacos.yaml from tacos-doctor template (--force)");
        if (!dryRun)
        {
            File.Copy(tacosYamlTemplatePath, tacosYamlPath, overwrite: true);
        }
    }
    else
    {
        var (applied, message) = MergeTacosYamlFile(tacosYamlPath, tacosYamlTemplatePath, dryRun);
        actions.Add(message);
        if (applied)
        {
            Console.WriteLine($"Note: {message}");
        }
    }

    var schemaSamePath = string.Equals(
        Path.GetFullPath(schemaSource)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        Path.GetFullPath(schemaDest)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        StringComparison.OrdinalIgnoreCase
    );
    if (schemaSamePath)
    {
        actions.Add("schema already at openspec/schemas/tacos");
    }
    else if (IsTacosSchemaInstalled(schemaDest) && !force)
    {
        if (refreshSchemaFromBundle && !schemaSamePath)
        {
            actions.Add("replace schema from bundle -> openspec/schemas/tacos/");
            ReplaceSchemaFromBundle(schemaSource, schemaDest, dryRun);
        }
        else
        {
            actions.Add("schema already at openspec/schemas/tacos");
        }
    }
    else
    {
        var incomplete = Directory.Exists(schemaDest) && !IsTacosSchemaInstalled(schemaDest);
        actions.Add(
            incomplete ? "repair incomplete schema -> openspec/schemas/tacos/"
            : force && IsTacosSchemaInstalled(schemaDest)
                ? "refresh schema -> openspec/schemas/tacos/"
            : "copy schema -> openspec/schemas/tacos/"
        );
        if (!dryRun)
        {
            CopyDirectory(schemaSource, schemaDest, overwrite: force);
        }
    }

    if (!sandbox)
    {
        var (_, agentsMessage) = EnsureHostAgentsSnippet(repoRoot, skillRoot, force, dryRun);
        actions.Add(agentsMessage);

        var (_, gatesMessage) = EnsureHostImplementationGatesSnippet(repoRoot, skillRoot, dryRun);
        actions.Add(gatesMessage);
        if (!dryRun && ImplementationGatesUpdatePreserveWarn(gatesMessage))
        {
            Console.WriteLine(
                "WARN implementation-gates: verify Commands stay current after stack or script changes"
            );
        }

        if (syncHostSubagents)
        {
            if (!EnsureHostSubagentBundleValid(skillRoot))
            {
                return 1;
            }

            var (_, hostAgentsMessage) = HostAgentInstall.InstallHostSubagentTemplates(
                repoRoot,
                ResolveHostSubagentsInstallRoot(repoRoot),
                skillRoot,
                dryRun
            );
            actions.Add(hostAgentsMessage);
        }
        else
        {
            actions.Add("skip host subagents (run /tacos-doctor config after *_models changes)");
        }
    }
    else
    {
        actions.Add("skip AGENTS.md snippet (--sandbox)");
        actions.Add("skip host subagent templates (--sandbox)");
    }

    actions.Add(SyncHostOverlayTemplates(skillRoot, openspecDir, dryRun));

    if (!sandbox && File.Exists(tacosYamlPath))
    {
        var layoutCtxForWorkspace = ResolveLayoutContext(repoRoot);
        if (
            layoutCtxForWorkspace.Mode == LayoutModeKind.Single
            && layoutCtxForWorkspace.GitRoot is not null
            && layoutCtxForWorkspace.LayoutRoot.Equals(
                layoutCtxForWorkspace.GitRoot,
                StringComparison.OrdinalIgnoreCase
            )
            && TryParseWorkspaceConfig(tacosYamlPath) is null
        )
        {
            var (wsApplied, wsMessage) = MergeSingleDefaultWorkspace(
                tacosYamlPath,
                skillRoot,
                dryRun
            );
            if (wsApplied || dryRun)
            {
                actions.Add(wsMessage);
            }
        }
    }

    if (!sandbox)
    {
        actions.Add(EnsureArtifactsGitignore(repoRoot, dryRun));
        RegenerateEntryArtifactsFromLayout(repoRoot, dryRun);
    }

    foreach (var a in actions)
    {
        Console.WriteLine(dryRun ? $"[dry-run] {a}" : a);
    }

    if (!dryRun)
    {
        Console.WriteLine();
        Console.WriteLine("Schema source: " + schemaSource);
        Console.WriteLine("Next: openspec schema validate tacos");
        PrintProjectOverviewInstallHint(repoRoot);
        if (backupStamp is not null)
        {
            var backupRel = Path.Combine("artifacts", TacosBackupSubdir, backupStamp)
                .Replace('\\', '/');
            Console.WriteLine();
            Console.WriteLine($"Backup: {backupRel}/ (under artifacts/, gitignored)");
            Console.WriteLine(
                "Restore openspec/config.yaml from backup if you need host edits after a mistaken sync."
            );
        }
    }

    return 0;
}

int RunDiagnose((string SkillRoot, string SchemaSource) bundle)
{
    var ok = true;
    void Check(bool pass, string msg)
    {
        Console.WriteLine(pass ? $"OK  {msg}" : $"FAIL {msg}");
        if (!pass)
        {
            ok = false;
        }
    }

    void Warn(string msg) => Console.WriteLine($"WARN {msg}");

    Check(Directory.Exists(bundle.SchemaSource), "tacos-doctor/schemas/tacos");
    Check(Directory.Exists(openspecDir), "openspec/ exists");
    var schemaDest = Path.Combine(openspecDir, "schemas", "tacos");
    var schemaInstalled = IsTacosSchemaInstalled(schemaDest);
    Check(schemaInstalled, "openspec/schemas/tacos has schema.yaml");

    var skillsPrefixEarly = FindHostTacosSkillsPrefix(repoRoot);
    if (skillsPrefixEarly is not null && !schemaInstalled)
    {
        Warn(
            $"tacos skills at {skillsPrefixEarly} but openspec/schemas/tacos/ missing "
                + "— run /tacos-doctor install"
        );
    }

    if (File.Exists(Path.Combine(openspecDir, "config.yaml")))
    {
        var configPath = Path.Combine(openspecDir, "config.yaml");
        var configText = File.ReadAllText(configPath, Encoding.UTF8);
        var schemaMatch = Regex.Match(configText, @"^schema:\s*(\S+)\s*$", RegexOptions.Multiline);
        Check(
            schemaMatch.Success && schemaMatch.Groups[1].Value == "tacos",
            $"config.yaml schema: {(schemaMatch.Success ? schemaMatch.Groups[1].Value : "(unset)")}"
        );
        Check(
            configText.Contains(TacosBeginMarker, StringComparison.Ordinal),
            "config.yaml has tacos context hook (tacos-config managed block)"
        );
        Check(
            configText.Contains("tacos-orchestration", StringComparison.Ordinal),
            "config.yaml references tacos-orchestration"
        );
        Check(
            configText.Contains("project-overview-hooks", StringComparison.Ordinal),
            "config.yaml references project-overview hooks"
        );
    }
    else
    {
        Check(false, "openspec/config.yaml exists");
    }

    var tacosYamlPath = Path.Combine(openspecDir, "tacos.yaml");
    Check(File.Exists(tacosYamlPath), "openspec/tacos.yaml exists");

    var layoutCtx = ResolveLayoutContext(Directory.GetCurrentDirectory());
    if (layoutCtx.IsAmbiguous)
    {
        Check(false, $"layout resolution: {layoutCtx.AmbiguityError}");
    }
    else
    {
        Check(true, $"layout root: {layoutCtx.LayoutRoot.Replace('\\', '/')}");
        Check(true, $"layout mode: {layoutCtx.ModeLabel}");
        if (layoutCtx.HasWorkspace && layoutCtx.Workspace is not null)
        {
            if (layoutCtx.IsTeamIslandWorkspace)
            {
                Check(true, "workspace team island (tacos-workspaces/<id>/)");
            }

            var folderCount = layoutCtx.Workspace.Folders.Count;
            Check(
                folderCount > 0,
                folderCount > 0
                    ? $"workspace.folders: {folderCount} entries"
                    : "workspace.folders: at least one entry required"
            );
        }
        else if (
            layoutCtx.Mode == LayoutModeKind.Single
            && layoutCtx.GitRoot is not null
            && layoutCtx.LayoutRoot.Equals(layoutCtx.GitRoot, StringComparison.OrdinalIgnoreCase)
        )
        {
            Warn(
                "openspec/tacos.yaml has no workspace block — run /tacos-doctor install or update"
            );
        }
    }

    if (
        schemaInstalled
        && File.Exists(tacosYamlPath)
        && TryGetBundleVersion(bundle.SkillRoot, out var bundleVersion)
        && TryReadTacosYamlVersion(tacosYamlPath, out var recordedVersion)
    )
    {
        var drift = CompareSemVer(recordedVersion, bundleVersion);
        if (drift < 0)
        {
            Warn(
                $"openspec/tacos.yaml version {recordedVersion} is behind installed bundle "
                    + $"{bundleVersion} — run /tacos-doctor update"
            );
        }
        else if (drift > 0)
        {
            Warn(
                $"openspec/tacos.yaml version {recordedVersion} is ahead of bundle "
                    + $"{bundleVersion} — run update after refreshing skills or pass --target-version"
            );
        }
        else if (!string.Equals(recordedVersion, bundleVersion, StringComparison.Ordinal))
        {
            Warn(
                $"openspec/tacos.yaml version {recordedVersion} differs from bundle "
                    + $"{bundleVersion} — run /tacos-doctor update to reconcile"
            );
        }
    }

    if (!sandbox)
    {
        var agentsPath = Path.Combine(repoRoot, "AGENTS.md");
        if (File.Exists(agentsPath))
        {
            var agentsText = File.ReadAllText(agentsPath, Encoding.UTF8);
            Check(
                agentsText.Contains(TacosAgentsBeginMarker, StringComparison.Ordinal),
                "AGENTS.md has tacos OpenSpec snippet (tacos-agents managed block)"
            );
            Check(
                agentsText.Contains("tacos-orchestration", StringComparison.Ordinal),
                "AGENTS.md references tacos-orchestration"
            );
            Check(
                agentsText.Contains("POST-ARTIFACT", StringComparison.OrdinalIgnoreCase),
                "AGENTS.md contains POST-ARTIFACT"
            );
            Check(
                agentsText.Contains("project-overview-hooks", StringComparison.Ordinal),
                "AGENTS.md references project-overview-hooks"
            );

            foreach (var line in DiagnoseImplementationGatesBlock(agentsText))
            {
                if (line.IsFailure)
                {
                    Check(false, line.Message);
                }
                else if (line.IsWarning)
                {
                    Warn(line.Message);
                }
                else
                {
                    Check(true, line.Message);
                }
            }
        }
        else
        {
            Check(false, "AGENTS.md exists (run set-schema to add OpenSpec snippet)");
        }
    }

    if (ArtifactsReviewOutputIgnored(repoRoot, out var gitignoreDetail))
    {
        Check(true, gitignoreDetail);
    }
    else
    {
        Warn($"{gitignoreDetail} — run set-schema to append artifacts/ or add it manually");
    }

    if (HasRedundantCursorSkillsDuplicate(repoRoot))
    {
        Warn(
            $"{CursorLegacySkillsPrefix}/ duplicates {CursorCanonicalSkillsPrefix}/ for Cursor "
                + "— remove .cursor/skills/tacos-* (Cursor loads .agents/skills/)"
        );
    }

    foreach (var nested in EnumerateNestedAccidentalSkillsInstalls(repoRoot))
    {
        Warn(FormatNestedAccidentalSkillsInstallMessage(nested));
    }

    var skillsSearchRoot = layoutCtx.LayoutRoot;
    var skillsPrefixes = EnumerateHostTacosSkillsPrefixes(skillsSearchRoot);
    if (HasGlobalTacosOrchestrationSkill() && skillsPrefixes.Count > 0)
    {
        Warn(
            "global and project tacos skills both present — Cursor may show duplicate slash commands; "
                + "use project-local skills only"
        );
    }

    if (skillsPrefixes.Count == 0)
    {
        Warn(
            $"tacos skills install root not found under repo (probed {HostSkillsRootCandidates.Length} "
                + "OpenSpec-aligned project paths); using "
                + DefaultSkillsPrefix
                + " for host snippets"
        );
    }
    else if (skillsPrefixes.Count == 1)
    {
        Check(true, $"tacos skills install root: {skillsPrefixes[0]}");
    }
    else
    {
        Check(
            true,
            $"tacos skills install roots ({skillsPrefixes.Count}): "
                + string.Join(", ", skillsPrefixes)
        );
    }

    var skillsPrefix = skillsPrefixes.FirstOrDefault();

    var forbiddenCount = CountForbiddenHostRootsInSkillBodies(
        skillsSearchRoot,
        out var forbiddenSample
    );
    if (forbiddenCount > 0)
    {
        Warn(
            $"skill bodies contain hardcoded host install roots ({forbiddenCount} file(s); e.g. {forbiddenSample})"
        );
    }
    else if (skillsPrefix is not null)
    {
        Check(true, "skill bodies: no hardcoded host install roots under discovered prefix");
    }

    foreach (var line in DiagnoseReviewSkillsWiring(repoRoot, openspecDir))
    {
        if (line.IsFailure)
        {
            Check(false, line.Message);
        }
        else if (line.IsWarning)
        {
            Warn(line.Message);
        }
        else
        {
            Check(true, line.Message);
        }
    }

    if (!sandbox)
    {
        var (hostOk, hostLines) = HostAgentInstall.DiagnoseHostSubagents(
            repoRoot,
            ResolveHostSubagentsInstallRoot(repoRoot),
            bundle.SkillRoot
        );
        foreach (var line in hostLines)
        {
            if (line.IsFailure)
            {
                Check(false, line.Message);
            }
            else if (line.IsWarning)
            {
                Warn(line.Message);
            }
            else
            {
                Check(true, line.Message);
            }
        }

        if (!hostOk)
        {
            ok = false;
        }
    }

    return ok ? 0 : 1;
}

bool EnsureHostSubagentBundleValid(string skillRoot)
{
    var hostBundleErrors = HostAgentInstall.ValidateBundleTemplates(skillRoot);
    if (hostBundleErrors.Count == 0)
    {
        return true;
    }

    foreach (var err in hostBundleErrors)
    {
        Console.Error.WriteLine($"FAIL host subagents bundle: {err}");
    }

    return false;
}

void PrintUsage()
{
    Console.WriteLine(
        """
        schema.cs — set OpenSpec schema "tacos"

        Usage:
          dotnet scripts/schema.cs [diagnose|set-schema|update|config|skill-refresh|workspace-init|merge-review-skills] [options]

        Options:
          --source <path>         tacos-doctor skill dir or repo (optional)
          --distribution-source <spec>
                                  skill-refresh: repo or package (default: servicetitan/tacos)
          --target-version <v>    update: record this semver in openspec/tacos.yaml (default: bundle template)
          --dry-run               Print actions only
          --force                 Overwrite host openspec/tacos.yaml; refresh tacos-config and
                                  tacos-agents managed blocks in config.yaml and AGENTS.md;
                                  replace openspec/schemas/tacos/ (does not refresh
                                  tacos-implementation-gates inner body)
          --no-set-schema         Copy schema and merge context; do not change config.yaml schema
          --sandbox               Write to artifacts/schema/openspec/ instead of repo openspec/
          --spec <path>           merge-review-skills: repo-relative path for spec review array
          --apply <path>          merge-review-skills: repo-relative path for apply review array
          workspace-init options:
          --island-id <name>      team island directory name under tacos-workspaces/ (requires git root)
          --layout-root <path>    personal workspace layout root (off-repo or in-repo; git optional)
          --folders-json <json>   JSON array [{\"name\":\"…\",\"path\":\"…\"}]

        merge-review-skills: populate empty review.*_additional_skills arrays only; preserve
                               non-empty arrays; skip invalid paths with WARN.
        update: set-schema for config/AGENTS.md; replace openspec/schemas/tacos/ from bundle;
                merge openspec/tacos.yaml + sync version (no host subagent sync).
        config: install or model-sync .cursor/agents/ and .claude/agents/ from openspec/tacos.yaml
                *_models (after install or when you change host config).
        skill-refresh: list npx skills add commands to refresh every detected project skills root.

        Examples:
          dotnet scripts/schema.cs diagnose
          dotnet scripts/schema.cs set-schema
          dotnet scripts/schema.cs update
          dotnet scripts/schema.cs config
          dotnet scripts/schema.cs skill-refresh
          dotnet scripts/schema.cs merge-review-skills --apply <skills-root>/backend-conventions
        """
    );
}
