using CliWrap;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using k8s.Models;
using KubeOps.KubernetesClient;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Kubernetes;

internal class KubernetesContainer<RT>(k8s.Kubernetes kubernetes, KubernetesClient client, V1Pod pod) : IContainer<RT> where RT : struct, HasCancel<RT>
{
    public async ValueTask DisposeAsync() => await client.DeleteAsync<V1Pod>(pod.Name(), pod.Namespace());

    public Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Step step) =>
        from _10 in Aff(async (RT rt) => await Cli.Wrap("kubectl")
            .WithArguments(["exec", "-i", "-n", pod.Namespace(), pod.Name(), "--", "/bin/sh"])
            .WithStandardInputPipe(PipeSource.FromString(step.Value + "\n"))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
            .ExecuteAsync(rt.CancellationToken))
        select unit;
}
