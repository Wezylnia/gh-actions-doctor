using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Rules;
using GhActionsDoctor.Tests.TestSupport;

namespace GhActionsDoctor.Tests.Rules;

public sealed class MissingPermissionsRuleTests : IDisposable
{
    private readonly RuleTestHarness _harness = new();

    [Fact]
    public void Reports_workflow_without_top_level_permissions()
    {
        var findings = _harness.Analyze(new MissingPermissionsRule(), """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var finding = Assert.Single(findings);
        Assert.Equal("missing-permissions", finding.RuleId);
        Assert.Equal(RuleSeverity.Warning, finding.Severity);
        Assert.Equal(RuleCategory.Security, finding.Category);
    }

    [Fact]
    public void Does_not_report_workflow_with_top_level_permissions()
    {
        var findings = _harness.Analyze(new MissingPermissionsRule(), """
        name: CI
        on:
          push:
        permissions:
          contents: read
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        Assert.Empty(findings);
    }

    [Fact]
    public void Promotes_to_error_in_strict_mode()
    {
        var findings = _harness.Analyze(
            new MissingPermissionsRule(),
            """
            name: CI
            on:
              push:
            jobs:
              build:
                runs-on: ubuntu-latest
                steps:
                  - run: dotnet test
            """,
            ScanOptions.Default with { Strict = true });

        var finding = Assert.Single(findings);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
    }

    public void Dispose()
    {
        _harness.Dispose();
    }
}
