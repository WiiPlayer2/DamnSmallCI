using DamnSmallCI.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.Server.RepositoryManager.GitCli;

public static class DI
{
    public static void AddGitCliRepositoryManager<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IRepositoryManager<RT>, GitCliRepositoryManager<RT>>();
    }
}
