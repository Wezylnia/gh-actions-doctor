using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class AdvancedSecurityRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Overbroad_id_token_reports_unused_top_level_permission()
    {
        var findings = _harness.Analyze(new OverbroadIdTokenPermissionRule(), """
        name: CI
        on:
          push:
        permissions:
          contents: read
          id-token: write
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("overbroad-id-token-permission", finding.RuleId);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
    }

    [Fact]
    public void Overbroad_id_token_allows_known_cloud_auth_action()
    {
        var findings = _harness.Analyze(new OverbroadIdTokenPermissionRule(), """
        name: Deploy
        on:
          push:
        permissions:
          contents: read
          id-token: write
        jobs:
          deploy:
            runs-on: ubuntu-latest
            steps:
              - uses: aws-actions/configure-aws-credentials@v5
        """);

        Assert.Empty(findings);
    }

    [Fact]
    public void Pull_request_target_untrusted_checkout_reports_head_sha()
    {
        var findings = _harness.Analyze(new PullRequestTargetUntrustedCheckoutRule(), """
        name: PR
        on:
          pull_request_target:
        jobs:
          review:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/checkout@v6
                with:
                  ref: ${{ github.event.pull_request.head.sha }}
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("pull-request-target-untrusted-checkout", finding.RuleId);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
    }

    [Fact]
    public void Pull_request_target_untrusted_checkout_ignores_default_checkout()
    {
        var findings = _harness.Analyze(new PullRequestTargetUntrustedCheckoutRule(), """
        name: PR
        on:
          pull_request_target:
        jobs:
          review:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/checkout@v6
        """);

        Assert.Empty(findings);
    }

    [Fact]
    public void Untrusted_expression_reports_pull_request_title_in_run()
    {
        var findings = _harness.Analyze(new UntrustedExpressionInRunRule(), """
        name: PR
        on:
          pull_request:
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - run: echo "${{ github.event.pull_request.title }}"
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("untrusted-expression-in-run", finding.RuleId);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }
}
