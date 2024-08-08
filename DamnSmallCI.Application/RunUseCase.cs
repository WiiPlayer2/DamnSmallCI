using LanguageExt;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class RunUseCase<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(FileInfo pipelineFile)
    {
        throw new NotImplementedException();
    }
}
