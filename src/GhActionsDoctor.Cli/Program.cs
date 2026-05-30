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

        if (!TryParseScanOptions(args.Skip(1).ToArray(), out var options, out var error))
        {
            Console.Error.WriteLine(error);
            return 2;
        }

        var scanner = new WorkflowScanner();
        var result = scanner.Scan(options);
        var output = options.Format == OutputFormat.Json
            ? new JsonReporter().Render(result)
            : new TextReporter().Render(result);

        Console.WriteLine(output);
        return CalculateExitCode(result, options.FailOn);
    }

    private static bool TryParseScanOptions(string[] args, out ScanOptions options, out string error)
    {
        var path = ScanOptions.Default.Path;
        var format = OutputFormat.Text;
        RuleSeverity? failOn = RuleSeverity.Error;
        var strict = false;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "--path":
                    if (!TryReadValue(args, ref index, arg, out path, out error))
                    {
                        options = ScanOptions.Default;
                        return false;
                    }
                    break;
                case "--format":
                    if (!TryReadValue(args, ref index, arg, out var formatValue, out error)
                        || !TryParseFormat(formatValue, out format))
                    {
                        options = ScanOptions.Default;
                        error = "Invalid --format value. Supported values: text, json.";
                        return false;
                    }
                    break;
                case "--fail-on":
                    if (!TryReadValue(args, ref index, arg, out var failOnValue, out error)
                        || !TryParseFailOn(failOnValue, out failOn))
                    {
                        options = ScanOptions.Default;
                        error = "Invalid --fail-on value. Supported values: error, warning, info, none.";
                        return false;
                    }
                    break;
                case "--include":
                    if (!TryReadValue(args, ref index, arg, out var includeValue, out error))
                    {
                        options = ScanOptions.Default;
                        return false;
                    }
                    AddCsv(include, includeValue);
                    break;
                case "--exclude":
                    if (!TryReadValue(args, ref index, arg, out var excludeValue, out error))
                    {
                        options = ScanOptions.Default;
                        return false;
                    }
                    AddCsv(exclude, excludeValue);
                    break;
                case "--strict":
                    strict = true;
                    break;
                default:
                    options = ScanOptions.Default;
                    error = $"Unknown option: {arg}";
                    return false;
            }
        }

        options = new ScanOptions(path, format, failOn, strict, include, exclude);
        error = string.Empty;
        return true;
    }

    private static bool TryReadValue(string[] args, ref int index, string option, out string value, out string error)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            value = string.Empty;
            error = $"Missing value for {option}.";
            return false;
        }

        index++;
        value = args[index];
        error = string.Empty;
        return true;
    }

    private static bool TryParseFormat(string value, out OutputFormat format)
    {
        if (string.Equals(value, "json", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.Json;
            return true;
        }

        if (string.Equals(value, "text", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.Text;
            return true;
        }

        format = OutputFormat.Text;
        return false;
    }

    private static bool TryParseFailOn(string value, out RuleSeverity? severity)
    {
        if (string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
        {
            severity = null;
            return true;
        }

        if (Enum.TryParse<RuleSeverity>(value, ignoreCase: true, out var parsed))
        {
            severity = parsed;
            return true;
        }

        severity = RuleSeverity.Error;
        return false;
    }

    private static void AddCsv(HashSet<string> target, string value)
    {
        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            target.Add(item);
        }
    }

    private static int CalculateExitCode(ScanResult result, RuleSeverity? failOn)
    {
        if (failOn is null)
        {
            return 0;
        }

        return result.Findings.Any(finding => finding.Severity >= failOn) ? 1 : 0;
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
