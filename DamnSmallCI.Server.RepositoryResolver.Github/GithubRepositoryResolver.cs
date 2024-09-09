using System.Text.Json;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

internal class GithubRepositoryResolver<RT> : IRepositoryResolver<RT> where RT : struct, HasCancel<RT>
{
    public RepositoryResolverName Name { get; } = RepositoryResolverName.From("github");

    public Aff<RT, RepositoryContext<RT>> Resolve(RepositoryResolverWebhookBody webhookBody) =>
        from dto in Eff(() => webhookBody.Value.Deserialize<WebhookBodyDto>())
        from repositoryUrl in Eff(() => RepositoryUrl.From(dto.Repository.CloneUrl))
        from repositoryCommitHash in Eff(() => RepositoryCommitHash.From(dto.After))
        let info = new RepositoryInfo(repositoryUrl, repositoryCommitHash)
        select new RepositoryContext<RT>(info, None);
}
