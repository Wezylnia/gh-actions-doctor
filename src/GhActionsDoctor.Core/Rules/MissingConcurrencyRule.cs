using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class MissingConcurrencyRule : IWorkflowRule
{
    public string Id => "missing-concurrency";

    public RuleCategory Category => RuleCategory.Cost;

    public RuleSeverity DefaultSeverity => RuleSeverity.Info;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            var root = workflow.Root!;
            if (root.HasChild("concurrency"))
            {
                continue;
            }

            var name = workflow.Name ?? Path.GetFileNameWithoutExtension(workflow.FilePath);
            var likelyUseful = RuleHelpers.HasTrigger(root, "pull_request")
                || RuleHelpers.HasTrigger(root, "pull_request_target")
                || RuleHelpers.HasTrigger(root, "push")
                || name.Contains("deploy", StringComparison.OrdinalIgnoreCase)
                || name.Contains("release", StringComparison.OrdinalIgnoreCase);

            if (likelyUseful)
            {
                findings.Add(RuleHelpers.Finding(
                    this,
                    workflow,
                    "Workflow does not define concurrency.",
                    "Consider adding concurrency with a group based on github.workflow and github.ref, and cancel-in-progress for CI workflows.",
                    locationNode: root));
            }
        }

        return findings;
    }
}
