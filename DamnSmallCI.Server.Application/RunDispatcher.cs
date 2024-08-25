using DamnSmallCI.Application;
using LanguageExt.Effects.Traits;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.Server.Application;

public class RunDispatcher<RT>(
    IServiceRuntime<RT> serviceRuntime,
    RunUseCase<RT> runUseCase) where RT : struct, HasCancel<RT>
{
    public Eff<Unit> Dispatch(IContainerRuntime<RT> containerRuntime, Environment environment, DirectoryInfo contextDirectory, FileInfo pipelineFile) =>
        from _10 in EffMaybe(() => runUseCase
            .Run(containerRuntime, environment, contextDirectory, pipelineFile)
            .Fork()
            .Run(serviceRuntime.Runtime))
        select unit;
}
