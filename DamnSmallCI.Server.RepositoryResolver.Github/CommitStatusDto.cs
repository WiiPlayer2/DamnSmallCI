using System.Text.Json.Serialization;

namespace DamnSmallCI.Server.RepositoryResolver.Github;

internal class CommitStatusDto
{
    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("state")]
    public required string State { get; set; }
}
