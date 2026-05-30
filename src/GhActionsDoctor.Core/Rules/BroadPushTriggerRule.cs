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
            if (IsBroadPush(onNode))
            {
                findings.Add(RuleHelpers.Finding(
                    this,
                    workflow,
                    "Workflow runs on every push without branch, tag, or path filters.",
                    "Add branch, tag, or path filters if this workflow does not need to run for every push."));
            }
        }

        return findings;
    }

    private static bool IsBroadPush(YamlNode? onNode)
    {
        return onNode switch
        {
            YamlScalarNode scalar => string.Equals(scalar.Value, "push", StringComparison.OrdinalIgnoreCase),
            YamlSequenceNode sequence => sequence.Children
                .OfType<YamlScalarNode>()
                .Any(node => string.Equals(node.Value, "push", StringComparison.OrdinalIgnoreCase)),
            YamlMappingNode mapping when mapping.GetChild("push") is { } push =>
                push.IsNullLike()
                || push is YamlMappingNode pushMapping && !FilterKeys.Any(pushMapping.HasChild),
            _ => false
        };
    }
}
