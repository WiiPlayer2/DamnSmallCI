using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public interface IContainerRuntimeContext<RT> : IAsyncDisposable where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> CopyFilesFromDirectory(DirectoryInfo directory);

    Aff<RT, IContainer<RT>> NewContainer(TaskContainerInfo containerInfo);
}
