using DamnSmallCi.Utils;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;

namespace DamnSmallCI.Server.Application;

public class TaskDispatcher<RT>(
    IServiceRuntime<RT> serviceRuntime,
    ILogger<TaskDispatcher<RT>> logger) where RT : struct, HasCancel<RT>
{
    public Eff<Unit> Dispatch(
        Aff<RT, Unit> task,
        Option<ILogger> loggerOption = default) =>
        from _10 in EffMaybe(() => task
            .OnError(error => Eff(fun(() => loggerOption.IfNone(logger).LogError(error.ToException(), "Dispatched task failed."))))
            .Fork()
            .Run(serviceRuntime.Runtime))
        select unit;
}
