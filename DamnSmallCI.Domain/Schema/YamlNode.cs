using FunicularSwitch.Generators;

namespace DamnSmallCI.Domain.Schema;

[UnionType]
public abstract partial record YamlNode
{
    public record List_(Lst<YamlNode> Value) : YamlNode;

    public record Map_(Map<string, YamlNode> Value) : YamlNode;

    public record Null_ : YamlNode;

    public record String_(string Value) : YamlNode;
}
