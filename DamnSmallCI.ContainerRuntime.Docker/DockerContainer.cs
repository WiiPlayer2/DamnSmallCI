using System.Text;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using Docker.DotNet;
using Docker.DotNet.Models;
using LanguageExt.Effects.Traits;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.ContainerRuntime.Docker;

internal class DockerContainer<RT>(DockerClient client, string containerId) : IContainer<RT> where RT : struct, HasCancel<RT>
{
    public async ValueTask DisposeAsync() => await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
        Force = true,
    });

    public Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Environment environment, Step step) =>
        from exec in Aff((RT rt) => client.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters
        {
            // Tty = true,
            AttachStdin = true,
            AttachStdout = true,
            AttachStderr = true,
            Cmd = ["sh"],
            Env = environment.Value.Pairs
                .Select(kv => $"{kv.Key}={kv.Value}")
                .ToList(),
        }, rt.CancellationToken).ToValue())
        from streams in Aff((RT rt) => client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, rt.CancellationToken).ToValue())
        from inputBuffer in Eff(() => Encoding.UTF8.GetBytes(step.Value + "\n"))
        from _10 in Aff((RT rt) => streams.WriteAsync(inputBuffer, 0, inputBuffer.Length, rt.CancellationToken).ToUnit().ToValue())
        from _15 in Eff(fun(streams.CloseWrite))
        let readBuffer = new byte[1024]
        from _20 in repeatUntil(
            from readResult in Aff((RT rt) => streams.ReadOutputAsync(readBuffer, 0, readBuffer.Length, rt.CancellationToken).ToValue())
            from text in Eff(() => Encoding.UTF8.GetString(readBuffer, 0, readResult.Count))
            from _10 in string.IsNullOrEmpty(text)
                ? unitEff
                : Eff(fun(() => outputProgress.Report(StepOutput.From(text))))
            select readResult.EOF,
            identity
        )
        from execInspect in Aff((RT rt) => client.Exec.InspectContainerExecAsync(exec.ID, rt.CancellationToken).ToValue())
        from _40 in execInspect.ExitCode == 0
            ? unitEff
            : FailEff<Unit>($"Expected exit code 0, got {execInspect.ExitCode}")
        select unit;
}
