namespace DamnSmallCI.Domain;

public record TaskContainerInfo(
    ImageName Image,
    Option<ContainerEntrypoint> Entrypoint);
