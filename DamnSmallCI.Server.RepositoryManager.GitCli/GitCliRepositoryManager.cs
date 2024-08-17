using CliWrap;
using CliWrap.Buffered;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryManager.GitCli;

internal class GitCliRepositoryManager<RT> : IRepositoryManager<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Clone(RepositoryInfo repositoryInfo, DirectoryInfo directory) =>
        from cloneResult in Aff(async (RT rt) => await Cli.Wrap("git")
            .WithArguments(["clone", repositoryInfo.Url.ToString(), directory.FullName])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(rt.CancellationToken))
        from _10 in guard(cloneResult.IsSuccess, Error.New($"Clone failed: {cloneResult.StandardError}"))
        from checkoutResult in Aff(async (RT rt) => await Cli.Wrap("git")
            .WithArguments(["-C", directory.FullName, "checkout", repositoryInfo.CommitHash.Value])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(rt.CancellationToken))
        from _30 in guard(checkoutResult.IsSuccess, Error.New($"Checkout failed: {checkoutResult.StandardError}"))
        select unit;
}
