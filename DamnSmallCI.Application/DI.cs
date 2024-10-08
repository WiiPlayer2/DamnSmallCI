using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.Application;

public static class DI
{
    public static void AddApplicationServices<RT>(this IServiceCollection services)
        where RT : struct, HasCancel<RT>
    {
        services.AddTransient<RunUseCase<RT>>();
        services.AddTransient<PipelineRunner<RT>>();
        services.AddTransient<TaskRunner<RT>>();
    }
}
