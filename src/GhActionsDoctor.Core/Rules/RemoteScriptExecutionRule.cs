using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class RemoteScriptExecutionRule : IWorkflowRule
{
    public string Id => "remote-script-execution";
    public RuleCategory Category => RuleCategory.Security;
    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    private static readonly string[] DangerousPatterns =
    [
        "curl ", "wget ", "irm ", "iwr "
    ];

    private static readonly string[] PipeTargets = ["| sh", "| bash", "| iex"];

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var step in workflow.Root!.GetRunSteps())
            {
                if (step.Run is null)
                    continue;

                var isDangerous = DangerousPatterns.Any(d => step.Run.Contains(d, StringComparison.OrdinalIgnoreCase)) &&
                    PipeTargets.Any(p => step.Run.Contains(p, StringComparison.OrdinalIgnoreCase));

                if (!isDangerous)
                    continue;

                if (step.Run.Contains(" -o ", StringComparison.OrdinalIgnoreCase) ||
                    step.Run.Contains(" -O ", StringComparison.OrdinalIgnoreCase) ||
                    step.Run.Contains("OutFile", StringComparison.OrdinalIgnoreCase) ||
                    step.Run.Contains("Out-File", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                findings.Add(RuleHelpers.Finding(this, workflow,
                    "This step pipes a remote script directly to a shell interpreter.",
                    "Download scripts to a file, verify integrity or pin a version, then execute only trusted content.",
                    RuleSeverity.Warning, step.Node));
            }
        }

        return findings;
    }
}
