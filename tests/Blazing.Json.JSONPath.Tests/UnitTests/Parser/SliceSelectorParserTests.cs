using Blazing.Json.JSONPath.Parser;
using Blazing.Json.JSONPath.Parser.Nodes;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Parser;

/// <summary>
/// Unit tests for parsing slice selectors.
/// </summary>
public class SliceSelectorParserTests
{
    [Fact]
    public void Parse_SliceWithStartAndEnd_ReturnsCorrectSlice()
    {
        // Arrange
        const string query = "$[1:5]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBe(1);
        selector.End.ShouldBe(5);
        selector.Step.ShouldBe(1);
    }

    [Fact]
    public void Parse_SliceWithStartEndAndStep_ReturnsCorrectSlice()
    {
        // Arrange
        const string query = "$[1:10:2]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBe(1);
        selector.End.ShouldBe(10);
        selector.Step.ShouldBe(2);
    }

    [Fact]
    public void Parse_SliceOnlyEnd_ReturnsNullStart()
    {
        // Arrange
        const string query = "$[:5]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBeNull();
        selector.End.ShouldBe(5);
        selector.Step.ShouldBe(1);
    }

    [Fact]
    public void Parse_SliceStartOnly_ReturnsNullEnd()
    {
        // Arrange
        const string query = "$[2:]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBe(2);
        selector.End.ShouldBeNull();
        selector.Step.ShouldBe(1);
    }

    [Fact]
    public void Parse_SliceOnlyStep_ReturnsNullStartAndEnd()
    {
        // Arrange
        const string query = "$[::2]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBeNull();
        selector.End.ShouldBeNull();
        selector.Step.ShouldBe(2);
    }

    [Fact]
    public void Parse_SliceNegativeStep_ReturnsNegativeStep()
    {
        // Arrange
        const string query = "$[::-1]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBeNull();
        selector.End.ShouldBeNull();
        selector.Step.ShouldBe(-1);
    }

    [Fact]
    public void Parse_SliceNegativeIndices_ReturnsNegativeValues()
    {
        // Arrange
        const string query = "$[-5:-1]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBe(-5);
        selector.End.ShouldBe(-1);
        selector.Step.ShouldBe(1);
    }

    [Theory]
    [InlineData("$[0:5:1]", 0, 5, 1)]
    [InlineData("$[1:10:2]", 1, 10, 2)]
    [InlineData("$[::3]", null, null, 3)]
    [InlineData("$[2:]", 2, null, 1)]
    [InlineData("$[:8]", null, 8, 1)]
    public void Parse_VariousSlices_ReturnsCorrectValues(string query, int? expectedStart, int? expectedEnd, int expectedStep)
    {
        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<SliceSelector>();
        selector.Start.ShouldBe(expectedStart);
        selector.End.ShouldBe(expectedEnd);
        selector.Step.ShouldBe(expectedStep);
    }
}
