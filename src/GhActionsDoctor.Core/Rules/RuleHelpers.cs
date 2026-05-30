using System.Text.RegularExpressions;
using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Rules;

internal static partial class RuleHelpers
{
    public static IEnumerable<WorkflowFile> ValidWorkflows(this RuleContext context)
    {
        return context.Workflows.Where(workflow => workflow.IsValid);
    }

    public static Finding Finding(
        IWorkflowRule rule,
        WorkflowFile workflow,
        string message,
        string suggestion,
        RuleSeverity? severity = null,
        YamlNode? locationNode = null)
    {
        var (line, column) = locationNode.GetLocation();

        return new Finding(
            rule.Id,
            workflow.FilePath,
            severity ?? rule.DefaultSeverity,
            rule.Category,
            message,
            suggestion,
            line,
            column);
    }

    public static bool HasTrigger(YamlMappingNode root, string triggerName)
    {
        var onNode = root.GetChild("on");
        return onNode switch
        {
            YamlMappingNode mapping => mapping.HasChild(triggerName),
            YamlSequenceNode sequence => sequence.Children
                .OfType<YamlScalarNode>()
                .Any(node => string.Equals(node.Value, triggerName, StringComparison.OrdinalIgnoreCase)),
            YamlScalarNode scalar => string.Equals(scalar.Value, triggerName, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public static IEnumerable<string> UsesValues(YamlMappingNode root)
    {
        return root.GetUsesSteps().Select(step => step.Uses);
    }

    public static bool IsLocalOrDockerAction(string uses)
    {
        return uses.StartsWith("./", StringComparison.Ordinal)
            || uses.StartsWith(".\\", StringComparison.Ordinal)
            || uses.StartsWith("docker://", StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetActionReference(string uses)
    {
        var atIndex = uses.LastIndexOf('@');
        return atIndex >= 0 && atIndex < uses.Length - 1 ? uses[(atIndex + 1)..] : null;
    }

    public static bool IsFullSha(string reference)
    {
        return FullShaRegex().IsMatch(reference);
    }

    public static bool IsFirstPartyGitHubAction(string uses)
    {
        return uses.StartsWith("actions/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool GrantsWritePermissions(YamlMappingNode root)
    {
        if (root.GetChild("permissions") is YamlScalarNode scalar)
        {
            return string.Equals(scalar.Value, "write-all", StringComparison.OrdinalIgnoreCase);
        }

        if (root.GetMapping("permissions") is not { } permissions)
        {
            return false;
        }

        return permissions.Children.Values
            .OfType<YamlScalarNode>()
            .Any(value => string.Equals(value.Value, "write", StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex("^[a-fA-F0-9]{40}$")]
    private static partial Regex FullShaRegex();
}
