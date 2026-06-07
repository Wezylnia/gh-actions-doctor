namespace GhActionsDoctor.Core.Models;

public sealed record ScanResult(
    int FilesScanned,
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<SuppressedFinding>? SuppressedFindings = null)
{
    public IReadOnlyList<SuppressedFinding> SuppressedFindings { get; init; } = SuppressedFindings ?? [];

    public int Errors => Findings.Count(f => f.Severity == RuleSeverity.Error);

    public int Warnings => Findings.Count(f => f.Severity == RuleSeverity.Warning);

    public int Info => Findings.Count(f => f.Severity == RuleSeverity.Info);
}

public sealed record SuppressedFinding(
    string RuleId,
    string FilePath,
    int? Line,
    int? Column,
    RuleSeverity Severity,
    RuleCategory Category,
    string Message,
    string SuppressionSource);
