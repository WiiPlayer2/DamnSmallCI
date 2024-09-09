using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public interface IRepositoryCommitStatusPublisher<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> Publish(CommitStatus commitStatus);
}
