using Blazing.Json.JSONPath.Analysis;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Analysis;

/// <summary>
/// Unit tests for the JsonPathComplexityAnalyzer to ensure fast-path routing works correctly.
/// </summary>
public class JsonPathComplexityAnalyzerTests
{
    [Theory]
    [InlineData("$.data[*]")]
    [InlineData("$.result[*].customer")]
    [InlineData("$.users[*].orders[*]")]
    [InlineData("$.store.book[*]")]
    [InlineData("$.a.b.c[*].d.e[*]")]
    public void Analyze_SimplePath_ReturnsSimple(string jsonPath)
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Simple);
    }

    [Theory]
    [InlineData("$..author")] // Recursive descent
    [InlineData("$..book[*]")] // Recursive descent with wildcard
    [InlineData("$.book[?@.price<10]")] // Filter expression
    [InlineData("$.arr[-1]")] // Negative index
    [InlineData("$.arr[1:5:2]")] // Array slice
    [InlineData("$.book[0,1]")] // Index selection
    [InlineData("$.book[0]")] // Single index
    [InlineData("$['name']")] // Quoted selector
    [InlineData("$[\"name\"]")] // Double-quoted selector
    [InlineData("$[?(length(@.title)>10)]")] // Function call
    [InlineData("$.book[?@.isbn]")] // Existence test
    [InlineData("$.book[?@.price < 10 && @.category == 'fiction']")] // Complex filter
    public void Analyze_ComplexPath_ReturnsComplex(string jsonPath)
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Fact]
    public void Analyze_EmptyString_ReturnsComplex()
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze("");

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Fact]
    public void Analyze_OnlyRoot_ReturnsComplex()
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze("$");

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Fact]
    public void Analyze_NoRootIdentifier_ReturnsComplex()
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(".data[*]");

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Theory]
    [InlineData("$[*].name")] // Starts with bracket wildcard
    [InlineData("$.data[*].items[*].id")] // Multiple nested wildcards
    public void Analyze_SimplePath_WithVariations_ReturnsSimple(string jsonPath)
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Simple);
    }

    [Theory]
    [InlineData("$.data[@]")] // Current identifier
    [InlineData("$.data[?]")] // Question mark without expression
    [InlineData("$.data[(]")] // Parenthesis
    [InlineData("$.data[==]")] // Operator
    public void Analyze_PathWithFilterElements_ReturnsComplex(string jsonPath)
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Fact]
    public void Analyze_Span_SimplePath_ReturnsSimple()
    {
        // Arrange
        ReadOnlySpan<char> jsonPath = "$.data[*]".AsSpan();

        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Simple);
    }

    [Fact]
    public void Analyze_Span_ComplexPath_ReturnsComplex()
    {
        // Arrange
        ReadOnlySpan<char> jsonPath = "$..author".AsSpan();

        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }

    [Theory]
    [InlineData("$.data[*")] // Unclosed bracket
    [InlineData("$.data[**]")] // Double wildcard
    public void Analyze_MalformedPath_ReturnsComplex(string jsonPath)
    {
        // Act
        var complexity = JsonPathComplexityAnalyzer.Analyze(jsonPath);

        // Assert
        complexity.ShouldBe(JsonPathComplexity.Complex);
    }
}
