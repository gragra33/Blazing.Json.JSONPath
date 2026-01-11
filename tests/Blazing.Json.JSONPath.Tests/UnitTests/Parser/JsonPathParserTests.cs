using Blazing.Json.JSONPath.Exceptions;
using Blazing.Json.JSONPath.Parser;
using Blazing.Json.JSONPath.Parser.Nodes;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Parser;

/// <summary>
/// Unit tests for the JsonPathParser.
/// Tests parsing of complete JSONPath queries.
/// </summary>
public class JsonPathParserTests
{
    [Fact]
    public void Parse_RootOnly_ReturnsEmptyQuery()
    {
        // Arrange
        const string query = "$";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.ShouldNotBeNull();
        result.IsEmpty.ShouldBeTrue();
        result.SegmentCount.ShouldBe(0);
    }

    [Fact]
    public void Parse_SimpleDotNotation_ReturnsSingleSegment()
    {
        // Arrange
        const string query = "$.store";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        segment.SelectorCount.ShouldBe(1);
        var selector = segment.Selectors[0].ShouldBeOfType<NameSelector>();
        selector.Name.ShouldBe("store");
    }

    [Fact]
    public void Parse_NestedDotNotation_ReturnsMultipleSegments()
    {
        // Arrange
        const string query = "$.store.book";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(2);
        
        var segment1 = result.Segments[0].ShouldBeOfType<ChildSegment>();
        segment1.Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("store");
        
        var segment2 = result.Segments[1].ShouldBeOfType<ChildSegment>();
        segment2.Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("book");
    }

    [Fact]
    public void Parse_BracketWildcard_ReturnsWildcardSelector()
    {
        // Arrange
        const string query = "$[*]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        segment.Selectors[0].ShouldBeOfType<WildcardSelector>();
    }

    [Fact]
    public void Parse_DotWildcard_ReturnsWildcardSelector()
    {
        // Arrange
        const string query = "$.*";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        segment.Selectors[0].ShouldBeOfType<WildcardSelector>();
    }

    [Fact]
    public void Parse_BracketNameSelector_ReturnsSingleQuotedName()
    {
        // Arrange
        const string query = "$['name']";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<NameSelector>();
        selector.Name.ShouldBe("name");
    }

    [Fact]
    public void Parse_BracketNameSelector_ReturnsDoubleQuotedName()
    {
        // Arrange
        const string query = "$[\"name\"]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<NameSelector>();
        selector.Name.ShouldBe("name");
    }

    [Fact]
    public void Parse_IndexSelector_ReturnsPositiveIndex()
    {
        // Arrange
        const string query = "$[0]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<IndexSelector>();
        selector.Index.ShouldBe(0);
    }

    [Fact]
    public void Parse_IndexSelector_ReturnsNegativeIndex()
    {
        // Arrange
        const string query = "$[-1]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<IndexSelector>();
        selector.Index.ShouldBe(-1);
    }

    [Fact]
    public void Parse_MultipleSelectors_ReturnsAllSelectors()
    {
        // Arrange
        const string query = "$[0,1,2]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        segment.SelectorCount.ShouldBe(3);
        
        segment.Selectors[0].ShouldBeOfType<IndexSelector>().Index.ShouldBe(0);
        segment.Selectors[1].ShouldBeOfType<IndexSelector>().Index.ShouldBe(1);
        segment.Selectors[2].ShouldBeOfType<IndexSelector>().Index.ShouldBe(2);
    }

    [Fact]
    public void Parse_DescendantSegment_ReturnsDescendantNode()
    {
        // Arrange
        const string query = "$..author";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<DescendantSegment>();
        segment.Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("author");
    }

    [Fact]
    public void Parse_DescendantWildcard_ReturnsDescendantWithWildcard()
    {
        // Arrange
        const string query = "$..[*]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(1);
        var segment = result.Segments[0].ShouldBeOfType<DescendantSegment>();
        segment.Selectors[0].ShouldBeOfType<WildcardSelector>();
    }

    [Fact]
    public void Parse_ComplexPath_ReturnsAllSegments()
    {
        // Arrange
        const string query = "$.store.book[*].author";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        result.SegmentCount.ShouldBe(4);
        
        result.Segments[0].ShouldBeOfType<ChildSegment>()
            .Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("store");
        
        result.Segments[1].ShouldBeOfType<ChildSegment>()
            .Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("book");
        
        result.Segments[2].ShouldBeOfType<ChildSegment>()
            .Selectors[0].ShouldBeOfType<WildcardSelector>();
        
        result.Segments[3].ShouldBeOfType<ChildSegment>()
            .Selectors[0].ShouldBeOfType<NameSelector>().Name.ShouldBe("author");
    }

    [Fact]
    public void Parse_NullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => JsonPathParser.Parse((string)null!));
    }

    [Fact]
    public void Parse_EmptyQuery_ThrowsSyntaxException()
    {
        // Arrange
        const string query = "";

        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => JsonPathParser.Parse(query));
    }

    [Fact]
    public void Parse_NoRootIdentifier_ThrowsSyntaxException()
    {
        // Arrange
        const string query = ".store";

        // Act & Assert
        var exception = Should.Throw<JsonPathSyntaxException>(() => JsonPathParser.Parse(query));
        exception.Message.ShouldContain("must start with '$'");
    }

    [Fact]
    public void Parse_UnclosedBracket_ThrowsSyntaxException()
    {
        // Arrange
        const string query = "$[0";

        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => JsonPathParser.Parse(query));
    }

    [Fact]
    public void Parse_InvalidSelector_ThrowsSyntaxException()
    {
        // Arrange
        const string query = "$[#]";

        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => JsonPathParser.Parse(query));
    }

    [Fact]
    public void Parse_EscapedStringInSelector_UnescapesCorrectly()
    {
        // Arrange
        const string query = "$['test\\'value']";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var selector = segment.Selectors[0].ShouldBeOfType<NameSelector>();
        selector.Name.ShouldBe("test'value");
    }
}
