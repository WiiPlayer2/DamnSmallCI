using DamnSmallCI.Application;
using Microsoft.Extensions.DependencyInjection;

namespace DamnSmallCI.YamlReader.YamlDotNet;

public static class DI
{
    public static void AddYamlDotNetYamlReader(this IServiceCollection services)
    {
        services.AddSingleton<IYamlReader, YamlDotNetYamlReader>();
    }
}
