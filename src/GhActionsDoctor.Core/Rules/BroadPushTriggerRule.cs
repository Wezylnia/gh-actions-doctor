using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

public sealed class BroadPushTriggerRule : IWorkflowRule
{
    private static readonly string[] FilterKeys =
    [
        "branches",
        "branches-ignore",
        "paths",
        "paths-ignore",
        "tags",
        "tags-ignore"
    ];

    public string Id => "broad-push-trigger";

    public RuleCategory Category => RuleCategory.Cost;

    public RuleSeverity DefaultSeverity => RuleSeverity.Info;

    public IReadOnlyList<Finding> Analyze(RuleContext context)
    {
        var findings = new List<Finding>();

        foreach (var workflow in context.ValidWorkflows())
        {
            var onNode = workflow.Root!.GetChild("on");
            if (IsBroadPush(onNode, out var locationNode))
            {
                findings.Add(RuleHelpers.Finding(
                    this,
                    workflow,
                    "Workflow runs on every push without branch, tag, or path filters.",
                    "Add branch, tag, or path filters if this workflow does not need to run for every push.",
                    locationNode: locationNode));
            }
        }

        return findings;
    }

    private static bool IsBroadPush(YamlNode? onNode, out YamlNode? locationNode)
    {
        locationNode = null;

        switch (onNode)
        {
            case YamlScalarNode scalar:
                locationNode = scalar;
                return string.Equals(scalar.Value, "push", StringComparison.OrdinalIgnoreCase);
            case YamlSequenceNode sequence:
                var pushNode = sequence.Children
                    .OfType<YamlScalarNode>()
                    .FirstOrDefault(node => string.Equals(node.Value, "push", StringComparison.OrdinalIgnoreCase));
                locationNode = pushNode;
                return pushNode is not null;
            case YamlMappingNode mapping when mapping.GetEntry("push") is { } pushEntry:
                locationNode = pushEntry.Key;
                return pushEntry.Value.IsNullLike()
                    || pushEntry.Value is YamlMappingNode pushMapping && !FilterKeys.Any(pushMapping.HasChild);
            default:
                return false;
        }
    }
}
