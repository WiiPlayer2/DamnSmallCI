using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DamnSmallCI.Server.Application;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

internal class GithubCommitStatusPublisher<RT>(
    GithubRepository repository,
    RepositoryCommitHash hash,
    GithubAccessToken accessToken,
    ILogger<GithubCommitStatusPublisher<RT>> logger) : IRepositoryCommitStatusPublisher<RT> where RT : struct, HasCancel<RT>
{
    private const string BASE_URL = "https://api.github.com";

    private const string PRODUCT_NAME = "DamnSmallCI";

    private const string PRODUCT_VERSION = "0.1";

    public Aff<RT, Unit> Publish(CommitStatus commitStatus) =>
        use(
            Eff(() => new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value),
                    UserAgent =
                    {
                        new ProductInfoHeaderValue(new ProductHeaderValue(PRODUCT_NAME, PRODUCT_VERSION)),
                    },
                },
            }),
            httpClient =>
                from _00 in unitAff
                let url = new Uri($"{BASE_URL}/repos/{repository.Value}/statuses/{hash.Value}")
                let dto = new CommitStatusDto
                {
                    Context = "damn-small-ci",
                    Description = commitStatus.Description.IfNoneUnsafe(default(string)),
                    State = commitStatus.Match(
                        _ => "failure",
                        _ => "pending",
                        _ => "success"),
                }
                let json = JsonSerializer.Serialize(dto)
                let content = new StringContent(json, Encoding.UTF8, "application/json")
                from _07 in Eff(fun(() => logger.LogDebug("Publish github commit status @ {url}: {json}", url, json)))
                from response in Aff((RT rt) => httpClient.PostAsync(url, content, rt.CancellationToken).ToValue())
                from _10 in Eff(fun(response.EnsureSuccessStatusCode))
                select unit
        );
}
