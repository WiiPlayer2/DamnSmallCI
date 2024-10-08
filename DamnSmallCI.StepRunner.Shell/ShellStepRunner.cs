﻿using System.Collections.Immutable;
using CliWrap;
using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.StepRunner.Shell;

public class ShellStepRunner<RT> : IStepRunner<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Environment environment, Step step) =>
        from command in CreateSystemCommand()
        let fullCommand = command
            .WithEnvironmentVariables(environment.Value.ToImmutableDictionary(x => x.Key.Value, x => x.Value.Value))
            .WithStandardInputPipe(PipeSource.FromString(step.Value))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => outputProgress.Report(StepOutput.From(line))))
        from result in Aff(async (RT rt) => await fullCommand.ExecuteAsync(rt.CancellationToken))
        select unit;

    private Eff<Command> CreateLinuxCommand() =>
        Eff(() => Cli.Wrap("bash").WithArguments("-"));

    private Eff<Command> CreateSystemCommand() =>
        OperatingSystem.IsLinux()
            ? CreateLinuxCommand()
            : FailEff<Command>(Error.New($"Platform {System.Environment.OSVersion.Platform} is not supported."));
}
