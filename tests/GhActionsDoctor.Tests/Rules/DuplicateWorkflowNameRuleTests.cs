using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class DuplicateWorkflowNameRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_each_workflow_that_reuses_a_name()
    {
        var findings = _harness.Analyze(
            new DuplicateWorkflowNameRule(),
            [
                ("one.yml", WorkflowNamed("CI")),
                ("two.yml", WorkflowNamed("CI")),
                ("deploy.yml", WorkflowNamed("Deploy"))
            ]);

        Assert.Equal(2, findings.Count);
        Assert.All(findings, finding => Assert.Equal("duplicate-workflow-name", finding.RuleId));
    }

    [Fact]
    public void Does_not_report_unique_or_missing_names()
    {
        var findings = _harness.Analyze(
            new DuplicateWorkflowNameRule(),
            [
                ("one.yml", WorkflowNamed("CI")),
                ("two.yml", WorkflowNamed("Deploy")),
                ("unnamed.yml", """
                on:
                  push:
                jobs:
                  build:
                    runs-on: ubuntu-latest
                    steps:
                      - run: echo ok
                """)
            ]);

        Assert.Empty(findings);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }

    private static string WorkflowNamed(string name)
    {
        return $$"""
        name: {{name}}
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: echo ok
        """;
    }
}
