using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public interface IRepositoryResolver<RT> where RT : struct, HasCancel<RT>
{
    RepositoryResolverName Name { get; }

    Aff<RT, RepositoryInfo> Resolve(RepositoryResolverWebhookBody webhookBody);
}
