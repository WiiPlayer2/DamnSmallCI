using LanguageExt;

namespace DamnSmallCI.Domain.Schema;

public record YamlError(YamlNode Node, string Error);