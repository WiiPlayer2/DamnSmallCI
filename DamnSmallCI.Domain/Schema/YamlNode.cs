using System;
using FunicularSwitch.Generators;

namespace DamnSmallCI.Domain.Schema;

[UnionType]
public abstract partial record YamlNode
{
    public record MapNode_(Map<string, YamlNode> Map) : YamlNode;
}
