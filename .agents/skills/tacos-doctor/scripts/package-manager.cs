partial class Program
{
    internal enum PackageManagerKind
    {
        Npm,
        Pnpm,
    }

    internal static PackageManagerKind ResolvePackageManager(LayoutContext layout)
    {
        var scanRoot = layout.GitRoot ?? layout.LayoutRoot;
        return HasPnpmLockFileUnder(scanRoot) ? PackageManagerKind.Pnpm : PackageManagerKind.Npm;
    }

    internal static string PackageManagerLabel(PackageManagerKind kind) =>
        kind switch
        {
            PackageManagerKind.Pnpm => "pnpm",
            PackageManagerKind.Npm => "npm",
            _ => "npm",
        };

    internal static string SkillsExecPrefix(PackageManagerKind kind) =>
        kind switch
        {
            PackageManagerKind.Pnpm => "pnpm dlx",
            PackageManagerKind.Npm => "npx",
            _ => "npx",
        };

    internal static string FormatGlobalInstallCommand(PackageManagerKind kind, string package) =>
        kind switch
        {
            PackageManagerKind.Pnpm =>
                $"pnpm add -g {package} --loglevel=error --no-fund --no-audit",
            _ => $"npm install -g {package} --loglevel=error --no-fund --no-audit",
        };

    internal static (string Command, string[] Args) GetGlobalInstallProcess(
        PackageManagerKind kind,
        string package
    ) =>
        kind switch
        {
            PackageManagerKind.Pnpm => (
                "pnpm",
                ["add", "-g", package, "--loglevel=error", "--no-fund", "--no-audit"]
            ),
            _ => ("npm", ["install", "-g", package, "--loglevel=error", "--no-fund", "--no-audit"]),
        };

    internal static string SkillsInstallManualHint(PackageManagerKind kind, string prefix) =>
        kind switch
        {
            PackageManagerKind.Pnpm =>
                $"refresh tacos skills under {prefix} with pnpm dlx skills add … --agent <host> -y",
            _ => $"refresh tacos skills under {prefix} with npx skills add … --agent <host> -y",
        };

    internal static string SkillsInstallCwdReminder(PackageManagerKind kind) =>
        kind switch
        {
            PackageManagerKind.Pnpm =>
                "pnpm dlx skills add installs relative to cwd — run it from the OpenSpec project root, "
                    + "not from inside a skills directory.",
            _ => "npx skills add installs relative to cwd — run it from the OpenSpec project root, "
                + "not from inside a skills directory.",
        };

    internal static string SkillsRefreshRunReminder(PackageManagerKind kind) =>
        kind switch
        {
            PackageManagerKind.Pnpm =>
                "Run each pnpm dlx command below with cwd = OpenSpec project root (do not cd into a skills directory).",
            _ =>
                "Run each npx command below with cwd = OpenSpec project root (do not cd into a skills directory).",
        };

    static bool HasPnpmLockFileUnder(string scanRoot)
    {
        if (!Directory.Exists(scanRoot))
        {
            return false;
        }

        if (File.Exists(Path.Combine(scanRoot, "pnpm-lock.yaml")))
        {
            return true;
        }

        var queue = new Queue<string>();
        queue.Enqueue(scanRoot);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var dir in Directory.EnumerateDirectories(current))
            {
                var name = Path.GetFileName(dir);
                if (name is ".git" or "node_modules" or "artifacts")
                {
                    continue;
                }

                if (File.Exists(Path.Combine(dir, "pnpm-lock.yaml")))
                {
                    return true;
                }

                queue.Enqueue(dir);
            }
        }

        return false;
    }
}
