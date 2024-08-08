using LanguageExt;

namespace DamnSmallCI.Domain.Schema;

public enum YamlTypeError
{
    
}

public interface IYamlNode
{
    OptionUnsafe<IYamlNode?> this[string name] { get; }
    
    Validation<YamlTypeError, string> String { get; }
}
