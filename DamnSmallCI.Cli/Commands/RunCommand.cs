using System.CommandLine;

namespace DamnSmallCI.Cli.Commands;

internal class RunCommand : Command
{
    public RunCommand() : base("run", "Runs the defined pipeline")
    {
        Add(Commands.Options.PipelineFile);
        Add(Commands.Options.EnvironmentFile);
        Add(Commands.Arguments.ContextDirectory);
    }
}
