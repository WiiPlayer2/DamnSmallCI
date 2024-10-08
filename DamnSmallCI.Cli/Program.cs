﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using DamnSmallCI.Application;
using DamnSmallCI.Cli.Commands;
using DamnSmallCI.Cli.Commands.Handlers;
using DamnSmallCI.ContainerRuntime.Docker;
using DamnSmallCI.ContainerRuntime.Kubernetes;
using DamnSmallCI.StepRunner.Shell;
using DamnSmallCI.YamlReader.YamlDotNet;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.Hosting;

return new CommandLineBuilder(new RootCommand("Pipeline running engine for local execution")
    {
        new RunCommand(),
    })
    .UseDefaults()
    .UseHost(
        args => new HostBuilder().ConfigureDefaults(args),
        builder => builder
            .ConfigureServices((context, services) =>
            {
                services.AddYamlDotNetYamlReader();
                services.AddShellStepRunner<Runtime>();
                services.AddDockerContainerRuntime<Runtime>();
                services.AddKubernetesContainerRuntime<Runtime>();
                services.AddApplicationServices<Runtime>();
            })
            .UseCommandHandler<RunCommand, RunCommandHandler>())
    .Build()
    .Invoke(args);
