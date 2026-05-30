using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Rules;

public sealed record RuleContext(IReadOnlyList<WorkflowFile> Workflows, ScanOptions Options);
