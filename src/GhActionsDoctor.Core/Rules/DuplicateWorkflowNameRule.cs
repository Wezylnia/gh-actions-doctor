using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;

namespace GhActionsDoctor.Core.Rules;

public sealed class DuplicateWorkflowNameRule : IWorkflowRule
{
    public string Id => "duplicate-workflow-name";

    public RuleCategory Category => RuleCategory.Maintainability;

    public RuleSeverity DefaultSeverity => RuleSeverity.Info;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        return context.ValidWorkflows()
            .Where(workflow => !string.IsNullOrWhiteSpace(workflow.Name))
            .GroupBy(workflow => workflow.Name!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(workflow => RuleHelpers.Finding(
                this,
                workflow,
                $"Workflow name '{group.Key}' is used by multiple workflow files.",
                "Use unique workflow names so GitHub UI entries and status checks are easier to understand.",
                locationNode: workflow.Root!.GetEntry("name")?.Key)))
            .ToArray();
    }
}
