using System.Text;
using System.Text.RegularExpressions;

static partial class HostAgentInstall
{
    public static (bool Ok, IReadOnlyList<DiagnosticLine> Lines) DiagnoseHostSubagents(
        string layoutRoot,
        string hostSubagentsRoot,
        string skillRoot
    )
    {
        var lines = new List<DiagnosticLine>();
        var failed = false;

        var bundleErrors = ValidateBundleTemplates(skillRoot);
        if (bundleErrors.Count == 0)
        {
            lines.Add(
                new DiagnosticLine(
                    false,
                    false,
                    $"bundle templates/agents: {AgentModelYamlPaths.Count} agents registered"
                )
            );
        }
        else
        {
            failed = true;
            foreach (var err in bundleErrors)
            {
                lines.Add(new DiagnosticLine(true, false, $"bundle templates/agents: {err}"));
            }
        }

        var tacosYamlPath = Path.Combine(layoutRoot, TacosYamlRelative);
        Dictionary<string, object?>? tacosRoot = null;
        if (!File.Exists(tacosYamlPath))
        {
            lines.Add(
                new DiagnosticLine(
                    false,
                    true,
                    "openspec/tacos.yaml missing — host model maps not checked"
                )
            );
        }
        else
        {
            tacosRoot = ParseYamlRoot(File.ReadAllText(tacosYamlPath, Encoding.UTF8));
            if (tacosRoot is null)
            {
                failed = true;
                lines.Add(new DiagnosticLine(true, false, "openspec/tacos.yaml: parse failed"));
            }
            else
            {
                ValidateTacosYamlModelMaps(tacosRoot, lines, ref failed);
            }
        }

        DiagnoseInstalledHostSet(
            layoutRoot,
            hostSubagentsRoot,
            ".cursor",
            "cursor",
            tacosRoot,
            lines,
            ref failed
        );
        DiagnoseInstalledHostSet(
            layoutRoot,
            hostSubagentsRoot,
            ".claude",
            "claude",
            tacosRoot,
            lines,
            ref failed
        );

        if (
            !Directory.Exists(Path.Combine(hostSubagentsRoot, ".cursor"))
            && !Directory.Exists(Path.Combine(hostSubagentsRoot, ".claude"))
            && !Directory.Exists(Path.Combine(layoutRoot, ".cursor"))
            && !Directory.Exists(Path.Combine(layoutRoot, ".claude"))
        )
        {
            lines.Add(
                new DiagnosticLine(
                    false,
                    true,
                    "no .cursor/ or .claude/ — host subagent install skipped"
                )
            );
        }

        return (!failed, lines);
    }

