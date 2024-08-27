using System.Text;
using CliWrap;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using k8s.Models;
using KubeOps.KubernetesClient;
using LanguageExt.Effects.Traits;
using LanguageExt.UnitsOfMeasure;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.ContainerRuntime.Kubernetes;

internal class KubernetesContainer<RT>(KubernetesClient client, V1Pod pod) : IContainer<RT> where RT : struct, HasCancel<RT>
{
    private static readonly Schedule retrySchedule = Schedule.exponential(1.Seconds())
                                                     | Schedule.maxDelay(1.Minutes());

    public async ValueTask DisposeAsync() => await retry(
        retrySchedule,
        from _10 in Aff(async () => await client.DeleteAsync<V1Pod>(pod.Name(), pod.Namespace()).ToUnit())
                    | @catch(_ =>
                        from pod in Aff(async () => Optional(await client.GetAsync<V1Pod>(pod.Name(), pod.Namespace())))
                        from _10 in pod.Match(
                            p => FailEff<Unit>("Pod still exists"),
                            () => unitEff)
                        select unit
                    )
        select unit
    ).RunUnit();

    public Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Environment environment, Step step) =>
        from scriptGuid in SuccessEff(Guid.NewGuid())
        let scriptDirectory = $"/tmp2/dsci/{scriptGuid}"
        let scriptStepFile = $"{scriptDirectory}/step.sh"
        let scriptExecFile = $"{scriptDirectory}/exec.sh"
        let scriptOutFile = $"{scriptDirectory}/out"
        let scriptExitFile = $"{scriptDirectory}/exit"
        let scriptPidFile = $"{scriptDirectory}/pid"
        let scriptContent = $"""
                             mkdir -p {scriptDirectory}

                             cat << '{scriptGuid}_eof' > {scriptStepFile}
                             {string.Join("\n", environment.Value.Pairs.Select(x => $"export {x.Key}={x.Value}"))}
                             {step}
                             {scriptGuid}_eof

                             cat << '{scriptGuid}_eof' > {scriptExecFile}
                             sh -e {scriptStepFile}
                             EXIT_CODE=$?
                             mkdir -p {scriptDirectory}
                             cp {scriptOutFile} {scriptOutFile}.bak || touch {scriptOutFile}.bak
                             rm {scriptOutFile} || true
                             echo $EXIT_CODE > {scriptExitFile}
                             {scriptGuid}_eof
                             """
        from _05 in retry(
            retrySchedule,
            Aff(async (RT rt) => await Cli.Wrap("kubectl")
                .WithArguments(["exec", "-i", "-n", pod.Namespace(), pod.Name(), "--", "sh", "-e"])
                .WithStandardInputPipe(PipeSource.FromString(scriptContent))
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
                .ExecuteAsync(rt.CancellationToken))
        )
        from _10 in Aff(async (RT rt) => await Cli.Wrap("kubectl")
            .WithArguments(["exec", "-i", "-n", pod.Namespace(), pod.Name(), "--", "sh", "-e"])
            .WithStandardInputPipe(PipeSource.FromString($"""
                                                          nohup sh {scriptExecFile} &> {scriptOutFile} &
                                                          echo $! > {scriptPidFile}
                                                          """))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
            .ExecuteAsync(rt.CancellationToken))
        from errorBuilder in SuccessEff(new StringBuilder())
        from result in retryWhile(
            retrySchedule,
            from result in Aff(async (RT rt) => await Cli.Wrap("kubectl")
                .WithArguments(["exec", "-i", "-n", pod.Namespace(), pod.Name(), "--", "sh", "-e"])
                .WithStandardInputPipe(PipeSource.FromString($"""
                                                              #echo "checking for exit file..."
                                                              if [ -f {scriptExitFile} ]; then
                                                                #echo "outputting backup file..."
                                                                cat {scriptOutFile}.bak
                                                              else
                                                                #echo "tailing output and waiting for exit..."
                                                                ( tail -f {scriptOutFile} || true ) &
                                                                while [ ! -f {scriptExitFile} ]; do
                                                                  sleep 1
                                                                done
                                                                pkill tail
                                                              fi
                                                              #echo "exiting $(cat {scriptExitFile})..."
                                                              exit $(cat {scriptExitFile} || echo 255)
                                                              """))
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
                {
                    errorBuilder.AppendLine(line);
                    outputProgress.Report(StepOutput.From(line));
                }))
                .ExecuteAsync(rt.CancellationToken))
            select result,
            x =>
            {
                var errorOutput = errorBuilder.ToString();
                var shouldRetry = !errorOutput.Contains("command terminated with exit code ");
                errorBuilder.Clear();
                return shouldRetry;
            }
        )
        select unit;
}
