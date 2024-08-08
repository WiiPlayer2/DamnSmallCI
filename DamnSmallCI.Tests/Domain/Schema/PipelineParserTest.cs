using DamnSmallCI.Domain;
using DamnSmallCI.Domain.Schema;
using FluentAssertions;

namespace DamnSmallCI.Tests.Domain.Schema;

[TestClass]
public class PipelineParserTest
{
    [TestMethod]
    public void Parse_WithEmptySchema_ReturnsEmptyPipeline()
    {
        // Arrange
        var schema = YamlNode.Null();
        var expected = new PipelineInfo(List<TaskInfo>());

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public void Parse_WithTask_ReturnsPipelineWithTask()
    {
        // Arrange
        var schema = YamlNode.MapNode(Map(
            ("tasks", YamlNode.MapNode(Map(
                ("test-task", YamlNode.MapNode(Map<string, YamlNode>())))))));
        var expected = new PipelineInfo(List(
            new TaskInfo(TaskName.From("test-task"), List<Step>())));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public void Parse_WithTaskWithSteps_ReturnsPipelineWithTaskWithSteps()
    {
        // Arrange
        var schema = YamlNode.MapNode(Map(
            ("tasks", YamlNode.MapNode(Map(
                ("test-task", YamlNode.MapNode(Map(
                    ("steps", YamlNode.ListNode(List(
                        YamlNode.StringNode("echo hi"))))))))))));
        var expected = new PipelineInfo(List(
            new TaskInfo(
                TaskName.From("test-task"),
                List(
                    Step.From("echo hi")))));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().BeEquivalentTo(expected);
    }
}
