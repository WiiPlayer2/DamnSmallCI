using System.Text.Json.Nodes;
using Vogen;

namespace DamnSmallCI.Server.Application;

[ValueObject<string>]
public partial record struct RepositoryResolverName
{
    private static string NormalizeInput(string input) => input.ToLowerInvariant();
}

[ValueObject<JsonObject>]
public partial record struct RepositoryResolverWebhookBody;
