using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Reporting;

public sealed class JsonReporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public string Render(ScanResult result, bool showSuppressions = false)
    {
        var payload = new
        {
            summary = new
            {
                filesScanned = result.FilesScanned,
                errors = result.Errors,
                warnings = result.Warnings,
                info = result.Info
            },
            findings = result.Findings.Select(finding => new
            {
                file = finding.FilePath,
                severity = ToWireValue(finding.Severity),
                ruleId = finding.RuleId,
                category = ToWireValue(finding.Category),
                message = finding.Message,
                suggestion = finding.Suggestion,
                line = finding.Line,
                column = finding.Column
            }),
            suppressedFindings = showSuppressions
                ? result.SuppressedFindings.Select(finding => new
                {
                    file = finding.FilePath,
                    severity = ToWireValue(finding.Severity),
                    ruleId = finding.RuleId,
                    category = ToWireValue(finding.Category),
                    message = finding.Message,
                    line = finding.Line,
                    column = finding.Column,
                    suppressionSource = finding.SuppressionSource
                })
                : null
        };

        return JsonSerializer.Serialize(payload, Options);
    }

    private static string ToWireValue<T>(T value) where T : struct, Enum
    {
        return value.ToString().ToLowerInvariant();
    }
}
