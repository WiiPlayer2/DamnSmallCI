using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public class ResolverProvider<RT>(
    IEnumerable<IRepositoryResolver<RT>> resolvers) where RT : struct, HasCancel<RT>
{
    private Eff<Map<RepositoryResolverName, IRepositoryResolver<RT>>> ResolverMap { get; } =
        Eff(() => resolvers.ToDictionary(x => x.Name).ToMap())
            .Memo();

    public Aff<RT, IRepositoryResolver<RT>> Provide(RepositoryResolverName resolverName) =>
        from map in ResolverMap
        from resolver in map.Find(resolverName).ToEff($"Unable to find resolver {resolverName}.")
        select resolver;
}
