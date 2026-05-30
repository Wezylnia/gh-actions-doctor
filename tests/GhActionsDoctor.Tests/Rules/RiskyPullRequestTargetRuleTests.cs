using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class RiskyPullRequestTargetRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_warning_when_pull_request_target_is_used_without_known_dangerous_pattern()
    {
        var findings = _harness.Analyze(new RiskyPullRequestTargetRule(), """
        name: Labeler
        on:
          pull_request_target:
        permissions:
          pull-requests: write
        jobs:
          label:
            runs-on: ubuntu-latest
            timeout-minutes: 5
            steps:
              - uses: actions/labeler@v5
        """);

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
    }

    [Fact]
    public void Reports_error_when_pull_request_target_checks_out_code()
    {
        var findings = _harness.Analyze(new RiskyPullRequestTargetRule(), """
        name: Risky
        on:
          pull_request_target:
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/checkout@v4
        """);

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
    }

    [Fact]
    public void Reports_error_when_pull_request_target_runs_scripts_with_write_permissions()
    {
        var findings = _harness.Analyze(new RiskyPullRequestTargetRule(), """
        name: Risky
        on:
          pull_request_target:
        permissions:
          contents: write
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - run: npm test
        """);

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
    }

    [Fact]
    public void Does_not_report_regular_pull_request_workflows()
    {
        var findings = _harness.Analyze(new RiskyPullRequestTargetRule(), """
        name: PR
        on:
          pull_request:
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/checkout@v4
        """);

        Assert.Empty(findings);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }
}
