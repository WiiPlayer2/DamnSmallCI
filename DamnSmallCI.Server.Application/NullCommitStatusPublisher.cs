using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

internal class NullCommitStatusPublisher<RT> : IRepositoryCommitStatusPublisher<RT> where RT : struct, HasCancel<RT>
{
    public static NullCommitStatusPublisher<RT> Instance { get; } = new();

    public Aff<RT, Unit> Publish(CommitStatus commitStatus) => unitAff;
}
