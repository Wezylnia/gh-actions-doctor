using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class MissingConcurrencyRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Theory]
    [InlineData("pull_request")]
    [InlineData("push")]
    public void Reports_workflows_with_common_ci_triggers_without_concurrency(string trigger)
    {
        var findings = _harness.Analyze(new MissingConcurrencyRule(), $$"""
        name: CI
        on:
          {{trigger}}:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        Assert.Single(findings);
    }

    [Fact]
    public void Reports_release_named_workflow_without_concurrency()
    {
        var findings = _harness.Analyze(new MissingConcurrencyRule(), """
        name: Release
        on:
          workflow_dispatch:
        jobs:
          publish:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet nuget push
        """);

        Assert.Single(findings);
    }

    [Fact]
    public void Does_not_report_when_concurrency_exists()
    {
        var findings = _harness.Analyze(new MissingConcurrencyRule(), """
        name: CI
        on:
          push:
        concurrency:
          group: ${{ github.workflow }}-${{ github.ref }}
          cancel-in-progress: true
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        Assert.Empty(findings);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }
}
