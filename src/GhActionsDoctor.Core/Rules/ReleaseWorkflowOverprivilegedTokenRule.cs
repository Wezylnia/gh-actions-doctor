using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class ReleaseWorkflowOverprivilegedTokenRule : IWorkflowRule
{
    public string Id => "release-workflow-overprivileged-token";
    public RuleCategory Category => RuleCategory.Security;
    public RuleSeverity DefaultSeverity => RuleSeverity.Warning;

    private static readonly string[] LegitimateReleaseActions =
    [
        "softprops/action-gh-release",
        "gh release",
        "dotnet nuget push",
        "npm publish",
        "docker buildx build --push",
        "docker/login-action"
    ];

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            var root = workflow.Root!;
            if (!HasReleaseTrigger(root))
                continue;

            var hasWriteAll = GrantsWriteAll(root);
            var hasContentsWrite = HasTopLevelWrite(root, "contents");
            var hasPackagesWrite = HasTopLevelWrite(root, "packages");
            var hasLegitimatePublish = HasLegitimatePublishStep(root);

            if (hasWriteAll)
            {
                findings.Add(RuleHelpers.Finding(this, workflow,
                    "Workflow triggered by release events grants write-all permissions.",
                    "Limit permissions to the minimum required. If publishing releases, use `contents: write` scoped to the release job.",
                    RuleSeverity.Warning, root));
            }
            else if (hasContentsWrite && !hasLegitimatePublish)
            {
                findings.Add(RuleHelpers.Finding(this, workflow,
                    "Workflow triggered by release events has `contents: write` without a clear release/publish step.",
                    "Add a release publishing step or limit permissions to `contents: read`.",
                    RuleSeverity.Warning, root));
            }
            else if (hasPackagesWrite && !hasLegitimatePublish)
            {
                findings.Add(RuleHelpers.Finding(this, workflow,
                    "Workflow triggered by release events has `packages: write` without a package publishing step.",
                    "Add a package publishing step or limit permissions to `packages: read`.",
                    RuleSeverity.Warning, root));
            }
        }

        return findings;
    }

    private static bool HasReleaseTrigger(YamlMappingNode root)
    {
        if (RuleHelpers.HasTrigger(root, "release") || RuleHelpers.HasTrigger(root, "workflow_dispatch"))
            return true;
        var onNode = root.GetChild("on") as YamlMappingNode;
        if (onNode?.GetChild("push") is YamlMappingNode push)
        {
            if (push.GetChild("tags") is YamlSequenceNode tags)
            {
                return tags.Children.OfType<YamlScalarNode>().Any(t => t.Value?.StartsWith("v") == true);
            }
        }

        return false;
    }

    private static bool GrantsWriteAll(YamlMappingNode root) =>
        root.GetChild("permissions") is YamlScalarNode s && s.Value?.Equals("write-all", StringComparison.OrdinalIgnoreCase) == true;

    private static bool HasTopLevelWrite(YamlMappingNode root, string scope) =>
        root.GetMapping("permissions")?.GetChild(scope) is YamlScalarNode s && s.Value?.Equals("write", StringComparison.OrdinalIgnoreCase) == true;

    private static bool HasLegitimatePublishStep(YamlMappingNode root)
    {
        foreach (var step in root.GetUsesSteps())
        {
            if (step.Uses is not null && LegitimateReleaseActions.Any(a => step.Uses.StartsWith(a, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        foreach (var step in root.GetRunSteps())
        {
            if (step.Run is not null && LegitimateReleaseActions.Any(a => step.Run.Contains(a, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }
}
