using System.Text.RegularExpressions;
using GhActionsDoctor.Core.Parsing;

namespace GhActionsDoctor.Core.Fixing;

public sealed record FixResult(int FixCount, IReadOnlyList<string> ChangedFiles, IReadOnlyList<string> Messages);

public sealed class WorkflowFixer
{
    public FixResult Fix(string path, bool apply, IReadOnlySet<string>? ruleFilter = null)
    {
        var files = WorkflowFileFinder.Find(path);
        var messages = new List<string>();
        var changedFiles = new List<string>();
        var fixCount = 0;
        var skippedInvalid = new List<string>();
        var skippedComplex = new List<string>();

        var applyPermissions = ruleFilter is null || ruleFilter.Count == 0 || ruleFilter.Contains("missing-permissions");
        var applyTimeout = ruleFilter is null || ruleFilter.Count == 0 || ruleFilter.Contains("missing-timeout");

        foreach (var file in files)
        {
            // Parse-first: skip invalid YAML
            if (!IsValidWorkflow(file))
            {
                skippedInvalid.Add(file);
                continue;
            }

            // Skip complex YAML with anchors/aliases
            var content = File.ReadAllText(file);
            if (content.Contains('&') && Regex.IsMatch(content, @"&\w+\s"))
            {
                skippedComplex.Add(file);
                continue;
            }

            if (content.Contains('*') && content.Contains("<<:"))
            {
                skippedComplex.Add(file);
                continue;
            }

            var original = content;
            var updated = original;

            if (applyPermissions)
            {
                var permissionsFix = AddMissingPermissions(updated);
                if (permissionsFix.Changed)
                {
                    updated = permissionsFix.Content;
                    fixCount++;
                    messages.Add($"{file}: add top-level permissions: contents: read");
                }
            }

            if (applyTimeout)
            {
                var timeoutFix = AddMissingTimeouts(updated);
                if (timeoutFix.Count > 0)
                {
                    updated = timeoutFix.Content;
                    fixCount += timeoutFix.Count;
                    messages.Add($"{file}: add timeout-minutes: 30 to {timeoutFix.Count} job(s)");
                }
            }

            if (!string.Equals(original, updated, StringComparison.Ordinal))
            {
                changedFiles.Add(file);
                if (apply)
                {
                    File.WriteAllText(file, updated);
                }
            }
        }

        foreach (var invalid in skippedInvalid)
        {
            messages.Add($"Skipped invalid YAML: {invalid}");
        }

        foreach (var complex in skippedComplex)
        {
            messages.Add($"Skipped complex YAML: {complex}");
        }

        return new FixResult(fixCount, changedFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), messages);
    }

    private static bool IsValidWorkflow(string path)
    {
        try
        {
            var parser = new WorkflowParser();
            var result = parser.Parse(path);
            return result.IsValid;
        }
        catch
        {
            return false;
        }
    }

    private static (bool Changed, string Content) AddMissingPermissions(string content)
    {
        if (Regex.IsMatch(content, @"(?m)^permissions\s*:"))
        {
            return (false, content);
        }

        var lines = SplitLines(content);
        var jobsIndex = Array.FindIndex(lines, line => Regex.IsMatch(line, @"^jobs\s*:"));
        if (jobsIndex < 0)
        {
            return (false, content);
        }

        var updated = lines.ToList();
        updated.Insert(jobsIndex, "permissions:");
        updated.Insert(jobsIndex + 1, "  contents: read");
        return (true, JoinLines(updated, content));
    }

    private static (int Count, string Content) AddMissingTimeouts(string content)
    {
        var lines = SplitLines(content).ToList();
        var jobsIndex = lines.FindIndex(line => Regex.IsMatch(line, @"^jobs\s*:"));
        if (jobsIndex < 0)
        {
            return (0, content);
        }

        var fixCount = 0;
        for (var index = jobsIndex + 1; index < lines.Count; index++)
        {
            if (!Regex.IsMatch(lines[index], @"^  [A-Za-z0-9_-]+\s*:"))
            {
                continue;
            }

            var jobStart = index;
            var jobEnd = lines.Count;
            for (var scan = jobStart + 1; scan < lines.Count; scan++)
            {
                if (Regex.IsMatch(lines[scan], @"^  [A-Za-z0-9_-]+\s*:"))
                {
                    jobEnd = scan;
                    break;
                }
            }

            if (lines.Skip(jobStart + 1).Take(jobEnd - jobStart - 1).Any(line => Regex.IsMatch(line, @"^\s+timeout-minutes\s*:")))
            {
                continue;
            }

            var runsOnIndex = -1;
            for (var scan = jobStart + 1; scan < jobEnd; scan++)
            {
                if (Regex.IsMatch(lines[scan], @"^    runs-on\s*:"))
                {
                    runsOnIndex = scan;
                    break;
                }
            }

            var insertAt = runsOnIndex >= 0 ? runsOnIndex + 1 : jobStart + 1;
            lines.Insert(insertAt, "    timeout-minutes: 30");
            fixCount++;
            index = insertAt;
        }

        return (fixCount, fixCount == 0 ? content : JoinLines(lines, content));
    }

    private static string[] SplitLines(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

    private static string JoinLines(IReadOnlyList<string> lines, string original) =>
        string.Join(original.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n", lines);
}
