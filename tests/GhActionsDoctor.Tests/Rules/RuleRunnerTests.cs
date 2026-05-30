using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class RuleRunnerTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Include_filter_runs_only_selected_rules()
    {
        var findings = _harness.Analyze(
            new CompositeRule(),
            ValidWorkflow(),
            ScanOptions.Default with
            {
                IncludeRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "selected-rule" }
            });

        var finding = Assert.Single(findings);
        Assert.Equal("selected-rule", finding.RuleId);
    }

    [Fact]
    public void Exclude_filter_skips_selected_rules()
    {
        var workflow = _harness.Parse(ValidWorkflow());
        var runner = new RuleRunner([new StaticRule("kept-rule"), new StaticRule("skipped-rule")]);

        var findings = runner.Run(
            [workflow],
            ScanOptions.Default with
            {
                ExcludeRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "skipped-rule" }
            });

        var finding = Assert.Single(findings);
        Assert.Equal("kept-rule", finding.RuleId);
    }

    [Fact]
    public void Parse_errors_are_returned_even_when_no_rule_runs()
    {
        var workflow = _harness.Parse("""
        name: Broken
        jobs:
          build
            runs-on: ubuntu-latest
        """);
        var runner = new RuleRunner([]);

        var findings = runner.Run([workflow], ScanOptions.Default);

        var finding = Assert.Single(findings);
        Assert.Equal("yaml-parse-error", finding.RuleId);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }

    private static string ValidWorkflow()
    {
        return """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """;
    }

    private sealed class CompositeRule : IWorkflowRule
    {
        public string Id => "selected-rule";

        public RuleCategory Category => RuleCategory.Security;

        public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

        public IReadOnlyList<Finding> Analyze(RuleContext context)
        {
            return [CreateFinding("selected-rule")];
        }
    }

    private sealed class StaticRule(string id) : IWorkflowRule
    {
        public string Id { get; } = id;

        public RuleCategory Category => RuleCategory.Security;

        public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

        public IReadOnlyList<Finding> Analyze(RuleContext context)
        {
            return [CreateFinding(Id)];
        }
    }

    private static Finding CreateFinding(string ruleId)
    {
        return new Finding(
            ruleId,
            "workflow.yml",
            RuleSeverity.Warning,
            RuleCategory.Security,
            "Message",
            "Suggestion");
    }
}
