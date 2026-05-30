using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Yaml;

internal static class YamlNodeExtensions
{
    public static YamlNode? GetChild(this YamlMappingNode mapping, string key)
    {
        foreach (var child in mapping.Children)
        {
            if (child.Key is YamlScalarNode scalar
                && string.Equals(scalar.Value, key, StringComparison.OrdinalIgnoreCase))
            {
                return child.Value;
            }
        }

        return null;
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

    public static bool IsNullLike(this YamlNode node)
    {
        return node is YamlScalarNode scalar && string.IsNullOrWhiteSpace(scalar.Value);
    }
}
