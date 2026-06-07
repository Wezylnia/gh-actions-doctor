using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Yaml;

internal static class YamlNodeExtensions
{
    public static YamlMappingEntry? GetEntry(this YamlMappingNode mapping, string key)
    {
        foreach (var child in mapping.Children)
        {
            if (child.Key is YamlScalarNode scalar
                && string.Equals(scalar.Value, key, StringComparison.OrdinalIgnoreCase))
            {
                return new YamlMappingEntry(child.Key, child.Value);
            }
        }

        return null;
    }

    public static YamlNode? GetChild(this YamlMappingNode mapping, string key)
    {
        return mapping.GetEntry(key)?.Value;
    }

    public static bool HasChild(this YamlMappingNode mapping, string key)
    {
        return mapping.GetChild(key) is not null;
    }

    public static string? GetScalarValue(this YamlMappingNode mapping, string key)
    {
        return mapping.GetChild(key) is YamlScalarNode scalar ? scalar.Value : null;
    }

    public static YamlMappingNode? GetMapping(this YamlMappingNode mapping, string key)
    {
        return mapping.GetChild(key) as YamlMappingNode;
    }

    public static IEnumerable<YamlMappingNode> GetJobMappings(this YamlMappingNode root)
    {
        if (root.GetMapping("jobs") is not { } jobs)
        {
            yield break;
        }

        foreach (var job in jobs.Children.Values.OfType<YamlMappingNode>())
        {
            yield return job;
        }
    }

    public static IEnumerable<(string JobName, YamlMappingNode Job)> GetNamedJobMappings(this YamlMappingNode root)
    {
        if (root.GetMapping("jobs") is not { } jobs)
        {
            yield break;
        }

        foreach (var child in jobs.Children)
        {
            if (child.Key is YamlScalarNode key && child.Value is YamlMappingNode job)
            {
                yield return (key.Value ?? "<unnamed>", job);
            }
        }
    }

    public static IEnumerable<(string JobName, YamlMappingNode Job, YamlNode JobKey)> GetNamedJobMappingEntries(this YamlMappingNode root)
    {
        if (root.GetMapping("jobs") is not { } jobs)
        {
            yield break;
        }

        foreach (var child in jobs.Children)
        {
            if (child.Key is YamlScalarNode key && child.Value is YamlMappingNode job)
            {
                yield return (key.Value ?? "<unnamed>", job, child.Key);
            }
        }
    }

    public static IEnumerable<YamlMappingNode> GetSteps(this YamlMappingNode root)
    {
        foreach (var job in root.GetJobMappings())
        {
            if (job.GetChild("steps") is not YamlSequenceNode steps)
            {
                continue;
            }

            foreach (var step in steps.Children.OfType<YamlMappingNode>())
            {
                yield return step;
            }
        }
    }

    public static IEnumerable<(YamlMappingNode Step, string Uses, YamlNode UsesNode)> GetUsesSteps(this YamlMappingNode root)
    {
        foreach (var step in root.GetSteps())
        {
            var usesEntry = step.GetEntry("uses");
            if (usesEntry?.Value is YamlScalarNode usesNode && !string.IsNullOrWhiteSpace(usesNode.Value))
            {
                yield return (step, usesNode.Value, usesNode);
            }
        }
    }

    public static IEnumerable<(YamlMappingNode Node, string Run)> GetRunSteps(this YamlMappingNode root)
    {
        foreach (var step in root.GetSteps())
        {
            var runEntry = step.GetEntry("run");
            if (runEntry?.Value is YamlScalarNode runNode && !string.IsNullOrWhiteSpace(runNode.Value))
            {
                yield return (step, runNode.Value);
            }
        }
    }

    public static (int? Line, int? Column) GetLocation(this YamlNode? node)
    {
        if (node is null || node.Start.Line <= 0)
        {
            return (null, null);
        }

        return ((int)node.Start.Line, node.Start.Column > 0 ? (int)node.Start.Column : null);
    }

    public static bool IsNullLike(this YamlNode node)
    {
        return node is YamlScalarNode scalar && string.IsNullOrWhiteSpace(scalar.Value);
    }
}
