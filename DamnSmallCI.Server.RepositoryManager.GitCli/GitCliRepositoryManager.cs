using CliWrap;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryManager.GitCli;

internal class GitCliRepositoryManager<RT> : IRepositoryManager<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Clone(RepositoryInfo repositoryInfo, DirectoryInfo directory) =>
        from _10 in Aff(async (RT rt) => await Cli.Wrap("git")
            .WithArguments(["clone", repositoryInfo.Url.ToString(), directory.FullName])
            .ExecuteAsync(rt.CancellationToken))
        from _20 in Aff(async (RT rt) => await Cli.Wrap("git")
            .WithArguments(["-C", directory.FullName, "checkout", repositoryInfo.CommitHash.Value])
            .ExecuteAsync(rt.CancellationToken))
        select unit;
}
