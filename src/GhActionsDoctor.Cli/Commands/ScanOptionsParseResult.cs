using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public sealed record ScanOptionsParseResult(ScanOptions? Options, string? Error)
{
    public bool Success => Options is not null;

    public static ScanOptionsParseResult Ok(ScanOptions options)
    {
        return new ScanOptionsParseResult(options, Error: null);
    }

    public static ScanOptionsParseResult Fail(string error)
    {
        return new ScanOptionsParseResult(Options: null, error);
    }
}
