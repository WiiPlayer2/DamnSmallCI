using System.Text.Json.Serialization;

namespace DamnSmallCI.Server.RepositoryResolver.Direct;

internal class WebhookBodyDto
{
    [JsonPropertyName("commit")]
    public required string CommitHash { get; set; }

    [JsonPropertyName("url")]
    public required Uri Url { get; set; }
}
