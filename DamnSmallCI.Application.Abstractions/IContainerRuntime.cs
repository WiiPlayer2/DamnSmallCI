using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public interface IContainerRuntime<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, IContainerRuntimeContext<RT>> NewContext();
}
