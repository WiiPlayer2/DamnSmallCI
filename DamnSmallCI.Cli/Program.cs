﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using DamnSmallCI.Cli.Commands;
using DamnSmallCI.Cli.Commands.Handlers;
using Microsoft.Extensions.Hosting;

return new CommandLineBuilder(new RootCommand("Pipeline running engine for local execution")
{
    new RunCommand(),
})
.UseDefaults()
.UseHost(
    args => new HostBuilder().ConfigureDefaults(args),
    builder => builder
        .UseCommandHandler<RunCommand, RunCommandHandler>())
.Build()
.Invoke(args);