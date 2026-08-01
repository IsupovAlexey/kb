using System.Text;
using System.Text.RegularExpressions;

static partial class HostAgentInstall
{
    public static void RelocateHostSubagentTemplates(string fromRoot, string toRoot)
    {
        if (
            Path.GetFullPath(fromRoot)
                .Equals(Path.GetFullPath(toRoot), StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        foreach (var hostDirName in new[] { ".cursor", ".claude" })
        {
            var fromAgents = Path.Combine(fromRoot, hostDirName, "agents");
            if (!Directory.Exists(fromAgents))
            {
                continue;
            }

            var toAgents = Path.Combine(toRoot, hostDirName, "agents");
            Directory.CreateDirectory(toAgents);

            foreach (var file in Directory.EnumerateFiles(fromAgents, "agent-tacos-*.md"))
            {
                var dest = Path.Combine(toAgents, Path.GetFileName(file));
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                File.Move(file, dest);
            }

            foreach (var file in Directory.EnumerateFiles(fromAgents, "tacos-*.md"))
            {
                var dest = Path.Combine(toAgents, Path.GetFileName(file));
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                File.Move(file, dest);
            }
        }
    }

    public static (bool AnyCopied, string Message) InstallHostSubagentTemplates(
        string layoutRoot,
        string hostSubagentsRoot,
        string skillRoot,
        bool dryRun
    )
    {
        var bundleErrors = ValidateBundleTemplates(skillRoot);
        if (bundleErrors.Count > 0)
        {
            return (false, "host subagents: bundle invalid — " + string.Join("; ", bundleErrors));
        }

        var parts = new List<string>();
        var anyCopied = false;

        var tacosYamlPath = Path.Combine(layoutRoot, TacosYamlRelative);
        Dictionary<string, object?>? tacosRoot = null;
        if (File.Exists(tacosYamlPath))
        {
            tacosRoot = ParseYamlRoot(File.ReadAllText(tacosYamlPath, Encoding.UTF8));
        }

        var splitHost = !Path.GetFullPath(hostSubagentsRoot)
            .Equals(Path.GetFullPath(layoutRoot), StringComparison.OrdinalIgnoreCase);

        anyCopied |= InstallHostSet(
            layoutRoot,
            hostSubagentsRoot,
            splitHost,
            skillRoot,
            ".cursor",
            "cursor",
            tacosRoot,
            dryRun,
            parts
        );
        anyCopied |= InstallHostSet(
            layoutRoot,
            hostSubagentsRoot,
            splitHost,
            skillRoot,
            ".claude",
            "claude",
            tacosRoot,
            dryRun,
            parts
        );

        if (parts.Count == 0)
        {
            return (false, "host subagents: skipped (no .cursor/ or .claude/ in repo)");
        }

        return (anyCopied, "host subagents: " + string.Join("; ", parts));
    }

    static string RenderAgentTemplate(string templateText, string model, string skillsPrefix)
    {
        var rendered = templateText
            .Replace(ModelPlaceholder, model, StringComparison.Ordinal)
            .Replace(SkillsPrefixPlaceholder, skillsPrefix, StringComparison.Ordinal);
        if (FrontmatterModelRegex.IsMatch(rendered))
        {
            rendered = FrontmatterModelRegex.Replace(rendered, $"model: {model}", 1);
        }

        return rendered;
    }

    static bool TrySyncSkillsPrefixPlaceholder(
        string agentFilePath,
        string skillsPrefix,
        bool dryRun,
        out bool changed
    )
    {
        changed = false;
        var text = File.ReadAllText(agentFilePath, Encoding.UTF8);
        if (!text.Contains(SkillsPrefixPlaceholder, StringComparison.Ordinal))
        {
            return true;
        }

        var patched = text.Replace(SkillsPrefixPlaceholder, skillsPrefix, StringComparison.Ordinal);
        changed = !string.Equals(text, patched, StringComparison.Ordinal);
        if (changed && !dryRun)
        {
            File.WriteAllText(
                agentFilePath,
                patched,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            );
        }

        return true;
    }

    static bool InstallHostSet(
        string layoutRoot,
        string hostSubagentsRoot,
        bool splitHost,
        string skillRoot,
        string hostDirName,
        string hostFlavor,
        Dictionary<string, object?>? tacosRoot,
        bool dryRun,
        List<string> parts
    )
    {
        var hostRoot = Path.Combine(hostSubagentsRoot, hostDirName);
        if (!Directory.Exists(hostRoot))
        {
            if (!splitHost)
            {
                return false;
            }

            if (!dryRun)
            {
                Directory.CreateDirectory(Path.Combine(hostRoot, "agents"));
            }
        }

        var skillsPrefix = Program.ResolveSkillsPrefixForHost(layoutRoot, hostDirName);

        var sourceDir = Path.Combine(skillRoot, "templates", "agents");
        if (!Directory.Exists(sourceDir))
        {
            parts.Add($"{hostDirName}/agents: bundle missing templates/agents");
            return false;
        }

        var destDir = Path.Combine(hostRoot, "agents");
        if (!dryRun)
        {
            Directory.CreateDirectory(destDir);
        }

        var copied = 0;
        var synced = 0;
        var unchanged = 0;
        var failed = 0;

        foreach (var (fileName, yamlPath) in AgentModelYamlPaths)
        {
            var sourceFile = Path.Combine(sourceDir, fileName);
            var target = Path.Combine(destDir, fileName);
            if (File.Exists(target))
            {
                var existingModel = ResolveModel(fileName, hostFlavor, tacosRoot, yamlPath);
                var existingModelError = ValidateModelSlug(hostFlavor, existingModel);
                if (existingModelError is not null)
                {
                    parts.Add($"{hostDirName}/agents: {fileName} model {existingModelError}");
                    failed++;
                    continue;
                }

                if (!TrySyncFrontmatterModel(target, existingModel, dryRun, out var modelChanged))
                {
                    parts.Add(
                        $"{hostDirName}/agents: {fileName} could not sync model: (missing frontmatter)"
                    );
                    failed++;
                    continue;
                }

                if (
                    !TrySyncSkillsPrefixPlaceholder(
                        target,
                        skillsPrefix,
                        dryRun,
                        out var prefixChanged
                    )
                )
                {
                    parts.Add(
                        $"{hostDirName}/agents: {fileName} could not sync {SkillsPrefixPlaceholder}"
                    );
                    failed++;
                    continue;
                }

                if (modelChanged || prefixChanged)
                {
                    synced++;
                }
                else
                {
                    unchanged++;
                }

                continue;
            }

            if (!File.Exists(sourceFile))
            {
                parts.Add($"{hostDirName}/agents: missing bundle template {fileName}");
                failed++;
                continue;
            }

            var model = ResolveModel(fileName, hostFlavor, tacosRoot, yamlPath);
            var modelError = ValidateModelSlug(hostFlavor, model);
            if (modelError is not null)
            {
                parts.Add($"{hostDirName}/agents: {fileName} model {modelError}");
                failed++;
                continue;
            }

            var templateText = File.ReadAllText(sourceFile, Encoding.UTF8);
            if (!templateText.Contains(ModelPlaceholder, StringComparison.Ordinal))
            {
                parts.Add($"{hostDirName}/agents: template {fileName} missing {ModelPlaceholder}");
                failed++;
                continue;
            }

            var rendered = RenderAgentTemplate(templateText, model, skillsPrefix);
            if (rendered.Contains(ModelPlaceholder, StringComparison.Ordinal))
            {
                parts.Add($"{hostDirName}/agents: {fileName} render left {ModelPlaceholder}");
                failed++;
                continue;
            }

            if (rendered.Contains(SkillsPrefixPlaceholder, StringComparison.Ordinal))
            {
                parts.Add(
                    $"{hostDirName}/agents: {fileName} render left {SkillsPrefixPlaceholder}"
                );
                failed++;
                continue;
            }

            if (!dryRun)
            {
                File.WriteAllText(
                    target,
                    rendered,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                );
            }

            copied++;
        }

        var suffix = failed > 0 ? $", {failed} failed" : "";
        parts.Add(
            $"{hostDirName}/agents: {copied} copied, {synced} model-synced, {unchanged} unchanged{suffix}"
        );
        return copied > 0 || synced > 0;
    }

    static bool TrySyncFrontmatterModel(
        string agentFilePath,
        string newModel,
        bool dryRun,
        out bool changed
    )
    {
        changed = false;
        var text = File.ReadAllText(agentFilePath, Encoding.UTF8);
        string patched;

        if (FrontmatterModelRegex.IsMatch(text))
        {
            patched = FrontmatterModelRegex.Replace(text, $"model: {newModel}", 1);
        }
        else if (FrontmatterNameRegex.IsMatch(text))
        {
            var nameMatch = FrontmatterNameRegex.Match(text);
            var insertAt = nameMatch.Index + nameMatch.Length;
            patched = text.Insert(insertAt, $"\nmodel: {newModel}");
        }
        else
        {
            return false;
        }

        changed = !string.Equals(text, patched, StringComparison.Ordinal);
        if (changed && !dryRun)
        {
            File.WriteAllText(
                agentFilePath,
                patched,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            );
        }

        return true;
    }

    static string? ValidateModelSlug(string hostFlavor, string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return "model must be non-empty";
        }

        if (model.Contains(ModelPlaceholder, StringComparison.Ordinal))
        {
            return "model must not contain placeholder";
        }

        if (model.Contains(',', StringComparison.Ordinal))
        {
            return "use a single slug per host (comma lists not supported in *_models maps)";
        }

        if (model.Any(char.IsWhiteSpace))
        {
            return "model must not contain whitespace";
        }

        return null;
    }

    static string ResolveModel(
        string fileName,
        string hostFlavor,
        Dictionary<string, object?>? tacosRoot,
        string[] yamlPath
    )
    {
        if (tacosRoot is not null)
        {
            var fromYaml = GetHostModelFromMap(tacosRoot, yamlPath, hostFlavor);
            if (!string.IsNullOrWhiteSpace(fromYaml))
            {
                return fromYaml.Trim();
            }
        }

        if (
            DefaultModels.TryGetValue(fileName, out var defaults)
            && defaults.TryGetValue(hostFlavor, out var fallback)
        )
        {
            return fallback;
        }

        return "inherit";
    }

    static string? GetHostModelFromMap(
        Dictionary<string, object?> root,
        string[] path,
        string hostFlavor
    )
    {
        if (!TryGetHostModelMap(root, path, out var hostMap))
        {
            return null;
        }

        if (!hostMap.TryGetValue(hostFlavor, out var value) || value is null)
        {
            return null;
        }

        return value.ToString()?.Trim();
    }

    static bool TryGetHostModelMap(
        Dictionary<string, object?> root,
        string[] path,
        out Dictionary<string, object?> hostMap
    )
    {
        hostMap = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        object? current = root;
        foreach (var segment in path)
        {
            if (
                current is not Dictionary<string, object?> dict
                || !dict.TryGetValue(segment, out current)
            )
            {
                return false;
            }
        }

        if (current is not Dictionary<string, object?> map)
        {
            return false;
        }

        hostMap = map;
        return true;
    }

    static bool TryReadFrontmatterModel(string agentFilePath, out string model)
    {
        model = "";
        var text = File.ReadAllText(agentFilePath, Encoding.UTF8);
        var match = FrontmatterModelRegex.Match(text);
        if (!match.Success)
        {
            return false;
        }

        model = match.Groups[1].Value.Trim().Trim('"').Trim('\'');
        return true;
    }

    static Dictionary<string, object?>? ParseYamlRoot(string yaml)
    {
        try
        {
            return YamlMergeAddOnly.ParseRoot(yaml);
        }
        catch
        {
            return null;
        }
    }
}
