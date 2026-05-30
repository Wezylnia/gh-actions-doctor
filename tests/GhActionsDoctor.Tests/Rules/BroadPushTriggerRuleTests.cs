using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class BroadPushTriggerRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Theory]
    [InlineData("""
        on: push
        """)]
    [InlineData("""
        on:
          - push
        """)]
    [InlineData("""
        on:
          push:
        """)]
    public void Reports_push_triggers_without_filters(string triggerYaml)
    {
        var findings = _harness.Analyze(new BroadPushTriggerRule(), $$"""
        name: CI
        {{triggerYaml}}
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var finding = Assert.Single(findings);
        Assert.NotNull(finding.Line);
    }

    [Theory]
    [InlineData("branches")]
    [InlineData("branches-ignore")]
    [InlineData("paths")]
    [InlineData("paths-ignore")]
    [InlineData("tags")]
    [InlineData("tags-ignore")]
    public void Does_not_report_push_triggers_with_filters(string filter)
    {
        var findings = _harness.Analyze(new BroadPushTriggerRule(), $$"""
        name: CI
        on:
          push:
            {{filter}}:
              - main
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
