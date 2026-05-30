using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class ActionNotShaPinnedRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_third_party_action_that_uses_a_tag()
    {
        var findings = _harness.Analyze(new ActionNotShaPinnedRule(), WorkflowUsing("docker/login-action@v3"));

        var finding = Assert.Single(findings);
        Assert.Equal("action-not-sha-pinned", finding.RuleId);
        Assert.Equal(RuleSeverity.Info, finding.Severity);
        Assert.Equal(RuleCategory.Security, finding.Category);
    }

    [Fact]
    public void Does_not_report_first_party_github_actions_using_tags()
    {
        var findings = _harness.Analyze(new ActionNotShaPinnedRule(), WorkflowUsing("actions/checkout@v4"));

        Assert.Empty(findings);
    }

    [Fact]
    public void Does_not_report_third_party_action_pinned_to_full_sha()
    {
        var findings = _harness.Analyze(
            new ActionNotShaPinnedRule(),
            WorkflowUsing("owner/action@0123456789abcdef0123456789abcdef01234567"));

        Assert.Empty(findings);
    }

    [Fact]
    public void Promotes_to_warning_in_strict_mode()
    {
        var findings = _harness.Analyze(
            new ActionNotShaPinnedRule(),
            WorkflowUsing("docker/login-action@v3"),
            ScanOptions.Default with { Strict = true });

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }

    private static string WorkflowUsing(string uses)
    {
        return $$"""
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - uses: {{uses}}
        """;
    }
}
