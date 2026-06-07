using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed record ScanOptionsParseResult(ScanOptions? Options, string? Error, string? WriteBaselinePath = null, bool PruneBaseline = false, bool HelpRequested = false)
{
    public bool Success => Options is not null || HelpRequested;

    public static ScanOptionsParseResult Ok(ScanOptions options, bool baselineSet = false, string? writeBaselinePath = null, bool pruneBaseline = false, bool helpRequested = false)
    {
        return new ScanOptionsParseResult(options, Error: null, writeBaselinePath, pruneBaseline, helpRequested);
    }

    public static ScanOptionsParseResult Fail(string error)
    {
        return new ScanOptionsParseResult(Options: null, error);
    }
}
