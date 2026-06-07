using GhActionsDoctor.Core.Models;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Cli.Commands;

internal sealed record ScanConfigLoadResult(ScanConfig Config, string? Error);

internal sealed record ScanConfig(
    string? Path,
    OutputFormat? Format,
    RuleSeverity? FailOn,
    bool FailOnSet,
    bool? Strict,
    IReadOnlySet<string> IncludeRules,
    IReadOnlySet<string> ExcludeRules,
    IReadOnlyDictionary<string, RuleSeverity> SeverityOverrides,
    string? BaselinePath)
{
    public static ScanConfig Empty { get; } = new(
        null,
        null,
        null,
        false,
        null,
        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        new Dictionary<string, RuleSeverity>(StringComparer.OrdinalIgnoreCase),
        null);
}

internal static class ScanConfigLoader
{
    public static ScanConfigLoadResult Load(string? configPath)
    {
        if (configPath?.Equals("none", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new ScanConfigLoadResult(ScanConfig.Empty, null);
        }

        var path = ResolveConfigPath(configPath);
        if (path is null)
        {
            return new ScanConfigLoadResult(ScanConfig.Empty, null);
        }

        if (!File.Exists(path))
        {
            return new ScanConfigLoadResult(ScanConfig.Empty, $"Config file not found: {path}");
        }

        try
        {
            using var reader = File.OpenText(path);
            var yaml = new YamlStream();
            yaml.Load(reader);

            if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new ScanConfigLoadResult(ScanConfig.Empty, "Config file must contain a YAML mapping.");
            }

            return new ScanConfigLoadResult(Parse(root), null);
        }
        catch (Exception exception)
        {
            return new ScanConfigLoadResult(ScanConfig.Empty, $"Config file could not be parsed: {exception.Message}");
        }
    }

    private static string? ResolveConfigPath(string? configPath)
    {
        var root = Directory.GetCurrentDirectory();
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return Path.GetFullPath(Path.IsPathRooted(configPath) ? configPath : Path.Combine(root, configPath));
        }

        var yml = Path.Combine(root, ".gh-actions-doctor.yml");
        if (File.Exists(yml))
        {
            return yml;
        }

        var yaml = Path.Combine(root, ".gh-actions-doctor.yaml");
        return File.Exists(yaml) ? yaml : null;
    }

    private static ScanConfig Parse(YamlMappingNode root)
    {
        string? path = null;
        OutputFormat? format = null;
        RuleSeverity? failOn = null;
        var failOnSet = false;
        bool? strict = null;
        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var severity = new Dictionary<string, RuleSeverity>(StringComparer.OrdinalIgnoreCase);
        string? baselinePath = null;

        foreach (var entry in root.Children)
        {
            var key = Scalar(entry.Key);
            switch (key)
            {
                case "path":
                case "workflowPath":
                    path = Scalar(entry.Value);
                    break;
                case "format":
                    format = ParseEnum<OutputFormat>(Scalar(entry.Value), "format");
                    break;
                case "failOn":
                case "fail-on":
                    failOn = ParseFailOn(Scalar(entry.Value));
                    failOnSet = true;
                    break;
                case "strict":
                    strict = ParseBool(Scalar(entry.Value), "strict");
                    break;
                case "include":
                    AddSequence(include, entry.Value);
                    break;
                case "exclude":
                case "disabledRules":
                    AddSequence(exclude, entry.Value);
                    break;
                case "severity":
                case "severityOverrides":
                    AddSeverityOverrides(severity, entry.Value);
                    break;
                case "baseline":
                    baselinePath = Scalar(entry.Value);
                    break;
            }
        }

        return new ScanConfig(path, format, failOn, failOnSet, strict, include, exclude, severity, baselinePath);
    }

    private static string Scalar(YamlNode node) =>
        node is YamlScalarNode scalar && scalar.Value is not null
            ? scalar.Value
            : throw new InvalidOperationException("Config value must be a scalar.");

    private static T ParseEnum<T>(string value, string name)
        where T : struct =>
        Enum.TryParse<T>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid {name} value: {value}.");

    private static RuleSeverity? ParseFailOn(string value) =>
        value.Equals("none", StringComparison.OrdinalIgnoreCase)
            ? null
            : ParseEnum<RuleSeverity>(value, "failOn");

    private static bool ParseBool(string value, string name) =>
        bool.TryParse(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid {name} value: {value}.");

    private static void AddSequence(HashSet<string> target, YamlNode node)
    {
        if (node is YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
            {
                target.Add(Scalar(child));
            }

            return;
        }

        foreach (var item in Scalar(node).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            target.Add(item);
        }
    }

    private static void AddSeverityOverrides(Dictionary<string, RuleSeverity> target, YamlNode node)
    {
        if (node is not YamlMappingNode mapping)
        {
            throw new InvalidOperationException("severity must be a mapping of rule IDs to severities.");
        }

        foreach (var entry in mapping.Children)
        {
            target[Scalar(entry.Key)] = ParseEnum<RuleSeverity>(Scalar(entry.Value), "severity");
        }
    }
}
