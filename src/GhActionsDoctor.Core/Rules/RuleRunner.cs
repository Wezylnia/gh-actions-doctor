using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Rules;

public sealed class RuleRunner
{
    private readonly IReadOnlyList<IWorkflowRule> _rules;

    public RuleRunner(IReadOnlyList<IWorkflowRule>? rules = null)
    {
        _rules = rules ?? RuleCatalog.DefaultRules;
    }

    public IReadOnlyList<Finding> Run(IReadOnlyList<WorkflowFile> workflows, ScanOptions options)
    {
        var findings = new List<Finding>();
        findings.AddRange(workflows.Select(workflow => workflow.ParseError).OfType<Finding>());

        var context = new RuleContext(workflows, options);
        foreach (var rule in _rules.Where(rule => ShouldRun(rule.Id, options)))
        {
            findings.AddRange(rule.Analyze(context).Select(finding => ApplySeverityOverride(finding, options)));
        }

        return findings
            .OrderBy(finding => finding.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(finding => finding.Severity)
            .ThenBy(finding => finding.RuleId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ShouldRun(string ruleId, ScanOptions options)
    {
        if (options.IncludeRules.Count > 0 && !options.IncludeRules.Contains(ruleId))
        {
            return false;
        }

        return !options.ExcludeRules.Contains(ruleId);
    }

    private static Finding ApplySeverityOverride(Finding finding, ScanOptions options) =>
        options.SeverityOverrides.TryGetValue(finding.RuleId, out var severity)
            ? finding with { Severity = severity }
            : finding;
}
