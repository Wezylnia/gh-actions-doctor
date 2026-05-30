using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class MissingTimeoutRule : IWorkflowRule
{
    public string Id => "missing-timeout";

    public RuleCategory Category => RuleCategory.Reliability;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var (jobName, job, jobKey) in workflow.Root!.GetNamedJobMappingEntries())
            {
                if (!job.HasChild("timeout-minutes"))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        $"Job '{jobName}' does not define timeout-minutes.",
                        "Add a timeout-minutes value that reflects the expected maximum runtime for this job.",
                        locationNode: jobKey));
                }
            }
        }

        return findings;
    }
}
