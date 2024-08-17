using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Server.Application;

public class ResolverProvider<RT>(IEnumerable<IRepositoryResolver<RT>> resolvers) where RT : struct, HasCancel<RT>
{
    public Aff<RT, IRepositoryResolver<RT>> Provide(RepositoryResolverName resolverName) => throw new NotImplementedException();
}
