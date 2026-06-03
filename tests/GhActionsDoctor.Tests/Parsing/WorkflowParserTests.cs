using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Parsing;

namespace GhActionsDoctor.Tests.Parsing;

public sealed class WorkflowParserTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gh-actions-doctor-parser-{Guid.NewGuid():N}");
    private readonly WorkflowParser _parser = new();

    public WorkflowParserTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Parse_reads_workflow_name_from_valid_mapping_document()
    {
        var path = WriteFile("ci.yml", """
        name: CI
        on:
          push:
        jobs:
          build:
            runs-on: ubuntu-latest
            steps:
              - run: dotnet test
        """);

        var workflow = _parser.Parse(path);

        Assert.True(workflow.IsValid);
        Assert.Equal("CI", workflow.Name);
        Assert.Null(workflow.ParseError);
        Assert.NotNull(workflow.Root);
    }

    [Fact]
    public void Parse_returns_yaml_parse_error_for_invalid_yaml()
    {
        var path = WriteFile("broken.yml", """
        name: Broken
        jobs:
          build
            runs-on: ubuntu-latest
        """);

        var workflow = _parser.Parse(path);

        Assert.False(workflow.IsValid);
        Assert.Null(workflow.Root);

        Assert.NotNull(workflow.ParseError);
        var finding = workflow.ParseError;
        Assert.Equal("yaml-parse-error", finding.RuleId);
        Assert.Equal(RuleSeverity.Error, finding.Severity);
        Assert.Equal(RuleCategory.Correctness, finding.Category);
        Assert.Equal(path, finding.FilePath);
        Assert.NotNull(finding.Line);
        Assert.NotNull(finding.Column);
    }

    [Fact]
    public void Parse_returns_yaml_parse_error_for_non_mapping_root()
    {
        var path = WriteFile("list.yml", """
        - name: CI
        - jobs: {}
        """);

        var workflow = _parser.Parse(path);

        Assert.False(workflow.IsValid);

        Assert.NotNull(workflow.ParseError);
        var finding = workflow.ParseError;
        Assert.Equal("yaml-parse-error", finding.RuleId);
        Assert.Contains("mapping at the document root", finding.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string WriteFile(string fileName, string contents)
    {
        var path = Path.Combine(_root, fileName);
        File.WriteAllText(path, contents);
        return path;
    }
}
