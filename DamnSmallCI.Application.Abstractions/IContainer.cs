using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public interface IContainer<RT> : IStepRunner<RT>, IAsyncDisposable where RT : struct, HasCancel<RT> { }
