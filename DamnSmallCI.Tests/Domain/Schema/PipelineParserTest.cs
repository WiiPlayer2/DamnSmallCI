using DamnSmallCI.Domain;
using DamnSmallCI.Domain.Schema;
using FluentAssertions;
using LanguageExt;
using Moq;

namespace DamnSmallCI.Tests.Domain.Schema;

[TestClass]
public class PipelineParserTest
{
    [TestMethod]
    public void Parse_WithEmptySchema_ReturnsEmptyPipeline()
    {
        // Arrange
        var schema = YamlNode.MapNode(Map<string, YamlNode>());
        var expected = new PipelineInfo(List<TaskInfo>());

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().BeEquivalentTo(expected);
    }
}
