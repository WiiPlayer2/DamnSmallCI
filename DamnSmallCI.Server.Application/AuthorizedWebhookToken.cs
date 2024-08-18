using DamnSmallCI.Server.Domain;

namespace DamnSmallCI.Server.Application;

public record AuthorizedWebhookToken(
    WebhookToken Token);
