using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class RiskyPullRequestTargetRule : IWorkflowRule
{
    public string Id => "risky-pull-request-target";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            var root = workflow.Root!;
            if (!RuleHelpers.HasTrigger(root, "pull_request_target"))
            {
                continue;
            }

            var checksOutCode = RuleHelpers.UsesValues(root)
                .Any(uses => uses.StartsWith("actions/checkout@", StringComparison.OrdinalIgnoreCase));
            var runsScripts = root.GetSteps().Any(step => step.HasChild("run"));
            var grantsWrite = RuleHelpers.GrantsWritePermissions(root);

            var dangerous = checksOutCode || (runsScripts && grantsWrite);
            findings.Add(RuleHelpers.Finding(
                this,
                workflow,
                dangerous
                    ? "pull_request_target is used with a high-risk pattern."
                    : "pull_request_target is used.",
                dangerous
                    ? "Avoid checking out or running pull request code with pull_request_target, especially when secrets or write permissions are available."
                    : "Review whether pull_request is sufficient and keep permissions minimal.",
                dangerous ? RuleSeverity.Error : RuleSeverity.Warning));
        }

        return findings;
    }
}
