using System.CommandLine;

namespace DamnSmallCI.Cli.Commands;

internal class RunCommand : Command
{
    public RunCommand() : base("run", "Runs the defined pipeline")
    {
        Add(Commands.Arguments.PipelineFile);
    }
}
