using System.Formats.Tar;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using Docker.DotNet;
using Docker.DotNet.Models;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Docker;

internal class DockerContainerRuntimeContext<RT>(DockerClient client, VolumeResponse volume) : IContainerRuntimeContext<RT> where RT : struct, HasCancel<RT>
{
    public async ValueTask DisposeAsync() => await client.Volumes.RemoveAsync(volume.Name);

    public Aff<RT, Unit> CopyFilesFromDirectory(DirectoryInfo directory) =>
        from container in Aff((RT rt) => client.Containers.CreateContainerAsync(new CreateContainerParameters() // TODO dispose container when done
        {
            Image = "ubuntu",
            WorkingDir = "/src",
            AttachStdin = true,
            Tty = true,
            Volumes = new Dictionary<string, EmptyStruct>
            {
                {$"{volume.Name}:/src", default},
            },
            HostConfig = new HostConfig
            {
                AutoRemove = true,
            },
        }, rt.CancellationToken).ToValue())
        let tarStream = new MemoryStream()
        from _10 in Aff((RT rt) => TarFile.CreateFromDirectoryAsync(directory.FullName, tarStream, false, rt.CancellationToken).ToUnit().ToValue())
        from _20 in Eff(fun(() => tarStream.Position = 0))
        from _30 in Aff((RT rt) => client.Containers.ExtractArchiveToContainerAsync(container.ID, new ContainerPathStatParameters
        {
            Path = "/src",
        }, tarStream, rt.CancellationToken).ToUnit().ToValue())
        select unit;

    public Aff<RT, IContainer<RT>> NewContainer(ImageName image) =>
        from container in Aff((RT rt) => client.Containers.CreateContainerAsync(new CreateContainerParameters() // TODO dispose container when done
        {
            Image = image.Value,
            WorkingDir = "/src",
            AttachStdin = true,
            Tty = true,
            Volumes = new Dictionary<string, EmptyStruct>
            {
                {$"{volume.Name}:/src", default},
            },
            HostConfig = new HostConfig
            {
                AutoRemove = true,
            },
        }, rt.CancellationToken).ToValue())
        from containerStarted in Aff((RT rt) => client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters(), rt.CancellationToken).ToValue())
        from _10 in guard(containerStarted, Error.New("Failed to start container"))
        select (IContainer<RT>) new DockerContainer<RT>(client, container.ID);
}
