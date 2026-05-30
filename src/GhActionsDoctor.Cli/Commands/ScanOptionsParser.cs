using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed class ScanOptionsParser
{
    public ScanOptionsParseResult Parse(string[] args)
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
                    if (!TryReadValue(args, ref index, arg, out path, out var pathError))
                    {
                        return ScanOptionsParseResult.Fail(pathError);
                    }

                    break;
                case "--format":
                    if (!TryReadValue(args, ref index, arg, out var formatValue, out var formatError))
                    {
                        return ScanOptionsParseResult.Fail(formatError);
                    }

                    if (!TryParseFormat(formatValue, out format))
                    {
                        return ScanOptionsParseResult.Fail("Invalid --format value. Supported values: text, json.");
                    }

                    break;
                case "--fail-on":
                    if (!TryReadValue(args, ref index, arg, out var failOnValue, out var failOnError))
                    {
                        return ScanOptionsParseResult.Fail(failOnError);
                    }

                    if (!TryParseFailOn(failOnValue, out failOn))
                    {
                        return ScanOptionsParseResult.Fail("Invalid --fail-on value. Supported values: error, warning, info, none.");
                    }

                    break;
                case "--include":
                    if (!TryReadValue(args, ref index, arg, out var includeValue, out var includeError))
                    {
                        return ScanOptionsParseResult.Fail(includeError);
                    }

                    AddCsv(include, includeValue);
                    break;
                case "--exclude":
                    if (!TryReadValue(args, ref index, arg, out var excludeValue, out var excludeError))
                    {
                        return ScanOptionsParseResult.Fail(excludeError);
                    }

                    AddCsv(exclude, excludeValue);
                    break;
                case "--strict":
                    strict = true;
                    break;
                default:
                    return ScanOptionsParseResult.Fail($"Unknown option: {arg}");
            }
        }

        return ScanOptionsParseResult.Ok(new ScanOptions(path, format, failOn, strict, include, exclude));
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
}
