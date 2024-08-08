using LanguageExt;

namespace DamnSmallCI.Domain.Schema;

public class PipelineParser
{
    public static Validation<YamlError, PipelineInfo> Parse(YamlNode schema) =>
        from root in schema.Match(
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(schema, "Expected map, got list")),
            x => Success<YamlError, Map<string, YamlNode>>(x.Map),
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(schema, "Expected map, got string")))
        from tasks in root.Find("tasks").Match(
            ParseTasks,
            () => Success<YamlError, Lst<TaskInfo>>(List<TaskInfo>()))
        select new PipelineInfo(tasks);

    private static Validation<YamlError, Lst<TaskInfo>> ParseTasks(YamlNode tasksSchema) =>
        from list in tasksSchema.Match(
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(tasksSchema, "Expected map, got list")),
            x => Success<YamlError, Map<string, YamlNode>>(x.Map),
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(tasksSchema, "Expected map, got string")))
        from tasks in list
            .Pairs
            .Select(kv => ParseTask(kv.Key, kv.Value))
            .Traverse(identity)
            .Map(toList)
        select tasks;

    private static Validation<YamlError, TaskInfo> ParseTask(string name, YamlNode taskSchema) =>
        from map in taskSchema.Match(
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(taskSchema, "Expected map, got list")),
            x => x.Map,
            x => Fail<YamlError, Map<string, YamlNode>>(new YamlError(taskSchema, "Expected map, got string")))
        from steps in map.Find("steps").Match(
            ParseSteps,
            () => List<Step>())
        select new TaskInfo(TaskName.From(name), steps);

    private static Validation<YamlError, Lst<Step>> ParseSteps(YamlNode stepsSchema) =>
        from stepList in stepsSchema.Match(
            x => Success<YamlError, Lst<YamlNode>>(x.List),
            x => Fail<YamlError, Lst<YamlNode>>(new YamlError(stepsSchema, "Expected list, got map")),
            x => Fail<YamlError, Lst<YamlNode>>(new YamlError(stepsSchema, "Expected list, got string")))
        from steps in stepList
            .Select(ParseStep)
            .Traverse(identity)
        select steps;

    private static Validation<YamlError, Step> ParseStep(YamlNode stepSchema) =>
        from @string in stepSchema.Match(
            x => Fail<YamlError, string>(new YamlError(stepSchema, "Expected string, got list")),
            x => Fail<YamlError, string>(new YamlError(stepSchema, "Expected string, got map")),
            x => x.Value)
        select Step.From(@string);
}
