using DamnSmallCI.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.StepRunner.Shell;

public static class DI
{
    public static void AddShellStepRunner<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddTransient<IStepRunner<RT>, ShellStepRunner<RT>>();
    }
}
