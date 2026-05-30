using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Parsing;
using GhActionsDoctor.Core.Rules;

namespace GhActionsDoctor.Tests.TestSupport;

internal sealed class RuleTestHarness : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gh-actions-doctor-rules-{Guid.NewGuid():N}");
    private readonly WorkflowParser _parser = new();

    public RuleTestHarness()
    {
        Directory.CreateDirectory(_root);
    }

    public IReadOnlyList<Finding> Analyze(IWorkflowRule rule, string yaml, ScanOptions? options = null)
    {
        return Analyze(rule, [("workflow.yml", yaml)], options);
    }

    public IReadOnlyList<Finding> Analyze(
        IWorkflowRule rule,
        IReadOnlyList<(string FileName, string Yaml)> workflows,
        ScanOptions? options = null)
    {
        var parsedWorkflows = workflows
            .Select(workflow => _parser.Parse(WriteWorkflow(workflow.FileName, workflow.Yaml)))
            .ToArray();

        return new RuleRunner([rule]).Run(parsedWorkflows, options ?? TestScanOptions());
    }

    public WorkflowFile Parse(string yaml, string fileName = "workflow.yml")
    {
        return _parser.Parse(WriteWorkflow(fileName, yaml));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string WriteWorkflow(string fileName, string yaml)
    {
        var filePath = Path.Combine(_root, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, yaml);
        return filePath;
    }

    private ScanOptions TestScanOptions()
    {
        return ScanOptions.Default with { Path = _root };
    }
}
