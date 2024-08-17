namespace DamnSmallCI.Domain;

public record TaskInfo(
    TaskName Name,
    Option<TaskContainerInfo> Container,
    Lst<Step> Steps);
