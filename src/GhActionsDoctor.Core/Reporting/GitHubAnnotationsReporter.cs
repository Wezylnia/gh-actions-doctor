using System.Text;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Reporting;

public sealed class GitHubAnnotationsReporter
{
    public string Render(ScanResult result)
    {
        var builder = new StringBuilder();

        foreach (var finding in result.Findings)
        {
            var command = finding.Severity switch
            {
                RuleSeverity.Error => "error",
                RuleSeverity.Warning => "warning",
                _ => "notice"
            };

            var file = finding.FilePath;
            var line = finding.Line?.ToString() ?? "";
            var col = finding.Column?.ToString() ?? "";
            var title = finding.RuleId;
            var message = Escape($"{finding.Message} Suggestion: {finding.Suggestion}");

            builder.Append($"::{command} file={Escape(file)}");

            if (!string.IsNullOrEmpty(line))
            {
                builder.Append($",line={line}");
            }

            if (!string.IsNullOrEmpty(col))
            {
                builder.Append($",col={col}");
            }

            builder.Append($",title={Escape(title)}::{message}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        return value
            .Replace("%", "%25")
            .Replace("\r", "%0D")
            .Replace("\n", "%0A")
            .Replace(":", "%3A")
            .Replace(",", "%2C");
    }
}
