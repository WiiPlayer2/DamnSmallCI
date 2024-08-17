using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryResolver.Direct;

internal class DirectRepositoryResolver<RT> : IRepositoryResolver<RT> where RT : struct, HasCancel<RT>
{
    public RepositoryResolverName Name { get; } = RepositoryResolverName.From("direct");

    public Aff<RT, RepositoryInfo> Resolve(RepositoryResolverWebhookBody webhookBody) => throw new NotImplementedException();
}
