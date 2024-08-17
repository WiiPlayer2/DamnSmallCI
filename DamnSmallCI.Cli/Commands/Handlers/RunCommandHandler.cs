using System.CommandLine.Invocation;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using DotNetEnv;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.Cli.Commands.Handlers;

internal class RunCommandHandler(ILogger<RunCommand> logger, RunUseCase<Runtime> runUseCase) : ICommandHandler
{
    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken());
        var runtime = Runtime.New(cts);
        return await AffMaybe((Runtime rt) => InvokeAff(context, runUseCase).Run(rt))
            .Run(runtime)
            .Map(x => x.Match(
                _ => 0,
                e =>
                {
                    logger.LogError(e.ToException(), "Failed to run");
                    return e.Code;
                }));
    }

    private Aff<RT, Unit> InvokeAff<RT>(InvocationContext context, RunUseCase<RT> runUseCase) where RT : struct, HasCancel<RT> =>
        from contextDirectory in Eff(() => context.ParseResult.GetValueForArgument(Arguments.ContextDirectory))
        from pipelineFile in Eff(() => context.ParseResult.GetValueForOption(Options.PipelineFile)
                                       ?? new FileInfo(Path.Combine(contextDirectory.FullName, DomainConstants.DEFAULT_PIPELINE_FILENAME)))
        from environmentFileOption in Eff(() => Optional(context.ParseResult.GetValueForOption(Options.EnvironmentFile)))
        from environment in environmentFileOption.Match(
            LoadEnvironmentFile<RT>,
            () => SuccessEff(Environment.Empty))
        from _ in runUseCase.Run(environment, contextDirectory, pipelineFile)
        select unit;

    private Aff<RT, Environment> LoadEnvironmentFile<RT>(FileInfo environmentFile) where RT : struct, HasCancel<RT> =>
        from pairs in Eff(() => Env.Load(environmentFile.FullName, LoadOptions.NoEnvVars()))
        from map in Eff(() => pairs
            .Select(x => (EnvironmentVariableName.From(x.Key), EnvironmentVariableValue.From(x.Value)))
            .ToMap()
        )
        select Environment.From(map);
}
