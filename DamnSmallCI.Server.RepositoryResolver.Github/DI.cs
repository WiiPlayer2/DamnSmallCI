using DamnSmallCI.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

public static class DI
{
    public static void AddGithubRepositoryResolver<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddOptions<GithubConfig>()
            .BindConfiguration("Github")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRepositoryResolver<RT>, GithubRepositoryResolver<RT>>();
        services.AddScoped<GithubAccessToken>(sp =>
            GithubAccessToken.From(sp.GetRequiredService<IOptionsSnapshot<GithubConfig>>().Value.AccessToken));
    }
}
