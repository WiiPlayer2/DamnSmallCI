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
        var schema = Mock.Of<IYamlNode>(x => x[It.IsAny<string>()] == OptionUnsafe<IYamlNode?>.None);
        var expected = new PipelineInfo(List<TaskInfo>());

        // Act
        var result = PipelineParser.Parse(schema);

        // Assert
        result.Case.Should().BeEquivalentTo(expected);
    }
}
