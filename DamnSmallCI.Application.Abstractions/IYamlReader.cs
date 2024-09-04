using DamnSmallCI.Domain.Schema;

namespace DamnSmallCI.Application;

public interface IYamlReader
{
    Eff<YamlNode> Read(string content);
}
