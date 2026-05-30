using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Rules;

public sealed class MutableActionReferenceRule : IWorkflowRule
{
    private static readonly HashSet<string> MutableReferences = new(StringComparer.OrdinalIgnoreCase)
    {
        "main",
        "master",
        "latest",
        "dev",
        "develop",
        "trunk"
    };

    public string Id => "mutable-action-reference";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var uses in RuleHelpers.UsesValues(workflow.Root!))
            {
                if (RuleHelpers.IsLocalOrDockerAction(uses))
                {
                    continue;
                }

                var reference = RuleHelpers.GetActionReference(uses);
                if (reference is not null && MutableReferences.Contains(reference))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        $"Action '{uses}' uses a mutable reference.",
                        "Use a version tag or, preferably for stricter security, pin the action to a full commit SHA.",
                        context.Options.Strict ? RuleSeverity.Error : null));
                }
            }
        }

        return findings;
    }
}
