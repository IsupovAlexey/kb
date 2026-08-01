#!/usr/bin/env dotnet
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:include ../../tacos-doctor/scripts/redact-secrets.cs

using System.Text;

var inputPath = "";
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--file" when i + 1 < args.Length:
            inputPath = args[++i];
            break;
        case "--help" or "-h":
            PrintUsage();
            Environment.Exit(0);
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[i]}");
            PrintUsage();
            Environment.Exit(1);
            break;
    }
}

string content;
if (string.IsNullOrEmpty(inputPath))
{
    using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
    content = await reader.ReadToEndAsync();
}
else
{
    var fullPath = Path.GetFullPath(inputPath);
    if (!File.Exists(fullPath))
    {
        Console.Error.WriteLine($"File not found: {fullPath}");
        Environment.Exit(1);
    }

    content = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
}

Console.Write(SecretRedaction.RedactSecrets(content));

static void PrintUsage()
{
    Console.WriteLine(
        """
        redact-secrets.cs — scan handoff markdown for secrets-shaped patterns

        Usage:
          dotnet scripts/redact-secrets.cs [--file <path>]

        Reads stdin when --file is omitted. Writes redacted content to stdout.
        Replace matches with [REDACTED] per tacos-handoff/references/redaction.md.
        """
    );
}
