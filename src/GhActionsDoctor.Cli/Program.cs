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
        var output = options.Format == OutputFormat.Json
            ? new JsonReporter().Render(result)
            : new TextReporter().Render(result);

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
          --format <text|json>          Output format. Defaults to text.
          --fail-on <error|warning|info|none>
                                        Controls non-zero exit code. Defaults to error.
          --include <rule-id,...>       Run only selected rules.
          --exclude <rule-id,...>       Skip selected rules.
          --strict                      Promote selected security findings.
        """);
    }
}
