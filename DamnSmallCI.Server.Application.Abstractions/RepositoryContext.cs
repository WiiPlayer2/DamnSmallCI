using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public record RepositoryContext<RT>(
    RepositoryInfo RepositoryInfo,
    Option<IRepositoryCommitStatusPublisher<RT>> CommitStatusPublisher) where RT : struct, HasCancel<RT>;
