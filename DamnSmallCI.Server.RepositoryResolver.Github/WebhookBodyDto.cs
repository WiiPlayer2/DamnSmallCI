using System.Text.Json.Serialization;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

internal class WebhookBodyDto
{
    [JsonPropertyName("after")]
    public required string After { get; set; }

    [JsonPropertyName("repository")]
    public required RepositoryDto Repository { get; set; }

    public class RepositoryDto
    {
        [JsonPropertyName("clone_url")]
        public required Uri CloneUrl { get; set; }

        [JsonPropertyName("full_name")]
        public required string FullName { get; set; }
    }
}
