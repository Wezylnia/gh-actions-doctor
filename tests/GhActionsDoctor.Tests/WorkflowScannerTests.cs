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
    public void Json_report_uses_expected_summary_shape()
    {
        WriteWorkflow("build.yml", MinimalWorkflow("CI"));

        var result = Scan("--include", "missing-permissions");
        var json = new JsonReporter().Render(result);

        Assert.Contains("\"summary\"", json);
        Assert.Contains("\"filesScanned\": 1", json);
        Assert.Contains("\"ruleId\": \"missing-permissions\"", json);
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
