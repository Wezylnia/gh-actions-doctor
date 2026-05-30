using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Models;

public sealed record WorkflowFile(
    string FilePath,
    string RawText,
    YamlMappingNode? Root,
    string? Name,
    Finding? ParseError)
{
    public bool IsValid => Root is not null && ParseError is null;
}
