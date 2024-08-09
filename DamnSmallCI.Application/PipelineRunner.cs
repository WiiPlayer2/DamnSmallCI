using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class PipelineRunner<RT>(TaskRunner<RT> taskRunner) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(IProgress<PipelineOutput> outputProgress, PipelineInfo pipeline) =>
        from taskOutput in SuccessEff(new Progress<TaskOutput>(x => outputProgress.Report(PipelineOutput.From(x.Value))))
        from _05 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE START] ======"))))
        from _10 in pipeline.Tasks
            .Select(x => taskRunner.Run(taskOutput, x))
            .TraverseSerial(identity)
        from _20 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE END] ======"))))
        select unit;
}
