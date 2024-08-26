using System.Formats.Tar;
using CliWrap;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Kubernetes;

internal class KubernetesContainerRuntimeContext<RT>(k8s.Kubernetes kubernetes, KubernetesClient client, V1PersistentVolumeClaim pvc) : IContainerRuntimeContext<RT> where RT : struct, HasCancel<RT>
{
    public async ValueTask DisposeAsync() => await client.DeleteAsync<V1PersistentVolumeClaim>(pvc.Name(), pvc.Namespace());

    public Aff<RT, Unit> CopyFilesFromDirectory(DirectoryInfo directory) =>
        from createdPod in CreatePod(ImageName.From("busybox"), None)
        from _10 in Aff((RT rt) => client.WatchAsync<V1Pod>(createdPod.Namespace(), cancellationToken: rt.CancellationToken)
            .FirstAsync(x => x.Type == WatchEventType.Modified && x.Entity.Status.Phase == "Running"))
        from _20 in use(
            SuccessAff(new MemoryStream()),
            memoryStream =>
                from _10 in Aff((RT rt) => TarFile.CreateFromDirectoryAsync(directory.FullName, memoryStream, false, rt.CancellationToken).ToUnit().ToValue())
                from _20 in Eff(() => memoryStream.Position = 0)
                from _30 in Aff(async (RT rt) => await Cli.Wrap("kubectl")
                    .WithArguments(["exec", "-i", "-n", createdPod.Namespace(), createdPod.Name(), "--", "sh", "-c", "tar xmf - -C /src"])
                    .WithStandardInputPipe(PipeSource.FromStream(memoryStream, true))
                    .ExecuteAsync(rt.CancellationToken))
                select unit
        )
        from _30 in Aff((RT rt) => client.DeleteAsync<V1Pod>(createdPod.Name(), createdPod.Namespace(), rt.CancellationToken).ToUnit().ToValue())
        select unit;

    public Aff<RT, IContainer<RT>> NewContainer(TaskContainerInfo containerInfo) =>
        from createdPod in CreatePod(containerInfo.Image, containerInfo.Entrypoint)
        select (IContainer<RT>) new KubernetesContainer<RT>(kubernetes, client, createdPod);

    private Aff<RT, V1Pod> CreatePod(ImageName image, Option<ContainerEntrypoint> entrypoint) =>
        from pod in SuccessEff(new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{pvc.Name()}-{Guid.NewGuid()}",
                NamespaceProperty = pvc.Namespace(),
            },
            Spec = new V1PodSpec
            {
                Volumes =
                [
                    new V1Volume
                    {
                        Name = "source",
                        PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource
                        {
                            ClaimName = pvc.Name(),
                        },
                    },
                ],
                Containers =
                [
                    new V1Container
                    {
                        Name = "task",
                        Image = image.Value,
                        Command = entrypoint.MatchUnsafe(
                            x => x.Value.ToList(),
                            () => default(IList<string>)),
                        WorkingDir = "/src",
                        Tty = true,
                        Stdin = true,
                        Resources = new V1ResourceRequirements
                        {
                            Limits = new Dictionary<string, ResourceQuantity>
                            {
                                {"cpu", new ResourceQuantity("50m")},
                            },
                        },
                        VolumeMounts =
                        [
                            new V1VolumeMount
                            {
                                Name = "source",
                                MountPath = "/src",
                            },
                        ],
                    },
                ],
            },
        })
        from createdPod in Aff((RT rt) => client.CreateAsync(pod, rt.CancellationToken).ToValue())
        from _10 in Aff((RT rt) => client.WatchAsync<V1Pod>(createdPod.Namespace(), cancellationToken: rt.CancellationToken)
            .FirstAsync(x => x.Type == WatchEventType.Modified && x.Entity.Status.Phase == "Running"))
        select createdPod;
}
