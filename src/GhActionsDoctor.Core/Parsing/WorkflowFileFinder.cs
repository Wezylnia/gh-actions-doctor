namespace GhActionsDoctor.Core.Parsing;

public static class WorkflowFileFinder
{
    private static readonly string[] WorkflowPatterns = ["*.yml", "*.yaml"];

    public static IReadOnlyList<string> Find(string path)
    {
        if (File.Exists(path))
        {
            return IsWorkflowFile(path) ? [Path.GetFullPath(path)] : [];
        }

        if (!Directory.Exists(path))
        {
            return [];
        }

        return WorkflowPatterns
            .SelectMany(pattern => Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsWorkflowFile(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".yml", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".yaml", StringComparison.OrdinalIgnoreCase);
    }
}
