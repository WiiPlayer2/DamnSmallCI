using System.CommandLine;

namespace DamnSmallCI.Cli.Commands;

internal static class Arguments
{
    public static Argument<DirectoryInfo> ContextDirectory { get; } = new(
        "context",
        () => new DirectoryInfo("."),
        "The directory in which the pipeline should be run."
    );
}
