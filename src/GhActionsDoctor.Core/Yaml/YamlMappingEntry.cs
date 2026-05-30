using YamlDotNet.RepresentationModel;

namespace GhActionsDoctor.Core.Yaml;

internal sealed record YamlMappingEntry(YamlNode Key, YamlNode Value);
