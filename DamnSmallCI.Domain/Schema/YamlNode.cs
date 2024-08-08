using System;
using FunicularSwitch.Generators;

namespace DamnSmallCI.Domain.Schema;

[UnionType]
public abstract partial record YamlNode
{
    public record MapNode_(Map<string, YamlNode> Map) : YamlNode;

    public record ListNode_(Lst<YamlNode> List) : YamlNode;

    public record StringNode_(string Value) : YamlNode;
}
