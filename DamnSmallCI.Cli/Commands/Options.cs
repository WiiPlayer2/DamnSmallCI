using System.CommandLine;
using DamnSmallCI.Domain;

namespace DamnSmallCI.Cli.Commands;

internal static class Options
{
    public static Option<FileInfo?> PipelineFile { get; } = new(
        "pipeline-file",
        () => default,
        $"The pipeline file to run instead of {DomainConstants.DEFAULT_PIPELINE_FILENAME} inside the context directory."
    );
}
