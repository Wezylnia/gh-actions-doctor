namespace GhActionsDoctor.Cli.Commands;

public sealed class FixOptionsParser
{
    private static readonly HashSet<string> ValidFixableRules = new(StringComparer.OrdinalIgnoreCase)
    {
        "missing-timeout",
        "missing-permissions"
    };

    public FixOptionsParseResult Parse(string[] args)
    {
        var path = ".github/workflows";
        var apply = false;
        var applySet = false;
        var dryRunSet = false;
        var rules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "-h":
                case "--help":
                    return FixOptionsParseResult.Help();
                case "--path":
                    if (!TryReadValue(args, ref index, out path, out var pathError))
                    {
                        return FixOptionsParseResult.Fail(pathError);
                    }

                    break;
                case "--dry-run":
                    dryRunSet = true;
                    if (applySet)
                    {
                        return FixOptionsParseResult.Fail("--dry-run and --apply are mutually exclusive.");
                    }

                    break;
                case "--apply":
                    applySet = true;
                    apply = true;
                    if (dryRunSet)
                    {
                        return FixOptionsParseResult.Fail("--dry-run and --apply are mutually exclusive.");
                    }

                    break;
                case "--rule":
                    if (!TryReadValue(args, ref index, out var ruleValue, out var ruleError))
                    {
                        return FixOptionsParseResult.Fail(ruleError);
                    }

                    foreach (var ruleId in ruleValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (!ValidFixableRules.Contains(ruleId))
                        {
                            return FixOptionsParseResult.Fail($"Unsupported fix rule: {ruleId}. Supported: missing-timeout, missing-permissions.");
                        }

                        rules.Add(ruleId);
                    }

                    break;
                default:
                    return FixOptionsParseResult.Fail($"Unknown option: {args[index]}");
            }
        }

        return FixOptionsParseResult.Ok(new FixOptions(path, apply, rules));
    }

    private static bool TryReadValue(string[] args, ref int index, out string value, out string error)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            value = string.Empty;
            error = $"Missing value for {args[index]}.";
            return false;
        }

        index++;
        value = args[index];
        error = string.Empty;
        return true;
    }
}
