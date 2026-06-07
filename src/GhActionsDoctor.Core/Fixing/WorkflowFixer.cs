using System.Text.RegularExpressions;
using GhActionsDoctor.Core.Parsing;

namespace GhActionsDoctor.Core.Fixing;

public sealed record FixResult(int FixCount, IReadOnlyList<string> ChangedFiles, IReadOnlyList<string> Messages);

public sealed class WorkflowFixer
{
    public FixResult Fix(string path, bool apply)
    {
        var files = WorkflowFileFinder.Find(path);
        var messages = new List<string>();
        var changedFiles = new List<string>();
        var fixCount = 0;

        foreach (var file in files)
        {
            var original = File.ReadAllText(file);
            var updated = original;

            var permissionsFix = AddMissingPermissions(updated);
            if (permissionsFix.Changed)
            {
                updated = permissionsFix.Content;
                fixCount++;
                messages.Add($"{file}: add top-level permissions: contents: read");
            }

            var timeoutFix = AddMissingTimeouts(updated);
            if (timeoutFix.Count > 0)
            {
                updated = timeoutFix.Content;
                fixCount += timeoutFix.Count;
                messages.Add($"{file}: add timeout-minutes: 30 to {timeoutFix.Count} job(s)");
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

        return new FixResult(fixCount, changedFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), messages);
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
