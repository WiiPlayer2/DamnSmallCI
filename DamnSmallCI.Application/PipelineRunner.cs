using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;

namespace DamnSmallCI.Application;

public class PipelineRunner<RT>(ILogger<PipelineRunner<RT>> logger, TaskRunner<RT> taskRunner) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(PipelineInfo pipeline) =>
        from outputProgress in SuccessEff(new Progress<TaskOutput>(x => logger.LogInformation(x.Value)))
        from _05 in Eff(fun(() => logger.LogInformation("====== [PIPELINE START] ======")))
        from _10 in pipeline.Tasks
            .Select(x => taskRunner.Run(outputProgress, x))
            .TraverseSerial(identity)
        from _20 in Eff(fun(() => logger.LogInformation("====== [PIPELINE END] ======")))
        select unit;
}
