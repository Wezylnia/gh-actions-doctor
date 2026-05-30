using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class ActionNotShaPinnedRule : IWorkflowRule
{
    public string Id => "action-not-sha-pinned";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Info;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var (_, uses, usesNode) in workflow.Root!.GetUsesSteps())
            {
                if (RuleHelpers.IsLocalOrDockerAction(uses) || RuleHelpers.IsFirstPartyGitHubAction(uses))
                {
                    continue;
                }

                var reference = RuleHelpers.GetActionReference(uses);
                if (reference is not null && !RuleHelpers.IsFullSha(reference))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        $"Third-party action '{uses}' is not pinned to a full commit SHA.",
                        "Pin third-party actions to a full 40-character commit SHA when you need stricter supply-chain security.",
                        context.Options.Strict ? RuleSeverity.Warning : null,
                        usesNode));
                }
            }
        }

        return findings;
    }
}
