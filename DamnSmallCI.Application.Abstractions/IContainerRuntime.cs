using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public interface IContainerRuntime<RT> where RT : struct, HasCancel<RT>
{
    string Name { get; }

    Aff<RT, IContainerRuntimeContext<RT>> NewContext();
}
