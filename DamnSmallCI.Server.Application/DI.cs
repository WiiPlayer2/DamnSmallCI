using DamnSmallCI.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.Application;

public static class DI
{
    public static void AddServerApplicationServices<RT>(this IServiceCollection services)
        where RT : struct, HasCancel<RT>
    {
        services.AddTransient<WebhookUseCase<RT>>();
        services.AddSingleton<ResolverProvider<RT>>();
    }
}
