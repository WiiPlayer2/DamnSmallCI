using DamnSmallCI.Domain;
using DamnSmallCI.Domain.Schema;
using FluentAssertions;

namespace DamnSmallCI.Tests.Domain.Schema;

[TestClass]
public class PipelineParserTest
{
    [ClassInitialize]
    public static void Initialize(TestContext ctx)
    {
        AssertionOptions.FormattingOptions.MaxDepth = 10;
    }

    [TestMethod]
    public void Parse_WithEmptySchema_ReturnsEmptyPipeline()
    {
        // Arrange
        var schema = YamlNode.Null();
        var expected = new PipelineInfo(List<TaskInfo>());

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_WithTask_ReturnsPipelineWithTask()
    {
        // Arrange
        var schema = YamlNode.Map(Map(
            ("tasks", YamlNode.List(List(
                YamlNode.Map(Map(
                    ("name", YamlNode.String("test-task")))))))));
        var expected = new PipelineInfo(List(
            new TaskInfo(
                TaskName.From("test-task"),
                None,
                List<Step>())));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_WithTaskWithImage_ReturnsPipelineWithTaskWithImage()
    {
        // Arrange
        var schema = YamlNode.Map(Map(
            ("tasks", YamlNode.List(List(
                YamlNode.Map(Map(
                    ("name", YamlNode.String("test-task")),
                    ("container", YamlNode.Map(Map(
                        ("image", YamlNode.String("test-image")))))
                )))
            ))));
        var expected = new PipelineInfo(List(
            new TaskInfo(
                TaskName.From("test-task"),
                Some(new TaskContainerInfo(
                    ImageName.From("test-image"),
                    None)),
                List<Step>())));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_WithTaskWithImageAndEntrypoint_ReturnsPipelineWithTaskWithImageAndEntrypoint()
    {
        // Arrange
        var schema = YamlNode.Map(Map(
            ("tasks", YamlNode.List(List(
                YamlNode.Map(Map(
                    ("name", YamlNode.String("test-task")),
                    ("container", YamlNode.Map(Map(
                        ("image", YamlNode.String("test-image")),
                        ("entrypoint", YamlNode.List(List(
                            YamlNode.String("cmd")))))))
                )))
            ))));
        var expected = new PipelineInfo(List(
            new TaskInfo(
                TaskName.From("test-task"),
                Some(new TaskContainerInfo(
                    ImageName.From("test-image"),
                    ContainerEntrypoint.From(Seq1("cmd")))),
                List<Step>())));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Parse_WithTaskWithSteps_ReturnsPipelineWithTaskWithSteps()
    {
        // Arrange
        var schema = YamlNode.Map(Map(
            ("tasks", YamlNode.List(List(
                YamlNode.Map(Map(
                    ("name", YamlNode.String("test-task")),
                    ("steps", YamlNode.List(List(
                        YamlNode.String("echo hi")))))))))));
        var expected = new PipelineInfo(List(
            new TaskInfo(
                TaskName.From("test-task"),
                None,
                List(
                    Step.From("echo hi")))));

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().Be(expected);
    }
}
