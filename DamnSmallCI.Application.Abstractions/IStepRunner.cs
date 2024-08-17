using DamnSmallCI.Domain;
using LanguageExt.Effects.Traits;
using Environment = DamnSmallCI.Domain.Environment;

namespace DamnSmallCI.Application;

public interface IStepRunner<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> Run(IProgress<StepOutput> outputProgress, Environment environment, Step step);
}
