using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class SetupNodeCacheMissingRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_setup_node_without_cache()
    {
        var findings = _harness.Analyze(new SetupNodeCacheMissingRule(), """
        name: Node
        on:
          push:
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/setup-node@v4
                with:
                  node-version: 20
        """);

        Assert.Single(findings);
    }

    [Theory]
    [InlineData("npm")]
    [InlineData("yarn")]
    [InlineData("pnpm")]
    public void Does_not_report_setup_node_with_cache(string cache)
    {
        var findings = _harness.Analyze(new SetupNodeCacheMissingRule(), $$"""
        name: Node
        on:
          push:
        jobs:
          test:
            runs-on: ubuntu-latest
            steps:
              - uses: actions/setup-node@v4
                with:
                  node-version: 20
                  cache: {{cache}}
        """);

        Assert.Empty(findings);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }
}
