using GhActionsDoctor.Cli.Commands;
using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Reporting;
using GhActionsDoctor.Core.Scanning;

return ProgramMain.Run(args);

internal static class ProgramMain
{
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintHelp();
            return args.Length == 0 ? 1 : 0;
        }

        if (!string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Unknown command: {args[0]}");
            PrintHelp();
            return 1;
        }

        var parsedOptions = new ScanOptionsParser().Parse(args.Skip(1).ToArray());
        if (!parsedOptions.Success)
        {
            Console.Error.WriteLine(parsedOptions.Error);
            return 2;
        }

        var options = parsedOptions.Options!;
        var scanner = new WorkflowScanner();
        var result = scanner.Scan(options);

        // Apply baseline suppression
        if (options.BaselinePath is not null && !options.BaselinePath.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var resolvedBaselinePath = Path.GetFullPath(Path.IsPathRooted(options.BaselinePath)
                    ? options.BaselinePath
                    : Path.Combine(Directory.GetCurrentDirectory(), options.BaselinePath));
                var suppressedFindings = BaselineSuppressor.Apply(result.Findings, resolvedBaselinePath);
                result = new ScanResult(result.FilesScanned, suppressedFindings);
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"Baseline file not found: {ex.Message}");
                return 2;
            }
        }

        // Write baseline
        if (parsedOptions.WriteBaselinePath is not null)
        {
            var resolvedWritePath = Path.GetFullPath(Path.IsPathRooted(parsedOptions.WriteBaselinePath)
                ? parsedOptions.WriteBaselinePath
                : Path.Combine(Directory.GetCurrentDirectory(), parsedOptions.WriteBaselinePath));
            var baseline = BaselineDocument.FromFindings(result.Findings);
            baseline.Save(resolvedWritePath);
        }

        var output = options.Format switch
        {
            OutputFormat.Json => new JsonReporter().Render(result),
            OutputFormat.GitHubAnnotations => new GitHubAnnotationsReporter().Render(result),
            OutputFormat.Sarif => new SarifReporter().Render(result),
            _ => new TextReporter().Render(result)
        };

        Console.WriteLine(output);
        return ExitCodeCalculator.Calculate(result, options.FailOn);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
        GitHub Actions Doctor

        Usage:
          gh-actions-doctor scan [options]

        Options:
          --path <path>                 Workflow directory or file. Defaults to ./.github/workflows.
          --format <text|json|github-annotations|sarif>          Output format. Defaults to text.
          --fail-on <error|warning|info|none>
                                        Controls non-zero exit code. Defaults to error.
          --include <rule-id,...>       Run only selected rules.
          --exclude <rule-id,...>       Skip selected rules.
          --strict                      Promote selected security findings.
          --config <path|none>          Config file. Defaults to .gh-actions-doctor.yml if present.
        """);
    }
}
