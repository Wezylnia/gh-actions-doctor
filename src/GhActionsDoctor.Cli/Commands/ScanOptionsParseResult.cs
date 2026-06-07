using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed record ScanOptionsParseResult(ScanOptions? Options, string? Error, string? WriteBaselinePath = null)
{
    public bool Success => Options is not null;

    public static ScanOptionsParseResult Ok(ScanOptions options, bool baselineSet = false, string? writeBaselinePath = null)
    {
        return new ScanOptionsParseResult(options, Error: null, writeBaselinePath);
    }

    public static ScanOptionsParseResult Fail(string error)
    {
        return new ScanOptionsParseResult(Options: null, error);
    }
}
