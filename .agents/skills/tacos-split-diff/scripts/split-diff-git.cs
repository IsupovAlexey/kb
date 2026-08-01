static class GitRunner
{
    public static void EnsureRefExists(string gitRef)
    {
        try
        {
            _ = Run("rev-parse", "--verify", gitRef);
        }
        catch (SplitDiffException)
        {
            throw new SplitDiffException($"git ref not found: {gitRef}");
        }
    }

    public static string Run(params string[] arguments)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();

        if (process.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : stderr.Trim();
            throw new SplitDiffException(
                $"git {string.Join(' ', arguments)} failed (exit {process.ExitCode}): {detail}"
            );
        }

        return stdout;
    }

    public static IEnumerable<string> RunLines(params string[] arguments) =>
        Run(arguments).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
}
