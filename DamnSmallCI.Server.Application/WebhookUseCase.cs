using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public class WebhookUseCase<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Execute(RepositoryResolverName resolverName, RepositoryResolverWebhookBody webhookBody, WebhookToken token) => throw new NotImplementedException();
}
