using DamnSmallCI.Domain;
using DamnSmallCI.Domain.Schema;
using LanguageExt.Common;
using LanguageExt.Effects.Traits;

namespace DamnSmallCI.Application;

public class RunUseCase<RT>(IYamlReader yamlReader, PipelineRunner<RT> pipelineRunner) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Run(DirectoryInfo contextDirectory, FileInfo pipelineFile) =>
        from fileContent in Aff((RT rt) => File.ReadAllTextAsync(pipelineFile.FullName).ToValue())
        from pipelineYaml in yamlReader.Read(fileContent)
        from pipeline in PipelineParser.Parse(pipelineYaml).ToEff(errors => Error.Many(errors.Select(x => Error.New($"{x.Error}"))))
        let outputProgress = new Progress<PipelineOutput>(x => Console.WriteLine(x))
        from _10 in pipelineRunner.Run(outputProgress, contextDirectory, pipeline)
        select unit;
}
