using DamnSmallCI.Application;
using Docker.DotNet;
using Docker.DotNet.Models;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Docker;

internal class DockerContainerRuntime<RT> : IContainerRuntime<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, IContainerRuntimeContext<RT>> NewContext() =>
        from dockerClient in Eff((RT rt) => new DockerClientConfiguration().CreateClient()) // TODO: dispose config & client
        from volumeResponse in Aff((RT rt) => dockerClient.Volumes.CreateAsync(new VolumesCreateParameters(), rt.CancellationToken).ToValue())
        select (IContainerRuntimeContext<RT>) new DockerContainerRuntimeContext<RT>(dockerClient, volumeResponse);
}
