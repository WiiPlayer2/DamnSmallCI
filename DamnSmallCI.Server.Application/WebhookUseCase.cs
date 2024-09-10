using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using DamnSmallCI.Server.Domain;
using DamnSmallCi.Utils;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;

namespace DamnSmallCI.Server.Application;

public class WebhookUseCase<RT>(
    ServerEnvironment environment,
    AuthorizedWebhookToken authorizedWebhookToken,
    ResolverProvider<RT> resolverProvider,
    IRepositoryManager<RT> repositoryManager,
    IContainerRuntime<RT> containerRuntime,
    RunUseCase<RT> runUseCase,
    TaskDispatcher<RT> taskDispatcher,
    ILogger<WebhookUseCase<RT>> logger) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Execute(RepositoryResolverName resolverName, RepositoryResolverWebhookBody webhookBody, WebhookToken token) =>
        from _05 in guard(authorizedWebhookToken.Token == token, Error.New("Unauthorized"))
        from resolver in resolverProvider.Provide(resolverName)
        from repositoryContext in resolver.Resolve(webhookBody)
        let repositoryInfo = repositoryContext.RepositoryInfo
        let publisher = repositoryContext.CommitStatusPublisher.IfNone(NullCommitStatusPublisher<RT>.Instance)
        from _07 in publisher.Publish(CommitStatus.Pending())
        from targetDirectory in Eff(() => new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())))
        from _10 in repositoryManager.Clone(repositoryInfo, targetDirectory)
        from _20 in taskDispatcher.Dispatch(
            from _10 in runUseCase.Run(
                    containerRuntime,
                    environment.Environment,
                    targetDirectory,
                    new FileInfo(Path.Combine(targetDirectory.FullName,
                        DomainConstants.DEFAULT_PIPELINE_FILENAME)))
                .OnError(error =>
                    from _10 in Eff(fun(() => logger.LogError(error.ToException(), "Pipeline run failed.")))
                    from _20 in publisher.Publish(CommitStatus.Failure(error.ToString()))
                    select unit)
            from _20 in publisher.Publish(CommitStatus.Success())
            select unit)
        select unit;
}
