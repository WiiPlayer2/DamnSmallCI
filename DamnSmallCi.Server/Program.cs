using System.Text.Json.Nodes;
using DamnSmallCI.Application;
using DamnSmallCI.ContainerRuntime.Kubernetes;
using DamnSmallCI.Domain;
using DamnSmallCi.Server;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using DamnSmallCI.Server.RepositoryManager.GitCli;
using DamnSmallCI.Server.RepositoryResolver.Direct;
using DamnSmallCI.Server.RepositoryResolver.Github;
using DamnSmallCI.StepRunner.Shell;
using DamnSmallCI.YamlReader.YamlDotNet;
using LanguageExt;
using LanguageExt.Sys.Live;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static LanguageExt.Prelude;
using Environment = DamnSmallCI.Domain.Environment;

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
builder.Services.AddGithubRepositoryResolver<Runtime>();

builder.Services.AddOptions<ServerConfig>()
    .BindConfiguration("Config")
    .ValidateDataAnnotations();
builder.Services.AddTransient<ServerEnvironment>(sp => sp.GetRequiredService<IOptionsSnapshot<ServerConfig>>()
    .Value.Environment
    .Select(x => (EnvironmentVariableName.From(x.Key), EnvironmentVariableValue.From(x.Value)))
    .ToMap()
    .Apply(x => new ServerEnvironment(Environment.From(x))));
builder.Services.AddTransient<AuthorizedWebhookToken>(sp => sp.GetRequiredService<IOptionsSnapshot<ServerConfig>>()
    .Value.AuthorizedToken.Apply(x => new AuthorizedWebhookToken(WebhookToken.From(x))));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/webhook/{resolver}",
        async (
            string resolver,
            [FromQuery] string token,
            [FromBody] JsonObject body,
            [FromServices] WebhookUseCase<Runtime> webhookUseCase,
            [FromServices] IOptionsSnapshot<ServerConfig> serverConfig,
            CancellationToken cancellationToken
        ) =>
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
