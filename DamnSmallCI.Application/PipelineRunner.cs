using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class PipelineRunner<RT>(IContainerRuntime<RT> containerRuntime, IStepRunner<RT> localStepRunner, TaskRunner<RT> taskRunner) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(IProgress<PipelineOutput> outputProgress, DirectoryInfo contextDirectory, PipelineInfo pipeline) =>
        from taskOutput in SuccessEff(new Progress<TaskOutput>(x => outputProgress.Report(PipelineOutput.From(x.Value))))
        let hasContainerTasks = HasContainerTasks(pipeline)
        from _05 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE START] ======"))))
        from _10 in hasContainerTasks
            ? RunTasksInContainers(taskOutput, contextDirectory, pipeline.Tasks)
            : RunTasksLocally(taskOutput, pipeline.Tasks)
        from _20 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE END] ======"))))
        select unit;

    private bool HasContainerTasks(PipelineInfo pipeline) =>
        pipeline.Tasks.Any(x => x.Image.IsSome);

    private Aff<RT, Unit> RunTaskInContainer(IContainerRuntimeContext<RT> context, IProgress<TaskOutput> outputProgress, TaskInfo task) =>
        from image in task.Image.ToEff($"Image not provided for task \"${task.Name}\"")
        from _10 in use(
            context.NewContainer(image).Map(x => x.WrapSync()),
            containerWrap => taskRunner.Run(containerWrap.Value, outputProgress, task)
        )
        select unit;

    private Aff<RT, Unit> RunTasksInContainers(IProgress<TaskOutput> outputProgress, DirectoryInfo baseDirectory, Lst<TaskInfo> tasks) =>
        use(
            containerRuntime.NewContext().Map(x => x.WrapSync()),
            containerRuntimeContextWrap =>
                from _10 in Eff(fun(() => outputProgress.Report(TaskOutput.From("==== [COPY CONTEXT] ======"))))
                from _20 in containerRuntimeContextWrap.Value.CopyFilesFromDirectory(baseDirectory)
                from _30 in tasks
                    .Select(x => RunTaskInContainer(containerRuntimeContextWrap.Value, outputProgress, x))
                    .TraverseSerial(identity)
                select unit
        );

    private Aff<RT, Unit> RunTasksLocally(IProgress<TaskOutput> outputProgress, Lst<TaskInfo> tasks) =>
        from _10 in tasks
            .Select(x => taskRunner.Run(localStepRunner, outputProgress, x))
            .TraverseSerial(identity)
        select unit;
}
