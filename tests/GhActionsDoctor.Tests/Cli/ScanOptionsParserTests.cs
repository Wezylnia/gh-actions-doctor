using GhActionsDoctor.Cli.Commands;
using GhActionsDoctor.Core.Models;

namespace GhActionsDoctor.Tests.Cli;

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
                "--strict"
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
    }

    [Fact]
    public void Parses_fail_on_none_as_no_failure_threshold()
    {
        var parsed = _parser.Parse(["--fail-on", "none"]);

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.Options);
        Assert.Null(parsed.Options.FailOn);
    }

    [Theory]
    [InlineData("--path")]
    [InlineData("--format")]
    [InlineData("--fail-on")]
    [InlineData("--include")]
    [InlineData("--exclude")]
    public void Rejects_options_missing_a_value(string option)
    {
        var parsed = _parser.Parse([option]);

        Assert.False(parsed.Success);
        Assert.Contains(option, parsed.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_invalid_format()
    {
        var parsed = _parser.Parse(["--format", "sarif"]);

        Assert.False(parsed.Success);
        Assert.Equal("Invalid --format value. Supported values: text, json.", parsed.Error);
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
