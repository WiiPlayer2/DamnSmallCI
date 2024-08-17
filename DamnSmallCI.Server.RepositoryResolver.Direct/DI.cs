using DamnSmallCI.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.Server.RepositoryResolver.Direct;

public static class DI
{
    public static void AddDirectRepositoryResolver<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IRepositoryResolver<RT>, DirectRepositoryResolver<RT>>();
    }
}
