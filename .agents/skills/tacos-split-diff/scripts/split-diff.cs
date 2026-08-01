#!/usr/bin/env dotnet
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:include split-diff-common.cs
#:include split-diff-diff.cs
#:include split-diff-analyze.cs
#:include split-diff-git.cs
#:include split-diff-slice-plan.cs
#:include split-diff-verify.cs

var command = "";
string? diffFile = null;
string? baseFile = null;
string? hunksArg = null;
string? outputPath = null;
string? featureRef = null;
string? presentationRef = null;
string? mergeBaseRef = null;
string? trunkRef = null;
string? slicePlanPath = null;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "analyze" or "reconstruct" or "verify-tip" or "verify-slices":
            command = args[i];
            break;
        case "--trunk-ref" when i + 1 < args.Length:
            trunkRef = args[++i];
            break;
        case "--diff-file" when i + 1 < args.Length:
            diffFile = Path.GetFullPath(args[++i]);
            break;
        case "--base-file" when i + 1 < args.Length:
            baseFile = Path.GetFullPath(args[++i]);
            break;
        case "--hunks" when i + 1 < args.Length:
            hunksArg = args[++i];
            break;
        case "--output" when i + 1 < args.Length:
            outputPath = Path.GetFullPath(args[++i]);
            break;
        case "--feature" when i + 1 < args.Length:
            featureRef = args[++i];
            break;
        case "--presentation" when i + 1 < args.Length:
            presentationRef = args[++i];
            break;
        case "--merge-base" when i + 1 < args.Length:
            mergeBaseRef = args[++i];
            break;
        case "--slice-plan" when i + 1 < args.Length:
            slicePlanPath = Path.GetFullPath(args[++i]);
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

if (string.IsNullOrEmpty(command))
{
    Console.Error.WriteLine(
        "Missing subcommand: analyze | reconstruct | verify-tip | verify-slices"
    );
    PrintUsage();
    Environment.Exit(2);
}

try
{
    switch (command)
    {
        case "analyze":
            AnalyzeCommands.RunAnalyze(diffFile);
            break;
        case "reconstruct":
            if (baseFile is null || diffFile is null)
            {
                Console.Error.WriteLine("reconstruct requires --base-file and --diff-file");
                PrintUsage();
                Environment.Exit(2);
            }
            AnalyzeCommands.RunReconstruct(baseFile, diffFile, hunksArg, outputPath);
            break;
        case "verify-tip":
            if (featureRef is null || presentationRef is null)
            {
                Console.Error.WriteLine("verify-tip requires --feature and --presentation");
                PrintUsage();
                Environment.Exit(2);
            }
            VerifyCommands.RunVerifyTip(featureRef, presentationRef, mergeBaseRef, slicePlanPath);
            break;
        case "verify-slices":
            if (slicePlanPath is null || presentationRef is null)
            {
                Console.Error.WriteLine("verify-slices requires --slice-plan and --presentation");
                PrintUsage();
                Environment.Exit(2);
            }
            VerifyCommands.RunVerifySlices(slicePlanPath, presentationRef, trunkRef);
            break;
    }
}
catch (SplitDiffException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

static void PrintUsage()
{
    Console.Error.WriteLine(
        """
        Usage (run from tacos-split-diff skill directory):
          dotnet run scripts/split-diff.cs analyze [--diff-file PATH]
          dotnet run scripts/split-diff.cs reconstruct --base-file PATH --diff-file PATH [--hunks 0,2] [--output PATH]
          dotnet run scripts/split-diff.cs verify-tip --feature REF --presentation REF [--merge-base REF] [--slice-plan PATH]
          dotnet run scripts/split-diff.cs verify-slices --slice-plan PATH --presentation REF [--trunk-ref REF]

        analyze        Parse a unified diff (stdin or --diff-file); emit JSON change-block index.
        reconstruct    Apply selected 0-based change blocks from a single-file diff onto --base-file.
        verify-tip     Fail unless presentation branch tip tree matches feature branch; optional slice-plan file coverage.
        verify-slices  Fail unless each presentation commit matches slice-plan incremental and cumulative paths.

        Implementation: split-diff.cs + split-diff-*.cs (#:include).
        """
    );
}
