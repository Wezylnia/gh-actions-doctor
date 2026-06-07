using GhActionsDoctor.Cli.Commands;
using GhActionsDoctor.Core.Fixing;
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

        if (string.Equals(args[0], "fix", StringComparison.OrdinalIgnoreCase))
        {
            return RunFix(args.Skip(1).ToArray());
        }

        if (!string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Unknown command: {args[0]}");
            PrintHelp();
            return 1;
        }

        var parsedOptions = new ScanOptionsParser().Parse(args.Skip(1).ToArray());
        if (parsedOptions.HelpRequested)
        {
            PrintHelp();
            return 0;
        }

        if (!parsedOptions.Success)
        {
            Console.Error.WriteLine(parsedOptions.Error);
            return 2;
        }

        var options = parsedOptions.Options!;
        var scanner = new WorkflowScanner();
        var result = scanner.Scan(options);
        var resultBeforeBaseline = result;

        // Prune baseline before applying baseline suppression.
        if (parsedOptions.PruneBaseline)
        {
            if (string.IsNullOrWhiteSpace(options.BaselinePath) || options.BaselinePath.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine("--prune-baseline requires a baseline file path via --baseline or config.");
                return 2;
            }

            try
            {
                var resolvedBaselinePath = Path.GetFullPath(Path.IsPathRooted(options.BaselinePath)
                    ? options.BaselinePath
                    : Path.Combine(Directory.GetCurrentDirectory(), options.BaselinePath));
                BaselineSuppressor.Prune(resultBeforeBaseline.Findings, resolvedBaselinePath);
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"Baseline file not found: {ex.Message}");
                return 2;
            }
        }

        // Apply baseline suppression
        if (options.BaselinePath is not null && !options.BaselinePath.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var resolvedBaselinePath = Path.GetFullPath(Path.IsPathRooted(options.BaselinePath)
                    ? options.BaselinePath
                    : Path.Combine(Directory.GetCurrentDirectory(), options.BaselinePath));
                result = BaselineSuppressor.ApplyWithTracking(result, resolvedBaselinePath);
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
            OutputFormat.Json => new JsonReporter().Render(result, options.ShowSuppressions),
            OutputFormat.GitHubAnnotations => new GitHubAnnotationsReporter().Render(result),
            OutputFormat.Sarif => new SarifReporter().Render(result),
            _ => new TextReporter().Render(result, options.ShowSuppressions)
        };

        Console.WriteLine(output);
        return ExitCodeCalculator.Calculate(result, options.FailOn);
    }

    private static int RunFix(string[] args)
    {
        var parsed = new FixOptionsParser().Parse(args);
        if (parsed.HelpRequested)
        {
            PrintHelp();
            return 0;
        }

        if (!parsed.Success)
        {
            Console.Error.WriteLine(parsed.Error);
            return 2;
        }

        var options = parsed.Options!;
        var result = new WorkflowFixer().Fix(options.Path, options.Apply, options.Rules);
        foreach (var message in result.Messages)
        {
            Console.WriteLine(options.Apply ? $"Applied: {message}" : $"Would apply: {message}");
        }

        if (result.FixCount == 0)
        {
            Console.WriteLine("No safe fixes found.");
        }

        if (result.HasInvalidWorkflow)
        {
            return 3;
        }

        return options.Apply || result.FixCount == 0 ? 0 : 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
        GitHub Actions Doctor

        Usage:
          gh-actions-doctor scan [options]
          gh-actions-doctor fix [options]

        Scan options:
          --path <path>                 Workflow directory or file. Defaults to ./.github/workflows.
          --format <text|json|github-annotations|sarif>          Output format. Defaults to text.
          --fail-on <error|warning|info|none>
                                        Controls non-zero exit code. Defaults to error.
          --include <rule-id,...>       Run only selected rules.
          --exclude <rule-id,...>       Skip selected rules.
          --strict                      Promote selected security findings.
          --config <path|none>          Config file. Defaults to .gh-actions-doctor.yml if present.
          --baseline <path|none>        Baseline file for suppressing known findings.
          --write-baseline <path>       Write current findings to a baseline file.
          --prune-baseline              Remove stale entries from the baseline file.
          --show-suppressions           Include suppressed findings in output.

        Fix options:
          --path <path>                 Workflow directory or file. Defaults to ./.github/workflows.
          --dry-run                     Print safe fixes without changing files. Default.
          --apply                       Apply safe fixes.
          --rule <rule-id,...>          Only apply selected fix rules (missing-timeout, missing-permissions).
        """);
    }
}
