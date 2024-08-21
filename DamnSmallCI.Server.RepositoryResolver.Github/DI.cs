using DamnSmallCI.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

public static class DI
{
    public static void AddGithubRepositoryResolver<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IRepositoryResolver<RT>, GithubRepositoryResolver<RT>>();
    }
}
