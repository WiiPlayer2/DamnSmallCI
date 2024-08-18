using DamnSmallCI.Application;
using DamnSmallCI.Domain;
using DamnSmallCI.Server.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public class WebhookUseCase<RT>(ServerEnvironment environment, ResolverProvider<RT> resolverProvider, IRepositoryManager<RT> repositoryManager, RunUseCase<RT> runUseCase) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Execute(RepositoryResolverName resolverName, RepositoryResolverWebhookBody webhookBody, WebhookToken token) =>
        from resolver in resolverProvider.Provide(resolverName)
        from repositoryInfo in resolver.Resolve(webhookBody)
        from targetDirectory in Eff(() => new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())))
        from _10 in repositoryManager.Clone(repositoryInfo, targetDirectory)
        from _20 in runUseCase.Run(environment.Environment, targetDirectory, new FileInfo(Path.Combine(targetDirectory.FullName, DomainConstants.DEFAULT_PIPELINE_FILENAME)))
        from _30 in Eff(fun(() => targetDirectory.Delete(true)))
        select unit;
}
