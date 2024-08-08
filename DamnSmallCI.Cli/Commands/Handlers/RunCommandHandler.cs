using System.CommandLine.Invocation;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace DamnSmallCI.Cli.Commands.Handlers;

internal class RunCommandHandler(ILogger<RunCommand> logger) : ICommandHandler
{
    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken());
        var runtime = Runtime.New(cts);
        return await AffMaybe((Runtime rt) => InvokeAff<Runtime>(context).Run(rt))
            .Run(runtime)
            .Map(x => x.Match(
                _ => 0,
                e =>
                {
                    logger.LogError(e.ToException(), "Failed to run");
                    return e.Code;
                }));
    }

    private Aff<RT, Unit> InvokeAff<RT>(InvocationContext context) where RT : struct, HasCancel<RT> => throw new NotImplementedException();
}
