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

        findings = findings
            .Where(finding =>
                !inlineSuppressions.TryGetValue(finding.FilePath, out var suppressions) ||
                !suppressions.IsSuppressed(finding))
            .ToArray();

        return new ScanResult(workflows.Length, findings);
    }
}
