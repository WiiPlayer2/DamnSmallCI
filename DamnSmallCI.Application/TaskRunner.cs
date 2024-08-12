using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class TaskRunner<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(IStepRunner<RT> stepRunner, IProgress<TaskOutput> outputProgress, TaskInfo task) =>
        from _05 in Eff(fun(() => outputProgress.Report(TaskOutput.From($"==== [TASK {task.Name}] ===="))))
        let stepOutput = new Progress<StepOutput>(x => outputProgress.Report(TaskOutput.From(x.Value)))
        from _10 in task.Steps
            .Select(x =>
                from _10 in Eff(fun(() => outputProgress.Report(TaskOutput.From($"+ {x}"))))
                from _20 in stepRunner.Run(stepOutput, x)
                select unit
            )
            .TraverseSerial(identity)
        select unit;
}
