using System.Text.Json;
using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Reporting;

namespace GhActionsDoctor.Tests.Reporting;

public sealed class ReporterTests
{
    [Fact]
    public void JsonReporter_renders_stable_summary_and_finding_shape()
    {
        var result = new ScanResult(
            FilesScanned: 2,
            Findings:
            [
                new Finding(
                    RuleId: "missing-timeout",
                    FilePath: ".github/workflows/ci.yml",
                    Severity: RuleSeverity.Warning,
                    Category: RuleCategory.Reliability,
                    Message: "Job 'build' does not define timeout-minutes.",
                    Suggestion: "Add timeout-minutes.",
                    Line: 12,
                    Column: 9)
            ]);

        var json = new JsonReporter().Render(result);
        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("summary").GetProperty("filesScanned").GetInt32());
        Assert.Equal(0, root.GetProperty("summary").GetProperty("errors").GetInt32());
        Assert.Equal(1, root.GetProperty("summary").GetProperty("warnings").GetInt32());
        Assert.Equal(0, root.GetProperty("summary").GetProperty("info").GetInt32());

        var finding = root.GetProperty("findings")[0];
        Assert.Equal(".github/workflows/ci.yml", finding.GetProperty("file").GetString());
        Assert.Equal("warning", finding.GetProperty("severity").GetString());
        Assert.Equal("missing-timeout", finding.GetProperty("ruleId").GetString());
        Assert.Equal("reliability", finding.GetProperty("category").GetString());
        Assert.Equal(12, finding.GetProperty("line").GetInt32());
        Assert.Equal(9, finding.GetProperty("column").GetInt32());
    }

    [Fact]
    public void TextReporter_renders_no_findings_summary()
    {
        var result = new ScanResult(FilesScanned: 0, Findings: []);

        var output = new TextReporter().Render(result);

        Assert.Contains("GitHub Actions Doctor", output, StringComparison.Ordinal);
        Assert.Contains("Scanned workflows: 0", output, StringComparison.Ordinal);
        Assert.Contains("No findings.", output, StringComparison.Ordinal);
        Assert.Contains("0 errors", output, StringComparison.Ordinal);
        Assert.Contains("0 warnings", output, StringComparison.Ordinal);
        Assert.Contains("0 info", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TextReporter_includes_file_rule_severity_and_line()
    {
        var result = new ScanResult(
            FilesScanned: 1,
            Findings:
            [
                new Finding(
                    RuleId: "missing-permissions",
                    FilePath: ".github/workflows/build.yml",
                    Severity: RuleSeverity.Warning,
                    Category: RuleCategory.Security,
                    Message: "Workflow does not define a top-level permissions block.",
                    Suggestion: "Add permissions: contents: read.",
                    Line: 1)
            ]);

        var output = new TextReporter().Render(result);

        Assert.Contains("build.yml", output, StringComparison.Ordinal);
        Assert.Contains("[warning]", output, StringComparison.Ordinal);
        Assert.Contains("missing-permissions line 1", output, StringComparison.Ordinal);
        Assert.Contains("Add permissions: contents: read.", output, StringComparison.Ordinal);
    }
}
