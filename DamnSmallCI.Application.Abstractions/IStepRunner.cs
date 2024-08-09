using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public interface IStepRunner<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Step step);
}
