using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Scanning;

public static class InlineSuppressionParser
{
    private const string DisableNextLinePrefix = "gh-actions-doctor-disable-next-line";
    private const string DisableFilePrefix = "gh-actions-doctor-disable-file";

    public static InlineSuppressions Parse(string content)
    {
        var disableNextLine = new Dictionary<int, HashSet<string>>();
        var disableFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var commentIndex = line.IndexOf('#');
            if (commentIndex < 0)
            {
                continue;
            }

            var comment = line[(commentIndex + 1)..].Trim();

            if (comment.StartsWith(DisableFilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var ruleId = comment[DisableFilePrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(ruleId))
                {
                    disableFile.Add(ruleId);
                }
            }
            else if (comment.StartsWith(DisableNextLinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var ruleId = comment[DisableNextLinePrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(ruleId))
                {
                    var nextLine = i + 1;
                    while (nextLine < lines.Length && string.IsNullOrWhiteSpace(lines[nextLine]))
                    {
                        nextLine++;
                    }

                    if (nextLine < lines.Length)
                    {
                        var lineNumber = nextLine + 1;
                        if (!disableNextLine.ContainsKey(lineNumber))
                        {
                            disableNextLine[lineNumber] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        disableNextLine[lineNumber].Add(ruleId);
                    }
                }
            }
        }

        return new InlineSuppressions(disableNextLine, disableFile);
    }
}

public sealed record InlineSuppressions(
    IReadOnlyDictionary<int, HashSet<string>> DisableNextLine,
    IReadOnlySet<string> DisableFile)
{
    public bool IsSuppressed(Finding finding)
    {
        return GetSuppressionSource(finding) is not null;
    }

    public string? GetSuppressionSource(Finding finding)
    {
        if (DisableFile.Contains(finding.RuleId))
        {
            return "inline-file";
        }

        if (finding.Line is { } line &&
            DisableNextLine.TryGetValue(line, out var ruleIds) &&
            ruleIds.Contains(finding.RuleId))
        {
            return "inline-next-line";
        }

        return null;
    }

    public static InlineSuppressions Empty { get; } = new(
        new Dictionary<int, HashSet<string>>(),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}