    static void ValidateTacosYamlModelMaps(
        Dictionary<string, object?> tacosRoot,
        List<DiagnosticLine> lines,
        ref bool failed
    )
    {
        foreach (var (fileName, yamlPath) in AgentModelYamlPaths)
        {
            var label = string.Join('.', yamlPath);
            if (!TryGetHostModelMap(tacosRoot, yamlPath, out var hostMap))
            {
                failed = true;
                lines.Add(
                    new DiagnosticLine(
                        true,
                        false,
                        $"openspec/tacos.yaml missing or invalid {label}"
                    )
                );
                continue;
            }

            foreach (var flavor in RequiredHostFlavors)
            {
                if (!hostMap.TryGetValue(flavor, out var raw) || raw is null)
                {
                    lines.Add(
                        new DiagnosticLine(
                            false,
                            true,
                            $"openspec/tacos.yaml {label}.{flavor} missing — install uses bundle default for {fileName}"
                        )
                    );
                    continue;
                }

                var model = raw.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(model))
                {
                    lines.Add(
                        new DiagnosticLine(
                            false,
                            true,
                            $"openspec/tacos.yaml {label}.{flavor} missing — install uses bundle default for {fileName}"
                        )
                    );
                    continue;
                }
                var modelError = ValidateModelSlug(flavor, model);
                if (modelError is not null)
                {
                    failed = true;
                    lines.Add(
                        new DiagnosticLine(
                            true,
                            false,
                            $"openspec/tacos.yaml {label}.{flavor}: {modelError}"
                        )
                    );
                }
            }
        }
    }

    static void DiagnoseInstalledHostSet(
        string layoutRoot,
        string hostSubagentsRoot,
        string hostDirName,
        string hostFlavor,
        Dictionary<string, object?>? tacosRoot,
        List<DiagnosticLine> lines,
        ref bool failed
    )
    {
        var hostRoot = Path.Combine(hostSubagentsRoot, hostDirName);
        if (!Directory.Exists(hostRoot))
        {
            return;
        }

        var agentsDir = Path.Combine(hostRoot, "agents");
        if (!Directory.Exists(agentsDir))
        {
            var misplaced = Path.Combine(layoutRoot, hostDirName, "agents");
            if (
                !Path.GetFullPath(hostSubagentsRoot)
                    .Equals(Path.GetFullPath(layoutRoot), StringComparison.OrdinalIgnoreCase)
                && Directory.Exists(misplaced)
                && (
                    Directory.EnumerateFiles(misplaced, "agent-tacos-*.md").Any()
                    || Directory.EnumerateFiles(misplaced, "tacos-*.md").Any()
                )
            )
            {
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents/* at layout root — run /tacos-doctor config to sync host subagents"
                    )
                );
            }
            else
            {
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents missing — run /tacos-doctor install"
                    )
                );
            }

            return;
        }

        var installed = 0;
        var drift = 0;
        var missing = 0;

        foreach (var (fileName, yamlPath) in AgentModelYamlPaths)
        {
            var target = Path.Combine(agentsDir, fileName);
            if (!File.Exists(target))
            {
                missing++;
                continue;
            }

            installed++;
            var expected = ResolveModel(fileName, hostFlavor, tacosRoot, yamlPath);
            if (!TryReadFrontmatterModel(target, out var actual))
            {
                failed = true;
                lines.Add(
                    new DiagnosticLine(
                        true,
                        false,
                        $"{hostDirName}/agents/{fileName}: no model: in frontmatter"
                    )
                );
                continue;
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                drift++;
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents/{fileName}: model {actual} != yaml/default {expected} (run /tacos-doctor config or edit file)"
                    )
                );
            }

            if (actual.Contains(ModelPlaceholder, StringComparison.Ordinal))
            {
                failed = true;
                lines.Add(
                    new DiagnosticLine(
                        true,
                        false,
                        $"{hostDirName}/agents/{fileName}: unreplaced {ModelPlaceholder}"
                    )
                );
            }

            var expectedPrefix = Program.ResolveSkillsPrefixForHost(layoutRoot, hostDirName);
            var body = File.ReadAllText(target, Encoding.UTF8);
            if (
                body.Contains(".agents/skills/tacos-", StringComparison.Ordinal)
                && !expectedPrefix.Equals(".agents/skills", StringComparison.Ordinal)
            )
            {
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents/{fileName}: stale hardcoded skills prefix (expected {expectedPrefix}) — edit paths or delete file and run /tacos-doctor config for a fresh template"
                    )
                );
            }
            else if (
                body.Contains(".cursor/skills/tacos-", StringComparison.Ordinal)
                && !expectedPrefix.Equals(".cursor/skills", StringComparison.Ordinal)
            )
            {
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents/{fileName}: stale hardcoded skills prefix (expected {expectedPrefix}) — edit paths or delete file and run /tacos-doctor config for a fresh template"
                    )
                );
            }
            else if (
                body.Contains(".claude/skills/tacos-", StringComparison.Ordinal)
                && !expectedPrefix.Equals(".claude/skills", StringComparison.Ordinal)
            )
            {
                lines.Add(
                    new DiagnosticLine(
                        false,
                        true,
                        $"{hostDirName}/agents/{fileName}: stale hardcoded skills prefix (expected {expectedPrefix}) — edit paths or delete file and run /tacos-doctor config for a fresh template"
                    )
                );
            }
        }

        var summary = $"{hostDirName}/agents: {installed}/{AgentModelYamlPaths.Count} installed";
        if (missing > 0)
        {
            summary += $", {missing} missing (run /tacos-doctor config)";
            lines.Add(new DiagnosticLine(false, true, summary));
        }
        else if (drift > 0)
        {
            lines.Add(
                new DiagnosticLine(
                    false,
                    true,
                    $"{summary}, {drift} model drift vs openspec/tacos.yaml"
                )
            );
        }
        else
        {
            lines.Add(
                new DiagnosticLine(false, false, summary + ", models match openspec/tacos.yaml")
            );
        }
    }
}
