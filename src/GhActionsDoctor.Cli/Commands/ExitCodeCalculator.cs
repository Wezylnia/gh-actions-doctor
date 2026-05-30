using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Cli.Commands;

public static class ExitCodeCalculator
{
    public static int Calculate(ScanResult result, RuleSeverity? failOn)
    {
        if (failOn is null)
        {
            return 0;
        }

        return result.Findings.Any(finding => finding.Severity >= failOn) ? 1 : 0;
    }
}
