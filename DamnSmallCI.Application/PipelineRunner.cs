using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class PipelineRunner<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(PipelineInfo pipeline) => throw new NotImplementedException();
}
