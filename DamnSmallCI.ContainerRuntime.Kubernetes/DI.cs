using DamnSmallCI.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.ContainerRuntime.Kubernetes;

public static class DI
{
    public static void AddKubernetesContainerRuntime<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddTransient<IContainerRuntime<RT>, KubernetesContainerRuntime<RT>>();
    }
}
