using System.Diagnostics;

partial class Program
{
    static async Task<bool> WaitForProcessExitAsync(
        Process process,
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    )
    {
        var exitTask = process.WaitForExitAsync(cancellationToken);
        if (await Task.WhenAny(exitTask, Task.Delay(timeout, cancellationToken)) != exitTask)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // best-effort kill on timeout
            }

            return false;
        }

        await exitTask;
        return true;
    }

    internal static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessCaptureAsync(
        string executable,
        IEnumerable<string> commandArgs,
        string? workingDirectory = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var wait = timeout ?? TimeSpan.FromMinutes(5);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executable,
                WorkingDirectory = workingDirectory ?? "",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        foreach (var arg in commandArgs)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        if (!await WaitForProcessExitAsync(process, wait, cancellationToken))
        {
            var minutes = (int)Math.Ceiling(wait.TotalMinutes);
            return (
                -1,
                stdoutTask.IsCompletedSuccessfully ? await stdoutTask : "",
                $"timed out after {minutes} minutes"
            );
        }

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    internal static async Task<(bool Found, string VersionLine, string FailureDetail)> TryRunAsync(
        string command,
        string[] commandArgs,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var argList = string.Join(" ", commandArgs);
        var executable = ResolveExecutable(command);
        if (executable is null)
        {
            return (false, "", $"not found on PATH (tried `{command} {argList}`)");
        }

        try
        {
            var wait = timeout ?? TimeSpan.FromSeconds(15);
            var (exitCode, stdout, stderr) = await RunProcessCaptureAsync(
                executable,
                commandArgs,
                timeout: wait,
                cancellationToken: cancellationToken
            );

            if (exitCode == -1)
            {
                return (
                    false,
                    "",
                    $"timed out after {(int)Math.Ceiling(wait.TotalSeconds)}s (ran `{Path.GetFileName(executable)} {argList}`)"
                );
            }

            if (exitCode != 0)
            {
                var err = (stderr + stdout).Trim();
                if (err.Length > 120)
                {
                    err = err[..120] + "…";
                }

                var detail = string.IsNullOrWhiteSpace(err)
                    ? $"exited {exitCode} (ran `{Path.GetFileName(executable)} {argList}`)"
                    : $"exited {exitCode}: {err.Replace('\r', ' ').Replace('\n', ' ')}";
                return (false, "", detail);
            }

            var line =
                (stdout + stderr)
                    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()
                ?? "";
            return (true, line, "");
        }
        catch (Exception ex)
        {
            return (false, "", $"failed to start `{Path.GetFileName(executable)}`: {ex.Message}");
        }
    }

    internal static async Task<string> TryGetOpenspecGlobalDeliveryAsync(string projectRoot)
    {
        var executable = ResolveExecutable("openspec");
        if (executable is null)
        {
            return "both";
        }

        var (getCode, stdout, _) = await RunProcessCaptureAsync(
            executable,
            ["config", "get", "delivery"],
            projectRoot
        );
        if (getCode != 0)
        {
            return "both";
        }

        return NormalizeOpenspecGlobalDelivery(stdout);
    }

    internal static string NormalizeOpenspecGlobalDelivery(string raw)
    {
        var normalized = raw.Trim();
        if (normalized.Length >= 2 && normalized[0] == '"' && normalized[^1] == '"')
        {
            normalized = normalized[1..^1];
        }

        return normalized.Equals("commands", StringComparison.OrdinalIgnoreCase) ? "commands"
            : normalized.Equals("skills", StringComparison.OrdinalIgnoreCase) ? "skills"
            : "both";
    }

    internal static string? ResolveExecutable(string command)
    {
        if (Path.IsPathRooted(command) && File.Exists(command))
        {
            return command;
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.COM";
        var extensions = pathExt.Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        var pathDirs = pathEnv.Split(
            Path.PathSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        foreach (var dir in pathDirs)
        {
            if (dir.Length == 0)
            {
                continue;
            }

            if (Path.HasExtension(command))
            {
                var candidate = Path.Combine(dir, command);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                continue;
            }

            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir, command + ext);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            var bare = Path.Combine(dir, command);
            if (File.Exists(bare))
            {
                return bare;
            }
        }

        return null;
    }
}
