using System.CommandLine;
using DamnSmallCI.Domain;

namespace DamnSmallCI.Cli.Commands;

internal static class Arguments
{
    public static Argument<FileInfo> PipelineFile { get; } = new(
        "pipeline-file",
        () => new FileInfo(DomainConstants.DEFAULT_PIPELINE_FILENAME),
        "The pipeline file to run"
    );
}
