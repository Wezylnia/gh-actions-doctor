using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class MissingTimeoutRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_each_job_without_timeout()
    {
        var findings = _harness.Analyze(new MissingTimeoutRule(), """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet build
          test:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        Assert.Equal(2, findings.Count);
        Assert.All(findings, finding => Assert.Equal(RuleSeverity.Warning, finding.Severity));
        Assert.Contains(findings, finding => finding.Message.Contains("'build'", StringComparison.Ordinal) && finding.Line == 5);
        Assert.Contains(findings, finding => finding.Message.Contains("'test'", StringComparison.Ordinal) && finding.Line == 9);
    }

    [Fact]
    public void Does_not_report_jobs_with_timeout()
    {
        var findings = _harness.Analyze(new MissingTimeoutRule(), """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            timeout-minutes: 15
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
