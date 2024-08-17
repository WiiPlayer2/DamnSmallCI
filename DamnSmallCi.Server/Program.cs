using System.Text.Json.Nodes;
using DamnSmallCI.Application;
using DamnSmallCI.ContainerRuntime.Kubernetes;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using DamnSmallCI.Server.RepositoryManager.GitCli;
using DamnSmallCI.Server.RepositoryResolver.Direct;
using DamnSmallCI.StepRunner.Shell;
using DamnSmallCI.YamlReader.YamlDotNet;
using LanguageExt;
using LanguageExt.Sys.Live;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.Prelude;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServerApplicationServices<Runtime>();
builder.Services.AddApplicationServices<Runtime>();
builder.Services.AddYamlDotNetYamlReader();
builder.Services.AddKubernetesContainerRuntime<Runtime>();
builder.Services.AddShellStepRunner<Runtime>();
builder.Services.AddDirectRepositoryResolver<Runtime>();
builder.Services.AddGitCliRepositoryManager<Runtime>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/webhook/{resolver}", async (string resolver, [FromQuery] string token, [FromBody] JsonObject body, [FromServices] WebhookUseCase<Runtime> webhookUseCase, CancellationToken cancellationToken) =>
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await (
                from resolverName in Eff(() => RepositoryResolverName.From(resolver))
                from webhookBody in Eff(() => RepositoryResolverWebhookBody.From(body))
                from webhookToken in Eff(() => WebhookToken.From(token))
                from _10 in AffMaybe((Runtime rt) => webhookUseCase.Execute(resolverName, webhookBody, webhookToken).Run(rt))
                select unit
            )
            .RunUnit(Runtime.New(cts));
    })
    .WithName("Webhook")
    .WithOpenApi();

app.Run();
