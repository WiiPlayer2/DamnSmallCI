using System.CommandLine;
using DamnSmallCI.Domain;

namespace DamnSmallCI.Cli.Commands;

internal static class Options
{
    public static Option<ContainerRuntime> ContainerRuntime { get; } = new(
        ["--container-runtime", "-r"],
        () => Commands.ContainerRuntime.Docker,
        "The container runtime to be used for executing container tasks."
    );

    public static Option<FileInfo?> EnvironmentFile { get; } = new(
        ["--environment-file", "-e"],
        () => default,
        "A file containing environment variables used during execution"
    );

    public static Option<FileInfo?> PipelineFile { get; } = new(
        ["--pipeline-file", "-f"],
        () => default,
        $"The pipeline file to run instead of {DomainConstants.DEFAULT_PIPELINE_FILENAME} inside the context directory."
    );
}
