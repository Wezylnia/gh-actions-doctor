using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class PullRequestTargetUntrustedCheckoutRule : IWorkflowRule
{
    public string Id => "pull-request-target-untrusted-checkout";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Error;

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

            foreach (var (step, uses, usesNode) in root.GetUsesSteps())
            {
                if (!uses.StartsWith("actions/checkout@", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var reference = step.GetMapping("with")?.GetScalarValue("ref");
                if (IsUntrustedPullRequestReference(reference))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        "pull_request_target checks out untrusted pull request head code.",
                        "Do not check out pull request head code in pull_request_target workflows. Use pull_request or keep checkout on the trusted base repository.",
                        locationNode: usesNode));
                }
            }
        }

        return findings;
    }

    private static bool IsUntrustedPullRequestReference(string? reference) =>
        reference is not null && (
            reference.Contains("github.event.pull_request.head.sha", StringComparison.OrdinalIgnoreCase) ||
            reference.Contains("github.head_ref", StringComparison.OrdinalIgnoreCase));
}
