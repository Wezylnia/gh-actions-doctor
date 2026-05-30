namespace GhActionsDoctor.Core.Models;

public sealed record Finding(
    string RuleId,
    string FilePath,
    RuleSeverity Severity,
    RuleCategory Category,
    string Message,
    string Suggestion,
    int? Line = null,
    int? Column = null);
