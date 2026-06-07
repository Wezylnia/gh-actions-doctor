using System.Text.Json;
using System.Text.Encodings.Web;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Reporting;

public sealed class SarifReporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public string Render(ScanResult result)
    {
        var rules = result.Findings
            .Select(f => f.RuleId)
            .Distinct(StringComparer.Ordinal)
            .Select(ruleId => new
            {
                id = ruleId,
                name = ruleId
            })
            .ToArray();

        var results = result.Findings.Select(finding => new
        {
            ruleId = finding.RuleId,
            level = ToSarifLevel(finding.Severity),
            message = new
            {
                text = $"{finding.Message} Suggestion: {finding.Suggestion}"
            },
            locations = new[]
            {
                new
                {
                    physicalLocation = new
                    {
                        artifactLocation = new
                        {
                            uri = finding.FilePath.Replace('\\', '/')
                        },
                        region = new
                        {
                            startLine = finding.Line ?? 1,
                            startColumn = finding.Column ?? 1
                        }
                    }
                }
            },
            properties = new
            {
                category = finding.Category.ToString().ToLowerInvariant()
            }
        }).ToArray();

        var sarif = new Dictionary<string, object>
        {
            ["version"] = "2.1.0",
            ["$schema"] = "https://json.schemastore.org/sarif-2.1.0.json",
            ["runs"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["tool"] = new Dictionary<string, object>
                    {
                        ["driver"] = new Dictionary<string, object>
                        {
                            ["name"] = "gh-actions-doctor",
                            ["informationUri"] = "https://github.com/Wezylnia/gh-actions-doctor",
                            ["rules"] = rules
                        }
                    },
                    ["results"] = results
                }
            }
        };

        return JsonSerializer.Serialize(sarif, Options);
    }

    private static string ToSarifLevel(RuleSeverity severity) => severity switch
    {
        RuleSeverity.Error => "error",
        RuleSeverity.Warning => "warning",
        _ => "note"
    };
}
