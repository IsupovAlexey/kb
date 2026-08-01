using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    static (string SkillRoot, string SchemaSource)? ResolveSchemaBundle(
        string targetRepo,
        string explicitSource
    )
    {
        var skillRoot =
            FindTacosDoctorSkillRoot(targetRepo)
            ?? FindTacosDoctorSkillRoot(Directory.GetCurrentDirectory())
            ?? (
                string.IsNullOrEmpty(explicitSource)
                    ? null
                    : FindTacosDoctorSkillRoot(explicitSource)
            );

        if (skillRoot is null && !string.IsNullOrEmpty(explicitSource))
        {
            var fromSource = Path.Combine(explicitSource, "tacos-doctor");
            if (Directory.Exists(Path.Combine(fromSource, "schemas", "tacos")))
            {
                skillRoot = fromSource;
            }
            else if (Directory.Exists(Path.Combine(explicitSource, "schemas", "tacos")))
            {
                skillRoot = explicitSource;
            }
        }

        if (skillRoot is null)
        {
            return null;
        }

        var schemaSource = Path.Combine(skillRoot, "schemas", "tacos");
        return Directory.Exists(schemaSource) ? (skillRoot, schemaSource) : null;
    }

    static string? FindOpenSpecRepoRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "openspec")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    static string? FindTacosRepoRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            // Repo root markers only — do not treat tacos-doctor/ as the repo (avoids skill-local artifacts/).
            if (
                Directory.Exists(Path.Combine(dir.FullName, "openspec"))
                || File.Exists(Path.Combine(dir.FullName, "AGENTS.md"))
            )
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    static void EnsureSandboxOpenspec(string targetOpenspecDir, string repoRoot, bool dryRun)
    {
        Directory.CreateDirectory(targetOpenspecDir);
        var configPath = Path.Combine(targetOpenspecDir, "config.yaml");
        if (File.Exists(configPath))
        {
            return;
        }

        var seedPath = Path.Combine(repoRoot, "dev", SandboxArtifactsSubdir, "stock-config.yaml");
        if (!File.Exists(seedPath))
        {
            Console.Error.WriteLine($"Sandbox seed not found: {seedPath}");
            Environment.Exit(1);
        }

        if (!dryRun)
        {
            File.Copy(seedPath, configPath, overwrite: false);
        }

        Console.WriteLine(
            dryRun
                ? "[dry-run] seed sandbox from dev/schema/stock-config.yaml"
                : "seed sandbox from dev/schema/stock-config.yaml"
        );
    }

    static string? FindTacosDoctorSkillRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            foreach (var skillsRoot in HostSkillsRootCandidates)
            {
                var doctor = CombineRepoRelative(dir.FullName, skillsRoot, "tacos-doctor");
                if (Directory.Exists(Path.Combine(doctor, "schemas", "tacos")))
                {
                    return doctor;
                }
            }

            if (
                dir.Name == "tacos-doctor"
                && Directory.Exists(Path.Combine(dir.FullName, "schemas", "tacos"))
            )
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    static bool IsTacosSchemaInstalled(string schemaDir) =>
        File.Exists(Path.Combine(schemaDir, "schema.yaml"));

    static void CopyDirectory(string source, string dest, bool overwrite)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(source, file);
            var target = Path.Combine(dest, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite);
        }
    }

    static void ReplaceSchemaFromBundle(string source, string dest, bool dryRun)
    {
        if (dryRun)
        {
            return;
        }

        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, recursive: true);
        }

        CopyDirectory(source, dest, overwrite: true);
    }

    static string TacosBackupRoot(string repoRoot) =>
        Path.Combine(repoRoot, "artifacts", TacosBackupSubdir);

    static string CreateTacosBackupStamp() => DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

    static string? BackupRepoPath(
        string repoRoot,
        string sourceAbsolutePath,
        string stamp,
        bool dryRun
    )
    {
        if (!File.Exists(sourceAbsolutePath))
        {
            return null;
        }

        var rel = Path.GetRelativePath(repoRoot, sourceAbsolutePath).Replace('\\', '/');
        var dest = Path.Combine(TacosBackupRoot(repoRoot), stamp, rel);
        if (!dryRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(sourceAbsolutePath, dest, overwrite: true);
        }

        return Path.GetRelativePath(repoRoot, dest).Replace('\\', '/');
    }

    static bool TryGetBundleVersion(string skillRoot, out string version)
    {
        version = "";
        var templatePath = Path.Combine(skillRoot, "templates", "openspec", "tacos.yaml");
        return TryReadTacosYamlVersion(templatePath, out version);
    }

    static bool TryReadTacosYamlVersion(string path, out string version)
    {
        version = "";
        if (!File.Exists(path))
        {
            return false;
        }

        var match = Regex.Match(
            File.ReadAllText(path, Utf8NoBom),
            @"^version:\s*[""']?(?<v>[^""'\r\n#]+)[""']?\s*$",
            RegexOptions.Multiline
        );
        if (!match.Success)
        {
            return false;
        }

        version = match.Groups["v"].Value.Trim();
        return version.Length > 0;
    }

    static int CompareSemVer(string left, string right)
    {
        var a = left.Trim().Trim('"');
        var b = right.Trim().Trim('"');
        if (Version.TryParse(a, out var va) && Version.TryParse(b, out var vb))
        {
            return va.CompareTo(vb);
        }

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
