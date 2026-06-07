using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed class ScanOptionsParser
{
    public ScanOptionsParseResult Parse(string[] args)
    {
        string? path = null;
        OutputFormat? format = null;
        RuleSeverity? failOn = null;
        var failOnSet = false;
        bool? strict = null;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var includeSet = false;
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var excludeSet = false;
        string? configPath = null;

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

                    if (!TryParseFormat(formatValue, out var parsedFormat))
                    {
                        return ScanOptionsParseResult.Fail("Invalid --format value. Supported values: text, json, github-annotations, sarif.");
                    }

                    format = parsedFormat;
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

                    failOnSet = true;
                    break;
                case "--include":
                    if (!TryReadValue(args, ref index, arg, out var includeValue, out var includeError))
                    {
                        return ScanOptionsParseResult.Fail(includeError);
                    }

                    AddCsv(include, includeValue);
                    includeSet = true;
                    break;
                case "--exclude":
                    if (!TryReadValue(args, ref index, arg, out var excludeValue, out var excludeError))
                    {
                        return ScanOptionsParseResult.Fail(excludeError);
                    }

                    AddCsv(exclude, excludeValue);
                    excludeSet = true;
                    break;
                case "--strict":
                    strict = true;
                    break;
                case "--config":
                    if (!TryReadValue(args, ref index, arg, out configPath, out var configError))
                    {
                        return ScanOptionsParseResult.Fail(configError);
                    }

                    break;
                default:
                    return ScanOptionsParseResult.Fail($"Unknown option: {arg}");
            }
        }

        var config = ScanConfigLoader.Load(configPath);
        if (config.Error is not null)
        {
            return ScanOptionsParseResult.Fail(config.Error);
        }

        var options = new ScanOptions(
            path ?? config.Config.Path ?? ScanOptions.Default.Path,
            format ?? config.Config.Format ?? OutputFormat.Text,
            failOnSet ? failOn : config.Config.FailOnSet ? config.Config.FailOn : RuleSeverity.Error,
            strict ?? config.Config.Strict ?? false,
            includeSet ? include : config.Config.IncludeRules,
            excludeSet ? exclude : config.Config.ExcludeRules,
            config.Config.SeverityOverrides);

        return ScanOptionsParseResult.Ok(options);
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

        if (string.Equals(value, "github-annotations", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "githubannotations", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "annotations", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.GitHubAnnotations;
            return true;
        }

        if (string.Equals(value, "sarif", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.Sarif;
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
