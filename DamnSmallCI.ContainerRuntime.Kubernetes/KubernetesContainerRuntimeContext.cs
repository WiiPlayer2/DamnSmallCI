using System.Formats.Tar;
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
        from pod in SuccessEff(new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = $"{pvc.Name()}-init",
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
                        Name = "init",
                        Image = "busybox",
                        WorkingDir = "/src",
                        Tty = true,
                        Stdin = true,
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
        from _20 in Aff(async (RT rt) =>
        {
            using var muxedStream = await kubernetes.MuxedStreamNamespacedPodExecAsync(
                createdPod.Name(),
                createdPod.Namespace(),
                ["sh", "-c", "tar xmf - -C /src"],
                "init",
                stdin: true,
                stderr: false,
                stdout: false,
                tty: false,
                cancellationToken: rt.CancellationToken).ConfigureAwait(false);
            using var stdIn = muxedStream.GetStream(null, ChannelIndex.StdIn);
            // using var error = muxedStream.GetStream(ChannelIndex.Error, null);
            // using var errorReader = new StreamReader(error);

            muxedStream.Start();

            await TarFile.CreateFromDirectoryAsync(directory.FullName, stdIn, false, rt.CancellationToken);
            await stdIn.FlushAsync(rt.CancellationToken);

            stdIn.Close();

            return unit;
        })
        from _30 in Aff((RT rt) => client.DeleteAsync<V1Pod>(createdPod.Name(), createdPod.Namespace(), rt.CancellationToken).ToUnit().ToValue())
        select unit;

    public Aff<RT, IContainer<RT>> NewContainer(ImageName image) =>
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
                        WorkingDir = "/src",
                        Tty = true,
                        Stdin = true,
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
        select (IContainer<RT>) new KubernetesContainer<RT>(kubernetes, client, createdPod);
}
