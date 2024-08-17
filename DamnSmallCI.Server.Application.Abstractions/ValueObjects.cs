using System.Text.Json.Nodes;
using Vogen;

namespace DamnSmallCI.Server.Application;

[ValueObject<string>]
public partial record struct RepositoryResolverName;

[ValueObject<JsonObject>]
public partial record struct RepositoryResolverWebhookBody;
