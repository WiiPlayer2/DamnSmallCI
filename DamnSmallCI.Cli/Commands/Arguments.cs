using System.CommandLine;

namespace DamnSmallCI.Cli.Commands;

internal static class Arguments
{
    public static Argument<FileInfo> PipelineFile { get; } = new(
        "pipeline-file",
        () => new FileInfo(".dsci.yaml"),
        "The pipeline file to run"
    );
}
