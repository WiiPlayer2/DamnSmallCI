using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public interface IRepositoryResolver<RT> where RT : struct, HasCancel<RT>
{
    RepositoryResolverName Name { get; }

    Aff<RT, RepositoryContext<RT>> Resolve(RepositoryResolverWebhookBody webhookBody);
}
