using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class SetupNodeCacheMissingRule : IWorkflowRule
{
    public string Id => "setup-node-cache-missing";

    public RuleCategory Category => RuleCategory.Performance;

    public RuleSeverity DefaultSeverity => RuleSeverity.Info;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var step in workflow.Root!.GetSteps())
            {
                var uses = step.GetScalarValue("uses");
                if (uses is null || !uses.StartsWith("actions/setup-node@", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (step.GetMapping("with")?.HasChild("cache") != true)
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        "actions/setup-node is used without dependency caching.",
                        "Add a cache value such as npm, yarn, or pnpm when the workflow installs Node.js dependencies."));
                }
            }
        }

        return findings;
    }
}
