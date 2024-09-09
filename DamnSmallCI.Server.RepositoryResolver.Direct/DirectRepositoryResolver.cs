using System.Text.Json;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryResolver.Direct;

internal class DirectRepositoryResolver<RT> : IRepositoryResolver<RT> where RT : struct, HasCancel<RT>
{
    public RepositoryResolverName Name { get; } = RepositoryResolverName.From("direct");

    public Aff<RT, RepositoryContext<RT>> Resolve(RepositoryResolverWebhookBody webhookBody) =>
        from dto in Eff(() => webhookBody.Value.Deserialize<WebhookBodyDto>())
        from repositoryUrl in Eff(() => RepositoryUrl.From(dto.Url))
        from repositoryCommitHash in Eff(() => RepositoryCommitHash.From(dto.CommitHash))
        let info = new RepositoryInfo(repositoryUrl, repositoryCommitHash)
        select new RepositoryContext<RT>(info, None);
}
