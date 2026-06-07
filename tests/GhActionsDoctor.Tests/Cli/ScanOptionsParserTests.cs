using GhActionsDoctor.Cli.Commands;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Tests.Cli;

[Collection("UsesCurrentDirectory")]
public sealed class ScanOptionsParserTests
{
    private readonly ScanOptionsParser _parser = new();

    [Fact]
    public void Parses_default_options()
    {
        var parsed = _parser.Parse([]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Equal(ScanOptions.Default.Path, parsed.Options.Path);
        Assert.Equal(OutputFormat.Text, parsed.Options.Format);
        Assert.Equal(RuleSeverity.Error, parsed.Options.FailOn);
        Assert.False(parsed.Options.Strict);
        Assert.Empty(parsed.Options.IncludeRules);
        Assert.Empty(parsed.Options.ExcludeRules);
    }

    [Fact]
    public void Parses_all_supported_options()
    {
        var parsed = _parser.Parse(
            [
                "--path", "samples/bad",
                "--format", "json",
                "--fail-on", "warning",
                "--include", "missing-permissions,missing-timeout",
                "--exclude", "action-not-sha-pinned",
                "--strict",
                "--baseline", "baseline.json",
                "--write-baseline", "new-baseline.json"
            ]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Equal("samples/bad", parsed.Options.Path);
        Assert.Equal(OutputFormat.Json, parsed.Options.Format);
        Assert.Equal(RuleSeverity.Warning, parsed.Options.FailOn);
        Assert.True(parsed.Options.Strict);
        Assert.Contains("missing-permissions", parsed.Options.IncludeRules);
        Assert.Contains("missing-timeout", parsed.Options.IncludeRules);
        Assert.Contains("action-not-sha-pinned", parsed.Options.ExcludeRules);
        Assert.Equal("baseline.json", parsed.Options.BaselinePath);
        Assert.Equal("new-baseline.json", parsed.WriteBaselinePath);
    }

    [Fact]
    public void Parses_fail_on_none_as_no_failure_threshold()
    {
        var parsed = _parser.Parse(["--fail-on", "none"]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Null(parsed.Options.FailOn);
    }

    [Fact]
    public void Loads_default_config_file_from_current_directory()
    {
        using var directory = new TempWorkingDirectory();
        directory.Write(
            ".gh-actions-doctor.yml",
            """
            path: workflows
            format: json
            failOn: warning
            strict: true
            include:
              - missing-permissions
            severity:
              action-not-sha-pinned: warning
            baseline: baseline.json
            """);

        var parsed = _parser.Parse([]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Equal("workflows", parsed.Options.Path);
        Assert.Equal(OutputFormat.Json, parsed.Options.Format);
        Assert.Equal(RuleSeverity.Warning, parsed.Options.FailOn);
        Assert.True(parsed.Options.Strict);
        Assert.Contains("missing-permissions", parsed.Options.IncludeRules);
        Assert.Equal(RuleSeverity.Warning, parsed.Options.SeverityOverrides["action-not-sha-pinned"]);
        Assert.Equal("baseline.json", parsed.Options.BaselinePath);
    }

    [Fact]
    public void Cli_options_override_config_values()
    {
        using var directory = new TempWorkingDirectory();
        directory.Write(
            ".gh-actions-doctor.yml",
            """
            path: configured
            format: json
            failOn: warning
            include:
              - missing-permissions
            """);

        var parsed = _parser.Parse([
            "--path", "cli",
            "--format", "text",
            "--fail-on", "none",
            "--exclude", "missing-permissions"
        ]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Equal("cli", parsed.Options.Path);
        Assert.Equal(OutputFormat.Text, parsed.Options.Format);
        Assert.Null(parsed.Options.FailOn);
        Assert.Contains("missing-permissions", parsed.Options.IncludeRules);
        Assert.Contains("missing-permissions", parsed.Options.ExcludeRules);
    }

    [Fact]
    public void Config_none_disables_default_config_file()
    {
        using var directory = new TempWorkingDirectory();
        directory.Write(
            ".gh-actions-doctor.yml",
            """
            path: configured
            """);

        var parsed = _parser.Parse(["--config", "none"]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Equal(ScanOptions.Default.Path, parsed.Options.Path);
    }

    [Theory]
    [InlineData("--path")]
    [InlineData("--format")]
    [InlineData("--fail-on")]
    [InlineData("--include")]
    [InlineData("--exclude")]
    [InlineData("--config")]
    [InlineData("--baseline")]
    [InlineData("--write-baseline")]
    public void Rejects_options_missing_a_value(string option)
    {
        var parsed = _parser.Parse([option]);

        Assert.False(parsed.Success);
        Assert.Contains(option, parsed.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_invalid_format()
    {
        var parsed = _parser.Parse(["--format", "xml"]);

        Assert.False(parsed.Success);
        Assert.Equal("Invalid --format value. Supported values: text, json, github-annotations, sarif.", parsed.Error);
    }

    [Fact]
    public void Rejects_invalid_fail_on()
    {
        var parsed = _parser.Parse(["--fail-on", "critical"]);

        Assert.False(parsed.Success);
        Assert.Equal("Invalid --fail-on value. Supported values: error, warning, info, none.", parsed.Error);
    }

    [Fact]
    public void Rejects_unknown_options()
    {
        var parsed = _parser.Parse(["--unknown"]);

        Assert.False(parsed.Success);
        Assert.Equal("Unknown option: --unknown", parsed.Error);
    }
}

[CollectionDefinition("UsesCurrentDirectory", DisableParallelization = true)]
public sealed class UsesCurrentDirectoryCollection
{
}

internal sealed class TempWorkingDirectory : IDisposable
{
    private readonly string previousDirectory = Directory.GetCurrentDirectory();

    public TempWorkingDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gh-actions-doctor-config-" + Guid.NewGuid());
        Directory.CreateDirectory(Path);
        Directory.SetCurrentDirectory(Path);
    }

    public string Path { get; }

    public void Write(string relativePath, string contents)
    {
        var filePath = System.IO.Path.Combine(Path, relativePath);
        var parent = System.IO.Path.GetDirectoryName(filePath);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(filePath, contents);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(previousDirectory);
        Directory.Delete(Path, recursive: true);
    }
}
