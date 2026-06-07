using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class OverbroadIdTokenPermissionRule : IWorkflowRule
{
    public string Id => "overbroad-id-token-permission";

    public RuleCategory Category => RuleCategory.Security;

    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            var root = workflow.Root!;
            if (HasIdTokenWrite(root.GetMapping("permissions"), out var topLevelNode) && !WorkflowUsesOidc(root))
            {
                findings.Add(RuleHelpers.Finding(
                    this,
                    workflow,
                    "Workflow grants id-token: write but no step appears to use OIDC authentication or signing.",
                    "Remove id-token: write or limit it to the job that performs cloud authentication or artifact signing.",
                    locationNode: topLevelNode));
            }

            foreach (var (_, job, jobKey) in root.GetNamedJobMappingEntries())
            {
                if (HasIdTokenWrite(job.GetMapping("permissions"), out var jobPermissionNode) && !JobUsesOidc(job))
                {
                    findings.Add(RuleHelpers.Finding(
                        this,
                        workflow,
                        "Job grants id-token: write but no step appears to use OIDC authentication or signing.",
                        "Remove id-token: write from this job or add it only where OIDC is actually required.",
                        locationNode: jobPermissionNode ?? jobKey));
                }
            }
        }

        return findings;
    }

    private static bool HasIdTokenWrite(YamlMappingNode? permissions, out YamlNode? locationNode)
    {
        var entry = permissions?.GetEntry("id-token");
        locationNode = entry?.Key;
        return entry?.Value is YamlScalarNode scalar &&
            string.Equals(scalar.Value, "write", StringComparison.OrdinalIgnoreCase);
    }

    private static bool WorkflowUsesOidc(YamlMappingNode root) =>
        root.GetJobMappings().Any(JobUsesOidc);

    private static bool JobUsesOidc(YamlMappingNode job)
    {
        if (job.GetChild("steps") is not YamlSequenceNode steps)
        {
            return false;
        }

        return steps.Children.OfType<YamlMappingNode>().Any(step =>
            IsKnownOidcAction(step.GetScalarValue("uses")) ||
            ContainsKnownOidcCommand(step.GetScalarValue("run")));
    }

    private static bool IsKnownOidcAction(string? uses) =>
        uses is not null && (
            uses.StartsWith("aws-actions/configure-aws-credentials@", StringComparison.OrdinalIgnoreCase) ||
            uses.StartsWith("google-github-actions/auth@", StringComparison.OrdinalIgnoreCase) ||
            uses.StartsWith("azure/login@", StringComparison.OrdinalIgnoreCase) ||
            uses.StartsWith("sigstore/cosign-installer@", StringComparison.OrdinalIgnoreCase));

    private static bool ContainsKnownOidcCommand(string? run) =>
        run is not null && run.Contains("cosign sign", StringComparison.OrdinalIgnoreCase);
}
