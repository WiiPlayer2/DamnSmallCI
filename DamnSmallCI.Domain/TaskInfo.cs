namespace DamnSmallCI.Domain;

public record TaskInfo(
    TaskName Name,
    Option<ImageName> Image,
    Lst<Step> Steps);
