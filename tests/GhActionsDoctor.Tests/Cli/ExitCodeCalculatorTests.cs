using GhActionsDoctor.Cli.Commands;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Tests.Cli;

public sealed class ExitCodeCalculatorTests
{
    [Fact]
    public void Returns_zero_when_fail_on_is_none()
    {
        var result = ResultWith(new Finding(
            "rule",
            "workflow.yml",
            RuleSeverity.Error,
            RuleCategory.Security,
            "message",
            "suggestion"));

        Assert.Equal(0, ExitCodeCalculator.Calculate(result, failOn: null));
    }

    [Fact]
    public void Returns_zero_when_no_findings_reach_threshold()
    {
        var result = ResultWith(new Finding(
            "rule",
            "workflow.yml",
            RuleSeverity.Warning,
            RuleCategory.Security,
            "message",
            "suggestion"));

        Assert.Equal(0, ExitCodeCalculator.Calculate(result, RuleSeverity.Error));
    }

    [Theory]
    [InlineData(RuleSeverity.Info, RuleSeverity.Info)]
    [InlineData(RuleSeverity.Warning, RuleSeverity.Info)]
    [InlineData(RuleSeverity.Warning, RuleSeverity.Warning)]
    [InlineData(RuleSeverity.Error, RuleSeverity.Warning)]
    [InlineData(RuleSeverity.Error, RuleSeverity.Error)]
    public void Returns_one_when_a_finding_reaches_threshold(RuleSeverity findingSeverity, RuleSeverity failOn)
    {
        var result = ResultWith(new Finding(
            "rule",
            "workflow.yml",
            findingSeverity,
            RuleCategory.Security,
            "message",
            "suggestion"));

        Assert.Equal(1, ExitCodeCalculator.Calculate(result, failOn));
    }

    private static ScanResult ResultWith(params Finding[] findings)
    {
        return new ScanResult(1, findings);
    }
}
