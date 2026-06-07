using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Reporting;
using GhActionsDoctor.Core.Scanning;

namespace GhActionsDoctor.Tests;

public sealed class WorkflowScannerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gh-actions-doctor-tests-{Guid.NewGuid():N}");
    private readonly string _workflows;

    public WorkflowScannerTests()
    {
        _workflows = Path.Combine(_root, ".github", "workflows");
        Directory.CreateDirectory(_workflows);
    }

    [Fact]
    public void Scan_reports_yaml_parse_errors_without_throwing()
    {
        WriteWorkflow("broken.yml", """
        name: Broken
        on:
          push:
        jobs:
          build
            runs-on: ubuntu-latest
        """);

        var result = Scan();

        var finding = Assert.Single(result.Findings);
        Assert.Equal("yaml-parse-error", finding.RuleId);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
    }

    [Fact]
    public void Scan_reports_missing_permissions_and_missing_timeout()
    {
        WriteWorkflow("build.yml", """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var result = Scan("--exclude", "missing-concurrency,broad-push-trigger");

        Assert.Contains(result.Findings, finding => finding.RuleId == "missing-permissions");
        Assert.Contains(result.Findings, finding => finding.RuleId == "missing-timeout");
    }

    [Fact]
    public void Scan_reports_action_reference_and_node_cache_findings()
    {
        WriteWorkflow("node.yml", """
        name: Node
        on:
          pull_request:
        permissions:
          contents: read
        jobs:
          test:
            runs-on: ubuntu-latest
            timeout-minutes: 10
            steps:
              - uses: actions/setup-node@v4
                with:
                  node-version: 20
              - uses: acme/example-action@main
        """);

        var result = Scan("--exclude", "missing-concurrency");

        Assert.Contains(result.Findings, finding => finding.RuleId == "setup-node-cache-missing");
        Assert.Contains(result.Findings, finding => finding.RuleId == "mutable-action-reference");
        Assert.Contains(result.Findings, finding => finding.RuleId == "action-not-sha-pinned");
    }

    [Fact]
    public void Scan_reports_duplicate_workflow_names()
    {
        WriteWorkflow("one.yml", MinimalWorkflow("CI"));
        WriteWorkflow("two.yml", MinimalWorkflow("CI"));

        var result = Scan("--include", "duplicate-workflow-name");

        Assert.Equal(2, result.Findings.Count(finding => finding.RuleId == "duplicate-workflow-name"));
    }

    [Fact]
    public void Scan_applies_inline_next_line_suppression()
    {
        WriteWorkflow("build.yml", """
        name: CI
        on:
          push:
        permissions:
          contents: read
        jobs:
          # gh-actions-doctor-disable-next-line missing-timeout
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var result = Scan("--include", "missing-timeout");

        Assert.DoesNotContain(result.Findings, finding => finding.RuleId == "missing-timeout");
    }

    [Fact]
    public void Json_report_uses_expected_summary_shape()
    {
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));

        var result = Scan("--include", "missing-permissions");
        var json = new JsonReporter().Render(result);

        Assert.Contains("\"summary\"", json);
        Assert.Contains("\"filesScanned\": 1", json);
        Assert.Contains("\"ruleId\": \"missing-permissions\"", json);
    }

    [Fact]
    public void V1Contract_Json_has_top_level_summary_and_findings()
    {
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));
        var result = Scan("--include", "missing-permissions");
        var json = new JsonReporter().Render(result);

        Assert.Contains("\"summary\"", json);
        Assert.Contains("\"findings\"", json);
        Assert.Contains("\"filesScanned\"", json);
        Assert.Contains("\"errors\"", json);
        Assert.Contains("\"warnings\"", json);
        Assert.Contains("\"info\"", json);
    }

    [Fact]
    public void V1Contract_Json_finding_has_all_fields()
    {
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));
        var result = Scan("--include", "missing-permissions");
        var json = new JsonReporter().Render(result);

        Assert.Contains("\"file\"", json);
        Assert.Contains("\"severity\"", json);
        Assert.Contains("\"ruleId\"", json);
        Assert.Contains("\"category\"", json);
        Assert.Contains("\"message\"", json);
        Assert.Contains("\"suggestion\"", json);
    }

    [Fact]
    public void V1Contract_Baseline_prune_creates_backup()
    {
        var baselinePath = Path.Combine(_root, ".gh-actions-doctor-baseline.json");
        File.WriteAllText(baselinePath, """{"version":1,"findings":[]}""");
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));

        BaselineSuppressor.Prune([], baselinePath);

        Assert.True(File.Exists(baselinePath + ".bak"));
    }

    [Fact]
    public void V1Contract_Baseline_prune_keeps_matching_entry()
    {
        var baselinePath = Path.Combine(_root, ".gh-actions-doctor-baseline.json");
        var finding = new Finding("missing-permissions", Path.Combine(_workflows, "build.yml").Replace('\\', '/'), RuleSeverity.Warning, RuleCategory.Security, "msg", "suggestion", 1);
        File.WriteAllText(baselinePath, """{"version":1,"findings":[{"ruleId":"missing-permissions","filePath":"build.yml","message":"msg"}]}""");
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));

        BaselineSuppressor.Prune([finding], baselinePath);

        var content = File.ReadAllText(baselinePath);
        Assert.Contains("missing-permissions", content);
    }

    [Fact]
    public void V1Contract_SARIF_has_version_and_driver()
    {
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));
        var result = Scan("--include", "missing-permissions");

        var sarif = new SarifReporter().Render(result);

        Assert.Contains("\"version\": \"2.1.0\"", sarif);
        Assert.Contains("\"gh-actions-doctor\"", sarif);
        Assert.Contains("\"rules\"", sarif);
        Assert.Contains("\"partialFingerprints\"", sarif);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private ScanResult Scan(params string[] args)
    {
        var options = ScanOptions.Default with
        {
            Path = _workflows,
            IncludeRules = GetRuleSet(args, "--include"),
            ExcludeRules = GetRuleSet(args, "--exclude")
        };

        return new WorkflowScanner().Scan(options);
    }

    private static IReadOnlySet<string> GetRuleSet(string[] args, string option)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var index = Array.IndexOf(args, option);
        if (index < 0 || index + 1 >= args.Length)
        {
            return set;
        }

        foreach (var item in args[index + 1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            set.Add(item);
        }

        return set;
    }

    private void WriteWorkflow(string fileName, string contents)
    {
        File.WriteAllText(Path.Combine(_workflows, fileName), contents);
    }

    private static string MinimalWorkflow(string name)
    {
        return $$"""
        name: {{name}}
        on:
          workflow_dispatch:
        jobs:
          build:
            runs-on: ubuntu-latest
            timeout-minutes: 10
            steps:
              - run: echo ok
        """;
    }
}
