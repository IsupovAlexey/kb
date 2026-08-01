using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

partial class Program
{
    static bool ArtifactsReviewOutputIgnored(string root, out string detail)
    {
        if (
            GitCheckIgnore(root, "artifacts/openspec-reviews")
            || GitCheckIgnore(root, "artifacts")
            || GitCheckIgnore(root, ArtifactsGitignoreLine.TrimEnd('/'))
        )
        {
            detail = "gitignore: artifacts/ ignored (openspec-reviews under artifacts/)";
            return true;
        }

        var gitignorePath = Path.Combine(root, ".gitignore");
        if (!File.Exists(gitignorePath))
        {
            detail = "gitignore: no .gitignore at repo root";
            return false;
        }

        if (GitignoreFileIgnoresArtifacts(File.ReadAllText(gitignorePath, Utf8NoBom)))
        {
            detail = "gitignore: .gitignore lists artifacts/";
            return true;
        }

        detail = "gitignore: artifacts/ not ignored — review output may be committed";
        return false;
    }

    static bool GitignoreFileIgnoresArtifacts(string text)
    {
        foreach (var raw in text.Split(['\r', '\n']))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("!", StringComparison.Ordinal))
            {
                continue;
            }

            var pattern = line.TrimEnd('/');
            if (
                pattern.Equals("artifacts", StringComparison.Ordinal)
                || pattern.Equals("/artifacts", StringComparison.Ordinal)
                || pattern.Equals("artifacts/**", StringComparison.Ordinal)
                || pattern.Equals("/artifacts/**", StringComparison.Ordinal)
            )
            {
                return true;
            }

            if (
                line.Equals("artifacts/", StringComparison.Ordinal)
                || line.Equals("/artifacts/", StringComparison.Ordinal)
            )
            {
                return true;
            }
        }

        return false;
    }

    static bool GitCheckIgnore(string repoRoot, string relativePath)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    WorkingDirectory = repoRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            process.StartInfo.ArgumentList.Add("check-ignore");
            process.StartInfo.ArgumentList.Add("-q");
            process.StartInfo.ArgumentList.Add("--");
            process.StartInfo.ArgumentList.Add(relativePath.Replace('\\', '/'));
            process.Start();
            process.WaitForExit(TimeSpan.FromSeconds(10));
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    static string EnsureArtifactsGitignore(string root, bool dryRun)
    {
        if (ArtifactsReviewOutputIgnored(root, out _))
        {
            return "gitignore: artifacts/ already ignored";
        }

        var gitignorePath = Path.Combine(root, ".gitignore");
        if (dryRun)
        {
            return File.Exists(gitignorePath)
                ? $"[dry-run] append {ArtifactsGitignoreLine} to .gitignore"
                : $"[dry-run] create .gitignore with {ArtifactsGitignoreLine}";
        }

        if (File.Exists(gitignorePath))
        {
            var text = File.ReadAllText(gitignorePath, Utf8NoBom);
            if (!text.EndsWith('\n'))
            {
                text += "\n";
            }

            text += ArtifactsGitignoreLine + "\n";
            File.WriteAllText(gitignorePath, text, Utf8NoBom);
            return $"append {ArtifactsGitignoreLine} to .gitignore";
        }

        File.WriteAllText(gitignorePath, ArtifactsGitignoreLine + "\n", Utf8NoBom);
        return $"create .gitignore with {ArtifactsGitignoreLine}";
    }
}
