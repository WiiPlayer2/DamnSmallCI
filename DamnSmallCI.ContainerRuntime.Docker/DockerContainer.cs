using System.Text;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using Docker.DotNet;
using Docker.DotNet.Models;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.ContainerRuntime.Docker;

internal class DockerContainer<RT>(DockerClient client, string containerId) : IContainer<RT> where RT : struct, HasCancel<RT>
{
    public async ValueTask DisposeAsync() => await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
        Force = true,
    });

    public Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Step step) =>
        from exec in Aff((RT rt) => client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
        {
            // Tty = true,
            AttachStdin = true,
            AttachStdout = true,
            AttachStderr = true,
            Cmd = ["/bin/sh"],
        }, rt.CancellationToken).ToValue())
        from streams in Aff((RT rt) => client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, rt.CancellationToken).ToValue())
        from inputBuffer in Eff(() => Encoding.UTF8.GetBytes(step.Value + "\n"))
        from _10 in Aff((RT rt) => streams.WriteAsync(inputBuffer, 0, inputBuffer.Length, rt.CancellationToken).ToUnit().ToValue())
        from _15 in Eff(fun(streams.CloseWrite))
        from outputTexts in Aff((RT rt) => streams.ReadOutputToEndAsync(rt.CancellationToken).ToValue())
        from _20 in string.IsNullOrEmpty(outputTexts.stdout)
            ? unitEff
            : Eff(fun(() => outputProgress.Report(StepOutput.From(outputTexts.stdout.TrimEnd('\n')))))
        from _30 in string.IsNullOrEmpty(outputTexts.stderr)
            ? unitEff
            : Eff(fun(() => outputProgress.Report(StepOutput.From(outputTexts.stderr.TrimEnd('\n')))))
        select unit;
}
