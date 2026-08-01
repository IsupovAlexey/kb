#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false

using System.Diagnostics;

const string DefaultBranch = "master";
const string TacosCommonFolderName = "ServiceTitan";
const string TacosLensCloneFolderName = "tacos-lens";
const string TacosLensSolutionFileName = "TacosLens.slnx";
const string TacosLensRepoUrl = "https://github.com/servicetitan/tacos-lens.git";
const string TacosCommonFolderEnvVar = "TACOS_COMMON_FOLDER";

Environment.Exit(await RunLensRefreshAsync(args));

async Task<int> RunLensRefreshAsync(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--ensure-clone":
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

    var clonePath = ResolveTacosLensClonePath();
    var commonFolder = ResolveTacosCommonFolder();
    Console.WriteLine($"tacos-lens clone path: {clonePath.Replace('\\', '/')}");

    if (!Directory.Exists(clonePath) || IsDirectoryEmpty(clonePath))
    {
        return await CloneLensAsync(commonFolder, clonePath);
    }

    if (IsGitCheckout(clonePath))
    {
        if (!IsTacosLensRoot(clonePath))
        {
            Console.Error.WriteLine(
                $"FAIL {clonePath.Replace('\\', '/')} is a git checkout but is not tacos-lens "
                    + $"({TacosLensSolutionFileName} missing)"
            );
            return 1;
        }

        return await PullLensAsync(clonePath);
    }

    if (IsTacosLensRoot(clonePath))
    {
        Console.WriteLine(
            $"WARN {clonePath.Replace('\\', '/')} contains {TacosLensSolutionFileName} "
                + "but is not a git checkout — removing and re-cloning"
        );
        if (!TryRemoveDirectory(clonePath))
        {
            return 1;
        }

        return await CloneLensAsync(commonFolder, clonePath);
    }

    Console.Error.WriteLine(
        $"FAIL {clonePath.Replace('\\', '/')} exists but is not a tacos-lens checkout "
            + $"({TacosLensSolutionFileName} missing); remove the directory or set {TacosCommonFolderEnvVar}"
    );
    return 1;
}

async Task<int> CloneLensAsync(string commonFolder, string clonePath)
{
    Directory.CreateDirectory(commonFolder);
    Console.WriteLine($"Cloning {TacosLensRepoUrl} into {clonePath.Replace('\\', '/')}");
    var cloneCode = await RunGitAsync(
        ["clone", "--branch", DefaultBranch, TacosLensRepoUrl, clonePath],
        commonFolder
    );
    if (cloneCode != 0)
    {
        Console.Error.WriteLine($"FAIL git clone exited {cloneCode}");
        return cloneCode;
    }

    Console.WriteLine("OK   tacos-lens cloned");
    return 0;
}

async Task<int> PullLensAsync(string clonePath)
{
    var fetchCode = await RunGitAsync(["fetch", "origin"], clonePath);
    if (fetchCode != 0)
    {
        Console.Error.WriteLine($"FAIL git fetch exited {fetchCode}");
        return fetchCode;
    }

    var checkoutCode = await RunGitAsync(["checkout", DefaultBranch], clonePath);
    if (checkoutCode != 0)
    {
        Console.Error.WriteLine($"FAIL git checkout {DefaultBranch} exited {checkoutCode}");
        return checkoutCode;
    }

    var pullCode = await RunGitAsync(["pull", "--ff-only", "origin", DefaultBranch], clonePath);
    if (pullCode != 0)
    {
        Console.Error.WriteLine($"FAIL git pull exited {pullCode}");
        return pullCode;
    }

    Console.WriteLine($"OK   tacos-lens updated on {DefaultBranch}");
    return 0;
}

static async Task<int> RunGitAsync(string[] gitArgs, string workingDirectory)
{
    var executable = ResolveExecutable("git");
    if (executable is null)
    {
        Console.Error.WriteLine("FAIL git not found on PATH");
        return 1;
    }

    var startInfo = new ProcessStartInfo
    {
        FileName = executable,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var arg in gitArgs)
    {
        startInfo.ArgumentList.Add(arg);
    }

    using var process = Process.Start(startInfo);
    if (process is null)
    {
        Console.Error.WriteLine("FAIL could not start git");
        return 1;
    }

    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var stdout = await stdoutTask;
    var stderr = await stderrTask;

    if (!string.IsNullOrWhiteSpace(stdout))
    {
        Console.Write(stdout);
    }

    if (!string.IsNullOrWhiteSpace(stderr))
    {
        Console.Error.Write(stderr);
    }

    return process.ExitCode;
}

static string ExpandUserPath(string path)
{
    var trimmed = path.Trim();
    if (trimmed == "~")
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    if (
        trimmed.StartsWith("~/", StringComparison.Ordinal)
        || trimmed.StartsWith("~\\", StringComparison.Ordinal)
    )
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, trimmed[2..]);
    }

    return Path.GetFullPath(trimmed);
}

static string ResolveTacosCommonFolder()
{
    var fromEnv = Environment.GetEnvironmentVariable(TacosCommonFolderEnvVar);
    if (!string.IsNullOrWhiteSpace(fromEnv))
    {
        return ExpandUserPath(fromEnv.Trim());
    }

    var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    return Path.Combine(profile, TacosCommonFolderName);
}

static string ResolveTacosLensClonePath() =>
    Path.Combine(ResolveTacosCommonFolder(), TacosLensCloneFolderName);

static bool IsDirectoryEmpty(string directoryPath) =>
    !Directory.EnumerateFileSystemEntries(directoryPath).Any();

static bool IsTacosLensRoot(string directoryPath) =>
    File.Exists(Path.Combine(directoryPath, TacosLensSolutionFileName));

static bool IsGitCheckout(string directoryPath)
{
    var gitPath = Path.Combine(directoryPath, ".git");
    return Directory.Exists(gitPath) || File.Exists(gitPath);
}

static bool TryRemoveDirectory(string directoryPath)
{
    try
    {
        Directory.Delete(directoryPath, recursive: true);
        return true;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(
            $"FAIL could not remove {directoryPath.Replace('\\', '/')}: {ex.Message}"
        );
        return false;
    }
}

static string? ResolveExecutable(string command)
{
    var pathEnv = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathEnv))
    {
        return null;
    }

    var extensions = OperatingSystem.IsWindows()
        ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT;.COM").Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        )
        : [string.Empty];

    foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        foreach (var ext in extensions)
        {
            var candidate = Path.Combine(dir, command + ext);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
    }

    return null;
}

void PrintUsage()
{
    Console.WriteLine(
        """
        lens-refresh.cs — refresh shared tacos-lens implementation checkout

        Usage:
          dotnet scripts/lens-refresh.cs

        Clone path:
          <TACOS_COMMON_FOLDER or ~/ServiceTitan>/tacos-lens

        Behavior:
          missing or empty directory — git clone master
          existing git checkout     — git fetch and fast-forward pull master
          broken copy (slnx, no .git) — remove and re-clone

        Exit codes:
          0  Success
          1  Failure (git missing, path conflict, or git command failed)
        """
    );
}
