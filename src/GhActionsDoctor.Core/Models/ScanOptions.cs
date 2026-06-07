namespace GhActionsDoctor.Core.Models;

public sealed record ScanOptions(
    string Path,
    OutputFormat Format,
    RuleSeverity? FailOn,
    bool Strict,
    IReadOnlySet<string> IncludeRules,
    IReadOnlySet<string> ExcludeRules,
    IReadOnlyDictionary<string, RuleSeverity> SeverityOverrides,
    string? BaselinePath = null)
{
    public static ScanOptions Default { get; } = new(
        Path: System.IO.Path.Combine(".", ".github", "workflows"),
        Format: OutputFormat.Text,
        FailOn: RuleSeverity.Error,
        Strict: false,
        IncludeRules: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        ExcludeRules: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        SeverityOverrides: new Dictionary<string, RuleSeverity>(StringComparer.OrdinalIgnoreCase));
}
