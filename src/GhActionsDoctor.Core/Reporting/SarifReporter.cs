using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Scanning;

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

        var results = result.Findings.Select(finding =>
        {
            var fingerprint = ComputeFingerprint(finding);
            return new Dictionary<string, object>
            {
                ["ruleId"] = finding.RuleId,
                ["level"] = ToSarifLevel(finding.Severity),
                ["message"] = new Dictionary<string, object>
                {
                    ["text"] = $"{finding.Message} Suggestion: {finding.Suggestion}"
                },
                ["locations"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["physicalLocation"] = new Dictionary<string, object>
                        {
                            ["artifactLocation"] = new Dictionary<string, object>
                            {
                                ["uri"] = finding.FilePath.Replace('\\', '/')
                            },
                            ["region"] = new Dictionary<string, object>
                            {
                                ["startLine"] = finding.Line ?? 1,
                                ["startColumn"] = finding.Column ?? 1
                            }
                        }
                    }
                },
                ["partialFingerprints"] = new Dictionary<string, string>
                {
                    ["primaryLocationLineHash"] = ComputeLineHash(finding),
                    ["ghActionsDoctorFingerprint"] = fingerprint
                },
                ["properties"] = new Dictionary<string, object>
                {
                    ["category"] = finding.Category.ToString().ToLowerInvariant()
                }
            };
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

    private static string ComputeFingerprint(Finding finding)
    {
        var input = string.Join("|",
            finding.RuleId.ToLowerInvariant(),
            finding.FilePath.Replace('\\', '/').ToLowerInvariant(),
            finding.Category.ToString().ToLowerInvariant(),
            finding.Message.ToLowerInvariant());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    private static string ComputeLineHash(Finding finding)
    {
        var input = $"{finding.FilePath}:{finding.Line ?? 0}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
