namespace DamnSmallCI.Domain.Schema;

public class PipelineParser
{
    public static Validation<YamlError, PipelineInfo> Parse(YamlNode schema) =>
        from root in AsMap(schema)
        from tasks in root.Find("tasks").Match(
            ParseTasks,
            () => Success<YamlError, Lst<TaskInfo>>(List<TaskInfo>()))
        select new PipelineInfo(tasks);

    private static Validation<YamlError, Lst<YamlNode>> AsList(YamlNode node) => node.Match(
        x => x.List,
        x => Fail<Lst<YamlNode>>(node, "Expected list, got map"),
        x => Fail<Lst<YamlNode>>(node, "Expected list, got string"));

    private static Validation<YamlError, Map<string, YamlNode>> AsMap(YamlNode node) => node.Match(
        x => Fail<Map<string, YamlNode>>(node, "Expected map, got list"),
        x => x.Map,
        x => Fail<Map<string, YamlNode>>(node, "Expected map, got string"));

    private static Validation<YamlError, string> AsString(YamlNode node) => node.Match(
        x => Fail<string>(node, "Expected string, got list"),
        x => Fail<string>(node, "Expected string, got map"),
        x => x.Value);

    private static Validation<YamlError, T> Fail<T>(YamlNode node, string message) =>
        Fail<YamlError, T>(new YamlError(node, message));

    private static Validation<YamlError, Step> ParseStep(YamlNode stepSchema) =>
        from @string in AsString(stepSchema)
        select Step.From(@string);

    private static Validation<YamlError, Lst<Step>> ParseSteps(YamlNode stepsSchema) =>
        from stepList in AsList(stepsSchema)
        from steps in stepList
            .Select(ParseStep)
            .Traverse(identity)
        select steps;

    private static Validation<YamlError, TaskInfo> ParseTask(string name, YamlNode taskSchema) =>
        from map in AsMap(taskSchema)
        from steps in map.Find("steps").Match(
            ParseSteps,
            () => List<Step>())
        select new TaskInfo(TaskName.From(name), steps);

    private static Validation<YamlError, Lst<TaskInfo>> ParseTasks(YamlNode tasksSchema) =>
        from list in AsMap(tasksSchema)
        from tasks in list
            .Pairs
            .Select(kv => ParseTask(kv.Key, kv.Value))
            .Traverse(identity)
            .Map(toList)
        select tasks;
}
