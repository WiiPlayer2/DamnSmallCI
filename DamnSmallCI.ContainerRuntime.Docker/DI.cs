using DamnSmallCI.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.ContainerRuntime.Docker;

public static class DI
{
    public static void AddDockerContainerRuntime<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddTransient<IContainerRuntime<RT>, DockerContainerRuntime<RT>>();
    }
}
