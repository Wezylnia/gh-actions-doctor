using System.Text;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Reporting;

public sealed class TextReporter
{
    public string Render(ScanResult result)
    {
        var builder = new StringBuilder();

        builder.AppendLine("GitHub Actions Doctor");
        builder.AppendLine();
        builder.AppendLine($"Scanned workflows: {result.FilesScanned}");
        builder.AppendLine();

        if (result.Findings.Count == 0)
        {
            builder.AppendLine("No findings.");
            builder.AppendLine();
        }
        else
        {
            foreach (var group in result.Findings.GroupBy(finding => finding.FilePath))
            {
                builder.AppendLine(Path.GetFileName(group.Key));

                foreach (var finding in group)
                {
                    var location = finding.Line is not null ? $" line {finding.Line}" : string.Empty;
                    builder.AppendLine($"  [{ToWireValue(finding.Severity)}] {finding.Message} ({finding.RuleId}{location})");
                    builder.AppendLine($"    Suggestion: {finding.Suggestion}");
                }

                builder.AppendLine();
            }
        }

        builder.AppendLine("Summary:");
        builder.AppendLine($"  {result.Errors} errors");
        builder.AppendLine($"  {result.Warnings} warnings");
        builder.AppendLine($"  {result.Info} info");

        return builder.ToString();
    }

    private static string ToWireValue(RuleSeverity severity)
    {
        return severity.ToString().ToLowerInvariant();
    }
}
