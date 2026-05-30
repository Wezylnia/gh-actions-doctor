using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class MutableActionReferenceRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Theory]
    [InlineData("owner/action@main")]
    [InlineData("owner/action@master")]
    [InlineData("owner/action@latest")]
    [InlineData("owner/action@dev")]
    [InlineData("owner/action@develop")]
    [InlineData("owner/action@trunk")]
    public void Reports_known_mutable_action_references(string uses)
    {
        var findings = _harness.Analyze(new MutableActionReferenceRule(), WorkflowUsing(uses));

        var finding = Assert.Single(findings);
        Assert.Equal("mutable-action-reference", finding.RuleId);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
        Assert.Equal(8, finding.Line);
    }

    [Theory]
    [InlineData("actions/checkout@v4")]
    [InlineData("owner/action@v1.2.3")]
    [InlineData("./.github/actions/local-action")]
    [InlineData("docker://alpine:3.20")]
    public void Does_not_report_stable_local_or_docker_references(string uses)
    {
        var findings = _harness.Analyze(new MutableActionReferenceRule(), WorkflowUsing(uses));

        Assert.Empty(findings);
    }

    [Fact]
    public void Promotes_to_error_in_strict_mode()
    {
        var findings = _harness.Analyze(
            new MutableActionReferenceRule(),
            WorkflowUsing("owner/action@main"),
            ScanOptions.Default with { Strict = true });

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
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
