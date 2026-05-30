using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Rules;

public interface IWorkflowRule
{
    string Id { get; }

    RuleCategory Category { get; }

    RuleSeverity DefaultSeverity { get; }

    IReadOnlyList<Finding> Analyze(RuleContext context);
}
