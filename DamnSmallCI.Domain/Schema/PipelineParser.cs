using LanguageExt;

namespace DamnSmallCI.Domain.Schema;

public class PipelineParser
{
    public static Validation<YamlTypeError, PipelineInfo> Parse(IYamlNode schema)
    {
        return new PipelineInfo(List<TaskInfo>());
    }
}
