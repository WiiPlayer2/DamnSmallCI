using Vogen;

namespace DamnSmallCI.Server.Domain;

public record RepositoryInfo(
    RepositoryUrl Url,
    RepositoryCommitHash CommitHash);

[ValueObject<Uri>]
public partial record struct RepositoryUrl;

[ValueObject<string>]
public partial record struct RepositoryCommitHash;

[ValueObject<string>]
public partial record struct WebhookToken;
