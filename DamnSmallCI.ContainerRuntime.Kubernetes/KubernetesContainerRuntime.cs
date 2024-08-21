using DamnSmallCI.Application;
using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Kubernetes;

internal class KubernetesContainerRuntime<RT> : IContainerRuntime<RT> where RT : struct, HasCancel<RT>
{
    public string Name => "kubernetes";

    public Aff<RT, IContainerRuntimeContext<RT>> NewContext() =>
        from clientConfig in Eff(KubernetesClientConfiguration.BuildDefaultConfig)
        from kubernetes in Eff(() => new k8s.Kubernetes(clientConfig))
        from client in Eff(() => new KubernetesClient(clientConfig, kubernetes))
        let volumeName = $"dsci-{Guid.NewGuid()}"
        let persistentVolumeClaim = new V1PersistentVolumeClaim
        {
            Metadata = new V1ObjectMeta
            {
                Name = volumeName,
                NamespaceProperty = "default",
            },
            Spec = new V1PersistentVolumeClaimSpec
            {
                AccessModes = ["ReadWriteOnce"],
                Resources = new V1ResourceRequirements
                {
                    Requests = new Dictionary<string, ResourceQuantity>
                    {
                        {"storage", new ResourceQuantity("1Gi")},
                    },
                },
            },
        }
        from createdPersistentVolumeClaim in Aff((RT rt) => client.CreateAsync(persistentVolumeClaim, rt.CancellationToken).ToValue())
        select (IContainerRuntimeContext<RT>) new KubernetesContainerRuntimeContext<RT>(kubernetes, client, createdPersistentVolumeClaim);
}
