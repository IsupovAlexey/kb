partial class Program
{
    static string SyncHostOverlayTemplates(string skillRoot, string openspecDir, bool dryRun)
    {
        var sourceDir = Path.Combine(skillRoot, "templates", "openspec", "host");
        if (!Directory.Exists(sourceDir))
        {
            return "skip host overlay templates (bundle templates/openspec/host/ missing)";
        }

        var destDir = Path.Combine(openspecDir, "host");
        var synced = 0;
        foreach (var file in Directory.EnumerateFiles(sourceDir))
        {
            var name = Path.GetFileName(file);
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var isReadme = name.Equals("README.md", StringComparison.OrdinalIgnoreCase);
            var isTemplate = name.EndsWith(".md.template", StringComparison.OrdinalIgnoreCase);
            if (!isReadme && !isTemplate)
            {
                continue;
            }

            if (!dryRun)
            {
                Directory.CreateDirectory(destDir);
                File.Copy(file, Path.Combine(destDir, name), overwrite: true);
            }

            synced++;
        }

        if (synced == 0)
        {
            return "skip host overlay templates (no README.md or *.md.template in bundle)";
        }

        return dryRun
            ? $"sync {synced} host overlay file(s) -> {destDir}/"
            : $"synced {synced} host overlay file(s) -> {destDir}/ (README.md + *.md.template overwrite; user-activated overlay *.md preserved)";
    }
}
