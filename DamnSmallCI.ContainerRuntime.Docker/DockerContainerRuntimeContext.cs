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
    public async ValueTask DisposeAsync() => await client.Volumes.RemoveAsync(volume.Name, true);

    public Aff<RT, Unit> CopyFilesFromDirectory(DirectoryInfo directory) =>
        from container in CreateContainer(new TaskContainerInfo(ImageName.From("ubuntu"), None)) // TODO dispose container when done
        from exec in Aff((RT rt) => client.Exec.ExecCreateContainerAsync(container.ID, new ContainerExecCreateParameters
        {
            AttachStdin = true,
            Cmd = ["sh", "-c", "tar xmf - -C /src"],
        }, rt.CancellationToken).ToValue())
        from streams in Aff((RT rt) => client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, rt.CancellationToken).ToValue())
        let tarStream = new MemoryStream()
        from _10 in Aff((RT rt) => TarFile.CreateFromDirectoryAsync(directory.FullName, tarStream, false, rt.CancellationToken).ToUnit().ToValue())
        from _20 in Eff(fun(() => tarStream.Position = 0))
        let tarData = tarStream.ToArray()
        from _30 in Aff((RT rt) => streams.WriteAsync(tarData, 0, tarData.Length, rt.CancellationToken).ToUnit().ToValue())
        from _40 in Eff(fun(streams.CloseWrite))
        from _50 in Aff((RT rt) => streams.ReadOutputToEndAsync(rt.CancellationToken).ToValue())
        from _60 in Aff((RT rt) => client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters {Force = true}, rt.CancellationToken).ToUnit().ToValue())
        select unit;

    public Aff<RT, IContainer<RT>> NewContainer(TaskContainerInfo containerInfo) =>
        from container in CreateContainer(containerInfo)
        select (IContainer<RT>) new DockerContainer<RT>(client, container.ID);

    private Aff<RT, CreateContainerResponse> CreateContainer(TaskContainerInfo containerInfo) =>
        from repoAndTag in SuccessEff(ParseImage(containerInfo.Image))
        from _10 in Aff((RT rt) => client.Images.ListImagesAsync(new ImagesListParameters(), rt.CancellationToken).ToValue())
            .Map(x => x.Find(x => x.RepoTags.Contains($"{repoAndTag.Repo}:{repoAndTag.Tag}"))
                .Match(
                    _ => unitAff,
                    () => Aff((RT rt) => client.Images.CreateImageAsync(new ImagesCreateParameters
                            {
                                FromImage = repoAndTag.Repo,
                                Tag = repoAndTag.Tag,
                            },
                            new AuthConfig(),
                            new Progress<JSONMessage>(),
                            rt.CancellationToken)
                        .ToUnit()
                        .ToValue())))
        from container in Aff((RT rt) => client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = containerInfo.Image.Value,
            Entrypoint = containerInfo.Entrypoint.MatchUnsafe(
                x => x.Value.ToList(),
                () => default(IList<string>)),
            WorkingDir = "/src",
            AttachStdin = true,
            Tty = true,
            HostConfig = new HostConfig
            {
                AutoRemove = true,
                Binds =
                [
                    $"{volume.Name}:/src",
                ],
            },
        }, rt.CancellationToken).ToValue())
        from containerStarted in Aff((RT rt) => client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters(), rt.CancellationToken).ToValue())
        from _20 in guard(containerStarted, Error.New("Failed to start container"))
        select container;

    // TODO should be part of the domain logic
    private (string Repo, string Tag) ParseImage(ImageName imageName)
    {
        var splitIndex = imageName.Value.LastIndexOf(':');
        return splitIndex == -1
            ? (imageName.Value, "latest")
            : (imageName.Value[..splitIndex], imageName.Value[(splitIndex + 1)..]);
    }
}
