namespace GhActionsDoctor.Cli.Commands;

public sealed record FixOptionsParseResult(FixOptions? Options, string? Error, bool Success)
{
    public static FixOptionsParseResult Ok(FixOptions options) => new(options, null, true);
    public static FixOptionsParseResult Fail(string error) => new(null, error, false);
}

public sealed record FixOptions(
    string Path,
    bool Apply,
    IReadOnlySet<string> Rules);
