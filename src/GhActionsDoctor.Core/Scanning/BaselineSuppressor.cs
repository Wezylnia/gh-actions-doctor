using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Core.Scanning;

public sealed record BaselineDocument(
    int Version,
    List<BaselineEntry> Findings)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static BaselineDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Baseline file not found: {path}");
        }

        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<BaselineDocument>(json, Options);
        return document ?? new BaselineDocument(1, []);
    }

    public void Save(string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(this, Options);
        File.WriteAllText(path, json);
    }

    public static BaselineDocument FromFindings(IReadOnlyList<Finding> findings) =>
        new(1, findings.Select(BaselineEntry.FromFinding).ToList());
}

public sealed record BaselineEntry(
    string RuleId,
    string FilePath,
    string Message,
    string? Fingerprint = null)
{
    public static BaselineEntry FromFinding(Finding finding) =>
        new(finding.RuleId, NormalizePath(finding.FilePath), finding.Message,
            ComputeFingerprint(finding));

    public bool Matches(Finding finding)
    {
        if (!string.IsNullOrWhiteSpace(Fingerprint))
        {
            return Fingerprint.Equals(ComputeFingerprint(finding), StringComparison.Ordinal);
        }

        return RuleId.Equals(finding.RuleId, StringComparison.OrdinalIgnoreCase) &&
            PathsMatch(FilePath, finding.FilePath) &&
            Message.Equals(finding.Message, StringComparison.Ordinal);
    }

    public static string ComputeFingerprint(Finding finding)
    {
        var input = string.Join("|",
            finding.RuleId.ToLowerInvariant(),
            NormalizePath(finding.FilePath),
            finding.Category.ToString().ToLowerInvariant(),
            finding.Message.ToLowerInvariant());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    private static bool PathsMatch(string baselinePath, string findingPath)
    {
        var normalizedBaseline = NormalizePath(baselinePath);
        var normalizedFinding = NormalizePath(findingPath);
        if (normalizedBaseline.Equals(normalizedFinding, StringComparison.OrdinalIgnoreCase))
            return true;
        return normalizedFinding.EndsWith("/" + normalizedBaseline, StringComparison.OrdinalIgnoreCase) ||
               normalizedFinding.EndsWith("\\" + normalizedBaseline.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizePath(string path) => path.Replace('\\', '/');
}

public static class BaselineSuppressor
{
    public static IReadOnlyList<Finding> Apply(IReadOnlyList<Finding> findings, string baselinePath)
    {
        var baseline = BaselineDocument.Load(baselinePath);
        var entries = baseline.Findings ?? [];
        return findings.Where(finding => !entries.Any(entry => entry.Matches(finding))).ToArray();
    }

    public static void Prune(IReadOnlyList<Finding> findings, string baselinePath)
    {
        var baseline = BaselineDocument.Load(baselinePath);
        var entries = baseline.Findings ?? [];
        var backupPath = baselinePath + ".bak";
        File.Copy(baselinePath, backupPath, overwrite: true);
        var pruned = entries.Where(entry => findings.Any(f => entry.Matches(f))).ToList();
        new BaselineDocument(baseline.Version, pruned).Save(baselinePath);
    }
}
