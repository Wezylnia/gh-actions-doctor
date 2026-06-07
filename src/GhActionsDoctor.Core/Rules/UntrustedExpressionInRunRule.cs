using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class UntrustedExpressionInRunRule : IWorkflowRule
{
    private static readonly string[] RiskyExpressions =
    [
        "github.event.pull_request.title",
        "github.event.pull_request.body",
        "github.event.issue.title",
        "github.event.issue.body",
        "github.event.comment.body"
    ];

    public string Id => "untrusted-expression-in-run";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var step in workflow.Root!.GetSteps())
            {
                var runEntry = step.GetEntry("run");
                if (runEntry?.Value is not YamlScalarNode runNode || string.IsNullOrWhiteSpace(runNode.Value))
                {
                    continue;
                }

                if (RiskyExpressions.Any(expression => runNode.Value.Contains(expression, StringComparison.OrdinalIgnoreCase)))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        "Run step interpolates untrusted GitHub event data directly into shell.",
                        "Move untrusted event data into an environment variable and quote it in the shell command.",
                        locationNode: runEntry.Value));
                }
            }
        }

        return findings;
    }
}
