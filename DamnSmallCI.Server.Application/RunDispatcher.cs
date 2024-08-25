using DamnSmallCI.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.Server.Application;

public class RunDispatcher<RT>(
    IServiceRuntime<RT> serviceRuntime,
    RunUseCase<RT> runUseCase,
    ILogger<RunDispatcher<RT>> logger) where RT : struct, HasCancel<RT>
{
    public Eff<Unit> Dispatch(
        IContainerRuntime<RT> containerRuntime,
        Environment environment,
        DirectoryInfo contextDirectory,
        FileInfo pipelineFile,
        Aff<RT, Unit> followUp) =>
        from _10 in EffMaybe(() => (
                from _10 in runUseCase
                                .Run(containerRuntime, environment, contextDirectory, pipelineFile)
                            | @catch(error =>
                                from _10 in Eff(fun(() => logger.LogError(error.ToException(), "Run failed.")))
                                from _20 in FailEff<Unit>(error)
                                select unit)
                from _20 in followUp
                select unit
            )
            .Fork()
            .Run(serviceRuntime.Runtime)
        )
        select unit;
}
