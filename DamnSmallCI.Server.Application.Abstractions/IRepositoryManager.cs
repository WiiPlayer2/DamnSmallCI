using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public interface IRepositoryManager<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> Clone(RepositoryInfo repositoryInfo, DirectoryInfo directory);
}
