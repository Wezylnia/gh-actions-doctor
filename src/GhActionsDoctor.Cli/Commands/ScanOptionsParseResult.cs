using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed record ScanOptionsParseResult(ScanOptions? Options, string? Error, string? WriteBaselinePath = null, bool PruneBaseline = false)
{
    public bool Success => Options is not null;

    public static ScanOptionsParseResult Ok(ScanOptions options, bool baselineSet = false, string? writeBaselinePath = null, bool pruneBaseline = false)
    {
        return new ScanOptionsParseResult(options, Error: null, writeBaselinePath, pruneBaseline);
    }

    public static ScanOptionsParseResult Fail(string error)
    {
        return new ScanOptionsParseResult(Options: null, error);
    }
}
