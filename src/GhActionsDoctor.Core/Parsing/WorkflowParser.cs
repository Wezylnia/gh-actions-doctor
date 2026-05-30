using GhActionsDoctor.Core.Models;
using GhActionsDoctor.Core.Yaml;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Parsing;

public sealed class WorkflowParser
{
    public WorkflowFile Parse(string filePath)
    {
        var rawText = File.ReadAllText(filePath);

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(rawText));

            if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new WorkflowFile(
                    filePath,
                    rawText,
                    Root: null,
                    Name: null,
                    ParseError: CreateParseError(filePath, "Workflow YAML must contain a mapping at the document root."));
            }

            var name = root.GetScalarValue("name");
            return new WorkflowFile(filePath, rawText, root, name, ParseError: null);
        }
        catch (YamlException exception)
        {
            return new WorkflowFile(
                filePath,
                rawText,
                Root: null,
                Name: null,
                ParseError: CreateParseError(
                    filePath,
                    exception.Message,
                    exception.Start.Line > 0 ? (int)exception.Start.Line : null,
                    exception.Start.Column > 0 ? (int)exception.Start.Column : null));
        }
    }

    private static Finding CreateParseError(string filePath, string problem, int? line = null, int? column = null)
    {
        return new Finding(
            RuleId: "yaml-parse-error",
            FilePath: filePath,
            Severity: RuleSeverity.Error,
            Category: RuleCategory.Correctness,
            Message: $"Failed to parse workflow file: {problem}",
            Suggestion: "Check YAML indentation and syntax.",
            Line: line,
            Column: column);
    }
}
