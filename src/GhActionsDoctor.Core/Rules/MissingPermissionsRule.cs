using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class MissingPermissionsRule : IWorkflowRule
{
    public string Id => "missing-permissions";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        return context.ValidWorkflows()
            .Where(workflow => !workflow.Root!.HasChild("permissions"))
            .Select(workflow => RuleHelpers.Finding(
                this,
                workflow,
                "Workflow does not define a top-level permissions block.",
                "Add a top-level permissions block and grant only the permissions required by the workflow.",
                context.Options.Strict ? RuleSeverity.Error : null))
            .ToArray();
    }
}
