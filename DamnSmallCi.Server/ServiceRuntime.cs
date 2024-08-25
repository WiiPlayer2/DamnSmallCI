using DamnSmallCI.Server.Application;
using LanguageExt.Sys.Live;

namespace DamnSmallCi.Server;

public class ServiceRuntime(IHostApplicationLifetime lifetime) : IServiceRuntime<Runtime>, IDisposable
{
    public void Dispose()
    {
        Runtime.CancellationTokenSource.Cancel();
        Runtime.CancellationTokenSource.Dispose();
    }

    public Runtime Runtime { get; } = Runtime.New(CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping));
}
