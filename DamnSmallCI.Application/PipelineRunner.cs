using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.Application;

public class PipelineRunner<RT>(IStepRunner<RT> localStepRunner, TaskRunner<RT> taskRunner) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(IContainerRuntime<RT> containerRuntime, IProgress<PipelineOutput> outputProgress, Environment environment, DirectoryInfo contextDirectory, PipelineInfo pipeline) =>
        from taskOutput in SuccessEff(new Progress<TaskOutput>(x => outputProgress.Report(PipelineOutput.From(x.Value))))
        let hasContainerTasks = HasContainerTasks(pipeline)
        from _05 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE START] ======"))))
        from _10 in hasContainerTasks
            ? RunTasksInContainers(containerRuntime, taskOutput, environment, contextDirectory, pipeline.Tasks)
            : RunTasksLocally(taskOutput, environment, pipeline.Tasks)
        from _20 in Eff(fun(() => outputProgress.Report(PipelineOutput.From("====== [PIPELINE END] ======"))))
        select unit;

    private bool HasContainerTasks(PipelineInfo pipeline) =>
        pipeline.Tasks.Any(x => x.Container.IsSome);

    private Aff<RT, Unit> RunTaskInContainer(IContainerRuntimeContext<RT> context, IProgress<TaskOutput> outputProgress, Environment environment, TaskInfo task) =>
        from container in task.Container.ToEff($"Container not provided for task \"${task.Name}\"")
        from _10 in use(
            context.NewContainer(container).Map(x => x.WrapSync()),
            containerWrap => taskRunner.Run(containerWrap.Value, outputProgress, environment, task)
        )
        select unit;

    private Aff<RT, Unit> RunTasksInContainers(IContainerRuntime<RT> containerRuntime, IProgress<TaskOutput> outputProgress, Environment environment, DirectoryInfo baseDirectory, Lst<TaskInfo> tasks) =>
        use(
            containerRuntime.NewContext().Map(x => x.WrapSync()),
            containerRuntimeContextWrap =>
                from _10 in Eff(fun(() => outputProgress.Report(TaskOutput.From("==== [COPY CONTEXT] ======"))))
                from _20 in containerRuntimeContextWrap.Value.CopyFilesFromDirectory(baseDirectory)
                from _30 in tasks
                    .Select(x => RunTaskInContainer(containerRuntimeContextWrap.Value, outputProgress, environment, x))
                    .TraverseSerial(identity)
                select unit
        );

    private Aff<RT, Unit> RunTasksLocally(IProgress<TaskOutput> outputProgress, Environment environment, Lst<TaskInfo> tasks) =>
        from _10 in tasks
            .Select(x => taskRunner.Run(localStepRunner, outputProgress, environment, x))
            .TraverseSerial(identity)
        select unit;
}
