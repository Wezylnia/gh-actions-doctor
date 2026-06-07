using GhActionsDoctor.Cli.Commands;

namespace GhActionsDoctor.Tests.Cli;

public sealed class FixOptionsParserTests
{
    private readonly FixOptionsParser _parser = new();

    [Fact]
    public void Parses_default_fix_options()
    {
        var result = _parser.Parse([]);

        Assert.True(result.Success);
        Assert.Equal(".github/workflows", result.Options!.Path);
        Assert.False(result.Options.Apply);
        Assert.Empty(result.Options.Rules);
    }

    [Fact]
    public void Parses_path()
    {
        var result = _parser.Parse(["--path", "my-workflows"]);

        Assert.True(result.Success);
        Assert.Equal("my-workflows", result.Options!.Path);
    }

    [Fact]
    public void Parses_apply()
    {
        var result = _parser.Parse(["--apply"]);

        Assert.True(result.Success);
        Assert.True(result.Options!.Apply);
    }

    [Fact]
    public void Parses_rule_filter()
    {
        var result = _parser.Parse(["--rule", "missing-timeout,missing-permissions"]);

        Assert.True(result.Success);
        Assert.Contains("missing-timeout", result.Options!.Rules);
        Assert.Contains("missing-permissions", result.Options!.Rules);
    }

    [Fact]
    public void Rejects_unsupported_rule()
    {
        var result = _parser.Parse(["--rule", "unknown-rule"]);

        Assert.False(result.Success);
        Assert.Contains("Unsupported fix rule", result.Error);
    }

    [Fact]
    public void Rejects_both_dry_run_and_apply()
    {
        var result = _parser.Parse(["--dry-run", "--apply"]);

        Assert.False(result.Success);
        Assert.Contains("mutually exclusive", result.Error);
    }

    [Fact]
    public void Rejects_missing_value()
    {
        var result = _parser.Parse(["--path"]);

        Assert.False(result.Success);
        Assert.Contains("Missing value", result.Error);
    }
}
