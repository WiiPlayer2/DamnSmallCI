using System;
using LanguageExt;

namespace DamnSmallCI.Domain;

public record TaskInfo(
    TaskName Name,
    Lst<Step> Steps);
