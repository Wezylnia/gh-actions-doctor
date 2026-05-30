namespace GhActionsDoctor.Core.Models;

public sealed record ScanResult(int FilesScanned, IReadOnlyList<Finding> Findings)
{
    public int Errors => Findings.Count(f => f.Severity == RuleSeverity.Error);

    public int Warnings => Findings.Count(f => f.Severity == RuleSeverity.Warning);

    public int Info => Findings.Count(f => f.Severity == RuleSeverity.Info);
}
