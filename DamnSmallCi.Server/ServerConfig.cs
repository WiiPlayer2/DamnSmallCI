namespace DamnSmallCi.Server;

public class ServerConfig
{
    public required string AuthorizedToken { get; set; }

    public Dictionary<string, string> Environment { get; set; } = new();
}
