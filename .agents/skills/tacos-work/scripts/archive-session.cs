#!/usr/bin/env dotnet
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:include ../../tacos-doctor/scripts/redact-secrets.cs
#:include archive-session-distill.cs

using System.Globalization;
using System.Text.Json;

var repoPath = "";
var slug = "";
var archiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
var preview = false;
var write = false;
var jsonFormat = false;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--repo" when i + 1 < args.Length:
            repoPath = Path.GetFullPath(args[++i]);
            break;
        case "--slug" when i + 1 < args.Length:
            slug = args[++i].Trim();
            break;
        case "--date" when i + 1 < args.Length:
            if (
                !DateOnly.TryParseExact(
                    args[++i],
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out archiveDate
                )
            )
            {
                Console.Error.WriteLine("Invalid --date: expected YYYY-MM-DD.");
                Environment.Exit(2);
            }
            break;
        case "--preview":
            preview = true;
            break;
        case "--write":
            write = true;
            break;
        case "--format" when i + 1 < args.Length:
            jsonFormat = args[++i].Equals("json", StringComparison.OrdinalIgnoreCase);
            break;
        case "--help" or "-h":
            PrintUsage();
            Environment.Exit(0);
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            PrintUsage();
            Environment.Exit(2);
            break;
    }
}

if (string.IsNullOrWhiteSpace(repoPath) || string.IsNullOrWhiteSpace(slug))
{
    Console.Error.WriteLine("Missing required --repo and --slug.");
    PrintUsage();
    Environment.Exit(2);
}

if (preview == write)
{
    Console.Error.WriteLine("Specify exactly one of --preview or --write.");
    PrintUsage();
    Environment.Exit(2);
}

if (!TryValidateSessionSlug(slug, out var slugError))
{
    Console.Error.WriteLine(slugError);
    Environment.Exit(2);
}

try
{
    var tacosWorkRoot = Path.GetFullPath(Path.Combine(repoPath, "artifacts", "tacos-work"));
    var tasksPath = Path.GetFullPath(Path.Combine(tacosWorkRoot, slug, "tasks.md"));
    if (!IsPathUnder(tasksPath, tacosWorkRoot))
    {
        Console.Error.WriteLine("Unsafe slug: path escapes artifacts/tacos-work.");
        Environment.Exit(2);
    }
    if (!File.Exists(tasksPath))
    {
        Console.Error.WriteLine($"Session source not found: {tasksPath}");
        Environment.Exit(1);
    }

    var tasksMarkdown = await File.ReadAllTextAsync(tasksPath);
    var result = TacosWorkSessionArchiver.Create(tasksMarkdown, slug, archiveDate, repoPath);

    if (preview)
    {
        EmitResult(result, jsonFormat);
        Environment.Exit(0);
    }

    var targetPath = Path.Combine(
        repoPath,
        result.TargetRelativePath.Replace('/', Path.DirectorySeparatorChar)
    );
    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
    await File.WriteAllTextAsync(targetPath, result.SessionMarkdown);
    EmitResult(result, jsonFormat);
    Environment.Exit(0);
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    Environment.Exit(1);
}

static void EmitResult(ArchiveSessionResult result, bool jsonFormat)
{
    if (jsonFormat)
    {
        var payload = new
        {
            targetPath = result.TargetRelativePath,
            sessionMarkdown = result.SessionMarkdown,
        };
        Console.WriteLine(JsonSerializer.Serialize(payload));
        return;
    }

    Console.WriteLine($"target: {result.TargetRelativePath}");
    Console.WriteLine();
    Console.Write(result.SessionMarkdown);
}

static void PrintUsage()
{
    Console.Error.WriteLine(
        """
        Usage:
          dotnet run scripts/archive-session.cs --repo <path> --slug <slug> --preview [--format json] [--date YYYY-MM-DD]
          dotnet run scripts/archive-session.cs --repo <path> --slug <slug> --write [--format json] [--date YYYY-MM-DD]

        Working directory: the directory containing scripts/archive-session.cs (tacos-work skill root).
        """
    );
}

static bool TryValidateSessionSlug(string slug, out string error)
{
    error = "";
    if (string.IsNullOrWhiteSpace(slug))
    {
        error = "Slug must not be empty.";
        return false;
    }

    if (slug != Path.GetFileName(slug))
    {
        error = "Slug must not contain path separators.";
        return false;
    }

    if (slug is "." or "..")
    {
        error = "Slug must not be '.' or '..'.";
        return false;
    }

    if (slug.Contains("..", StringComparison.Ordinal))
    {
        error = "Slug must not contain '..' segments.";
        return false;
    }

    foreach (var character in slug)
    {
        if (!char.IsLetterOrDigit(character) && character is not ('-' or '_'))
        {
            error = "Slug may only contain letters, digits, hyphens, and underscores.";
            return false;
        }
    }

    return true;
}

static bool IsPathUnder(string fullPath, string rootFullPath)
{
    var normalizedPath = Path.GetFullPath(fullPath);
    var normalizedRoot = Path.GetFullPath(rootFullPath);
    if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar))
    {
        normalizedRoot += Path.DirectorySeparatorChar;
    }

    return normalizedPath.StartsWith(
        normalizedRoot,
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
    );
}
