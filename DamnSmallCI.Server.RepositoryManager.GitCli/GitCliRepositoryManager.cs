using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryManager.GitCli;

internal class GitCliRepositoryManager<RT> : IRepositoryManager<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Clone(RepositoryInfo repositoryInfo, DirectoryInfo directory) => throw new NotImplementedException();
}
