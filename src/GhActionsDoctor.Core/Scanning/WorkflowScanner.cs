using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Parsing;
using GhActionsDoctor.Core.Rules;

namespace GhActionsDoctor.Core.Scanning;

public sealed class WorkflowScanner
{
    private readonly WorkflowParser _parser;
    private readonly RuleRunner _ruleRunner;

    public WorkflowScanner(WorkflowParser? parser = null, RuleRunner? ruleRunner = null)
    {
        _parser = parser ?? new WorkflowParser();
        _ruleRunner = ruleRunner ?? new RuleRunner();
    }

    public ScanResult Scan(ScanOptions options)
    {
        var files = WorkflowFileFinder.Find(options.Path);
        var workflows = files.Select(_parser.Parse).ToArray();
        var findings = _ruleRunner.Run(workflows, options);

        // Apply inline suppressions
        var inlineSuppressions = new Dictionary<string, InlineSuppressions>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                inlineSuppressions[file] = InlineSuppressionParser.Parse(content);
            }
            catch
            {
                inlineSuppressions[file] = InlineSuppressions.Empty;
            }
        }

        var activeFindings = new List<Finding>();
        var suppressedFindings = new List<SuppressedFinding>();
        foreach (var finding in findings)
        {
            if (inlineSuppressions.TryGetValue(finding.FilePath, out var suppressions) &&
                suppressions.GetSuppressionSource(finding) is { } source)
            {
                suppressedFindings.Add(new SuppressedFinding(
                    finding.RuleId,
                    finding.FilePath,
                    finding.Line,
                    finding.Column,
                    finding.Severity,
                    finding.Category,
                    finding.Message,
                    source));
                continue;
            }

            activeFindings.Add(finding);
        }

        return new ScanResult(workflows.Length, activeFindings, suppressedFindings);
    }
}
