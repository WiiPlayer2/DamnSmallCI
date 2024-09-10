using System.Net.Http.Headers;
using System.Text.Json;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

internal class GithubCommitStatusPublisher<RT>(
    GithubRepository repository,
    RepositoryCommitHash hash,
    GithubAccessToken accessToken) : IRepositoryCommitStatusPublisher<RT> where RT : struct, HasCancel<RT>
{
    private const string BASE_URL = "https://api.github.com";

    public Aff<RT, Unit> Publish(CommitStatus commitStatus) =>
        use(
            Eff(() => new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value),
                },
            }),
            httpClient =>
                from _00 in unitAff
                let url = new Uri($"{BASE_URL}/repos/{repository}/statuses/{hash}")
                let dto = new CommitStatusDto
                {
                    Context = "damn-small-ci",
                    Description = commitStatus.Description.IfNoneUnsafe(default(string)),
                    State = commitStatus.Match(
                        _ => "failure",
                        _ => "pending",
                        _ => "success"),
                }
                let content = new StringContent(JsonSerializer.Serialize(dto))
                from response in Aff((RT rt) => httpClient.PostAsync(url, content, rt.CancellationToken).ToValue())
                from _10 in Eff(fun(response.EnsureSuccessStatusCode))
                select unit
        );
}
