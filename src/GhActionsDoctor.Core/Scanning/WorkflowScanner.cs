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

        return new ScanResult(workflows.Length, findings);
    }
}
