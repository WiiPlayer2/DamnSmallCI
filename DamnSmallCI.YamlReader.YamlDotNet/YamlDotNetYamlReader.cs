using DamnSmallCI.Application;
using LanguageExt.UnsafeValueAccess;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlNodeDto = YamlDotNet.RepresentationModel.YamlNode;
using YamlNode = DamnSmallCI.Domain.Schema.YamlNode;

namespace DamnSmallCI.YamlReader.YamlDotNet;

public class YamlDotNetYamlReader : IYamlReader
{
    public Eff<YamlNode> Read(string content) =>
        from stringReader in Eff(() => new StringReader(content))
        from parser in Eff(() => new Parser(stringReader))
        from yamlStream in Eff(() => new YamlStream())
        from _10 in Eff(fun(() => yamlStream.Load(parser)))
        from yamlDocument in Eff(() => Optional(yamlStream.Documents.SingleOrDefault()))
        let node = MapNode(yamlDocument.Map(x => x.RootNode).ValueUnsafe())
        select node;

    private static YamlNode MapMappingNode(YamlMappingNode mapping) =>
        YamlNode.MapNode(mapping.Children
            .Select(t => ((t.Key as YamlScalarNode)?.Value ?? string.Empty, MapNode(t.Value)))
            .ToMap());


    private static YamlNode MapNode(YamlNodeDto? dto) =>
        dto is null
            ? YamlNode.Null()
            : dto switch
            {
                YamlMappingNode mapping => MapMappingNode(mapping),
                YamlSequenceNode sequence => MapSequenceNode(sequence),
                YamlScalarNode scalar => MapScalarNode(scalar),
                _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, null),
            };

    private static YamlNode MapScalarNode(YamlScalarNode scalar) =>
        YamlNode.StringNode(scalar.Value);

    private static YamlNode MapSequenceNode(YamlSequenceNode sequence) =>
        YamlNode.ListNode(toList(sequence.Children
            .Select(MapNode)));
}
