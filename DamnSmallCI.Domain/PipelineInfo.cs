using System;
using LanguageExt;

namespace DamnSmallCI.Domain;

public record PipelineInfo(
    Lst<TaskInfo> Tasks);
