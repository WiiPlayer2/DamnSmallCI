using LanguageExt;

namespace DamnSmallCI.Domain.Schema;

public class PipelineParser
{
    public static Validation<YamlError, PipelineInfo> Parse(YamlNode schema)
    {
        return new PipelineInfo(List<TaskInfo>());
    }
}
