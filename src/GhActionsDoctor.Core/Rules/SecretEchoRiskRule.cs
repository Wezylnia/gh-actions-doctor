using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class SecretEchoRiskRule : IWorkflowRule
{
    public string Id => "secret-echo-risk";
    public RuleCategory Category => RuleCategory.Security;
    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            foreach (var step in workflow.Root!.GetRunSteps())
            {
                if (step.Run is null)
                    continue;
                var run = step.Run;

                var hasSecretRef = run.Contains("${{ secrets.", StringComparison.OrdinalIgnoreCase);
                if (!hasSecretRef)
                    continue;

                var isDangerous = run.Contains("echo ", StringComparison.OrdinalIgnoreCase) ||
                    run.Contains("Write-Host ", StringComparison.OrdinalIgnoreCase) ||
                    run.Contains("printf ", StringComparison.OrdinalIgnoreCase) ||
                    run.Contains("> ", StringComparison.OrdinalIgnoreCase) ||
                    run.Contains(">> ", StringComparison.OrdinalIgnoreCase);

                if (!isDangerous)
                    continue;

                if (run.Contains("::add-mask::", StringComparison.OrdinalIgnoreCase))
                    continue;

                findings.Add(RuleHelpers.Finding(this, workflow,
                    "This step appears to echo or redirect a secret value, which may leak it in logs or files.",
                    "Use `env` to pass secrets safely instead of interpolating them directly in run commands.",
                    RuleSeverity.Warning, step.Node));
            }
        }

        return findings;
    }
}
