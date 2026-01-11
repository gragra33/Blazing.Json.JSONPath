using Blazing.Json.JSONPath.Utilities;
using Shouldly;
using Xunit;
using static Blazing.Json.JSONPath.Utilities.JsonPathHelper;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Utilities;

/// <summary>
/// Tests for JsonPathHelper RFC 9535 feature detection.
/// Validates that all RFC 9535 features are correctly identified.
/// </summary>
public sealed class JsonPathHelperTests
{
    #region HasFeatures - Filter Selectors

    [Theory]
    [InlineData("$[?@.price < 10]")]
    [InlineData("$.book[?@.author == 'Smith']")]
    [InlineData("$[?@.age >= 18 && @.active]")]
    [InlineData("$.items[?@.optional]")]
    [InlineData("$[?length(@.name) > 5]")]
    public void IsRfc9535Expression_FilterSelectors_ReturnsTrue(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Filter selector in '{jsonPath}' should be detected");
    }

    #endregion

    #region HasFeatures - Built-in Functions

    [Theory]
    [InlineData("$[?length(@.name) > 5]", "length()")]
    [InlineData("$.users[?count(@.tags[*]) == 2]", "count()")]
    [InlineData("$[?match(@.email, '.*@example\\.com$')]", "match()")]
    [InlineData("$.items[?search(@.desc, 'urgent')]", "search()")]
    [InlineData("$[?value(@.items[0]) == 'first']", "value()")]
    public void IsRfc9535Expression_Functions_ReturnsTrue(string jsonPath, string functionName)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Function {functionName} in '{jsonPath}' should be detected");
    }

    [Theory]
    [InlineData("$[?length(@.name) > 5 && count(@.tags[*]) > 0]")]
    [InlineData("$.data[?match(@.code, '^[A-Z]+$') || search(@.name, 'admin')]")]
    public void IsRfc9535Expression_MultipleFunctions_ReturnsTrue(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Multiple functions in '{jsonPath}' should be detected");
    }

    #endregion

    #region HasFeatures - Array Slicing

    [Theory]
    [InlineData("$[0:5]", "Basic slice with start and end")]
    [InlineData("$[1:10:2]", "Slice with step")]
    [InlineData("$[:5]", "Slice with only end")]
    [InlineData("$[2:]", "Slice with only start")]
    [InlineData("$[::2]", "Slice with only step")]
    [InlineData("$[::-1]", "Reverse slice")]
    [InlineData("$[-3:]", "Negative start, open end")]
    [InlineData("$[:-2]", "Open start, negative end")]
    [InlineData("$[-5:-2]", "Both negative indices")]
    public void IsRfc9535Expression_Slicing_ReturnsTrue(string jsonPath, string description)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Slice syntax '{description}' in '{jsonPath}' should be detected");
    }

    [Theory]
    [InlineData("$.items[0:3].name")]
    [InlineData("$.data.values[::2]")]
    [InlineData("$..book[-2:]")]
    public void IsRfc9535Expression_SlicingInComplexPaths_ReturnsTrue(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Slice in complex path '{jsonPath}' should be detected");
    }

    #endregion

    #region HasFeatures - Recursive Descent

    [Theory]
    [InlineData("$..author")]
    [InlineData("$..book[*].title")]
    [InlineData("$.store..price")]
    [InlineData("$..employees[?@.salary > 50000]")]
    [InlineData("$..[*]")]
    public void IsRfc9535Expression_RecursiveDescent_ReturnsTrue(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Recursive descent in '{jsonPath}' should be detected");
    }

    #endregion

    #region HasFeatures - Non-RFC Features (should return FALSE)

    [Theory]
    [InlineData("$")]
    [InlineData("$.store")]
    [InlineData("$.store.book")]
    [InlineData("$['store']['book']")]
    [InlineData("$.store.book[0]")]
    [InlineData("$.store.book[-1]")]
    [InlineData("$.store.book[*]")]
    [InlineData("$[*]")]
    [InlineData("$.a.b.c.d.e")]
    public void IsRfc9535Expression_SimpleNavigation_ReturnsFalse(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeFalse($"Simple navigation in '{jsonPath}' should NOT be detected as RFC feature");
    }

    [Theory]
    [InlineData("$.items[0,1,2]")]
    [InlineData("$.book[0,3,5]")]
    public void IsRfc9535Expression_MultipleIndices_ReturnsFalse(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeFalse($"Multiple indices in '{jsonPath}' should NOT be detected (not RFC specific)");
    }

    #endregion

    #region HasFeatures - Edge Cases

    [Theory]
    [InlineData("$.data['key:value']", "Colon in string key should not trigger slice detection")]
    [InlineData("$['user:id']", "Colon in bracket notation key")]
    [InlineData("$.items[\"title:subtitle\"]", "Colon in double-quoted key")]
    public void IsRfc9535Expression_ColonInStrings_ReturnsFalse(string jsonPath, string reason)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeFalse($"{reason}: '{jsonPath}'");
    }

    [Fact]
    public void IsRfc9535Expression_NullOrEmpty_ThrowsException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => JsonPathHelper.HasFeatures(null!));
        Should.Throw<ArgumentException>(() => JsonPathHelper.HasFeatures(""));
        Should.Throw<ArgumentException>(() => JsonPathHelper.HasFeatures("   "));
    }

    #endregion

    #region AnalyzeComplexity - Simple Queries

    [Theory]
    [InlineData("$")]
    [InlineData("$.store")]
    [InlineData("$.store.book[0]")]
    [InlineData("$['store']['book'][0]['title']")]
    [InlineData("$.a.b.c")]
    [InlineData("$[*]")]
    [InlineData("$.items[0,1,2]")]
    public void AnalyzeComplexity_SimpleNavigation_ReturnsSimple(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Simple, $"'{jsonPath}' should be classified as Simple");
    }

    #endregion

    #region AnalyzeComplexity - Moderate Queries

    [Theory]
    [InlineData("$[0:5]")]
    [InlineData("$[::2]")]
    [InlineData("$[-3:]")]
    [InlineData("$.items[1:10:2]")]
    public void AnalyzeComplexity_Slicing_ReturnsModerate(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Moderate, $"Slicing in '{jsonPath}' should be Moderate");
    }

    [Theory]
    [InlineData("$..author")]
    [InlineData("$..book[*]")]
    [InlineData("$.store..price")]
    public void AnalyzeComplexity_RecursiveDescent_ReturnsModerate(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Moderate, $"Recursive descent in '{jsonPath}' should be Moderate");
    }

    #endregion

    #region AnalyzeComplexity - Complex Queries

    [Theory]
    [InlineData("$[?@.price < 10]")]
    [InlineData("$.book[?@.author == 'Smith']")]
    [InlineData("$[?@.age >= 18 && @.active]")]
    [InlineData("$.items[?@.optional || @.required]")]
    public void AnalyzeComplexity_Filters_ReturnsComplex(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Complex, $"Filter in '{jsonPath}' should be Complex");
    }

    [Theory]
    [InlineData("$[?length(@.name) > 5]")]
    [InlineData("$.users[?count(@.tags[*]) > 2]")]
    [InlineData("$[?match(@.email, '.*@example\\.com$')]")]
    [InlineData("$.items[?search(@.desc, 'urgent')]")]
    public void AnalyzeComplexity_Functions_ReturnsComplex(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Complex, $"Function in '{jsonPath}' should be Complex");
    }

    [Theory]
    [InlineData("$..book[?@.price < 10]")]
    [InlineData("$.store[0:5][?@.active]")]
    [InlineData("$..items[1:10:2][?length(@.name) > 3]")]
    public void AnalyzeComplexity_Mixed_ReturnsComplex(string jsonPath)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(QueryComplexity.Complex, $"Mixed features with filters/functions in '{jsonPath}' should be Complex");
    }

    #endregion

    #region AnalyzeComplexity - Edge Cases

    [Fact]
    public void AnalyzeComplexity_NullOrEmpty_ThrowsException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => JsonPathHelper.AnalyzeComplexity(null!));
        Should.Throw<ArgumentException>(() => JsonPathHelper.AnalyzeComplexity(""));
        Should.Throw<ArgumentException>(() => JsonPathHelper.AnalyzeComplexity("   "));
    }

    #endregion

    #region Real-World Examples

    [Theory]
    [InlineData("$.store.book[?@.price < 10]", QueryComplexity.Complex, "Bookstore price filter")]
    [InlineData("$..book[*].author", QueryComplexity.Moderate, "Recursive descent with wildcard")]
    [InlineData("$.products[0:10][?@.inStock]", QueryComplexity.Complex, "Pagination with filter")]
    [InlineData("$..employees[?@.salary > 50000 && @.department == 'Engineering']", QueryComplexity.Complex, "Complex employee filter")]
    [InlineData("$.items[-5:][?length(@.name) > 10]", QueryComplexity.Complex, "Last 5 items with length filter")]
    public void AnalyzeComplexity_RealWorldExamples_CorrectClassification(string jsonPath, QueryComplexity expected, string scenario)
    {
        // Act
        var result = JsonPathHelper.AnalyzeComplexity(jsonPath);

        // Assert
        result.ShouldBe(expected, $"Scenario '{scenario}': '{jsonPath}'");
    }

    #endregion

    #region Feature Detection - Combined Features

    [Theory]
    [InlineData("$..book[0:5][?@.price < 10]", "Recursive descent + slicing + filter")]
    [InlineData("$.items[::2][?length(@.name) > 5]", "Slicing + function")]
    [InlineData("$..data[?count(@.tags[*]) > 0]", "Recursive descent + count function")]
    public void IsRfc9535Expression_CombinedFeatures_ReturnsTrue(string jsonPath, string features)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"Combined features ({features}) in '{jsonPath}' should be detected");
    }

    #endregion

    #region Logical Operators Detection (via filters)

    [Theory]
    [InlineData("$[?@.a && @.b]", "AND operator")]
    [InlineData("$[?@.x || @.y]", "OR operator")]
    [InlineData("$[?!@.inactive]", "NOT operator")]
    [InlineData("$[?(@.a && @.b) || @.c]", "Mixed operators")]
    public void IsRfc9535Expression_LogicalOperators_DetectedViaFilter(string jsonPath, string operator_)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeTrue($"{operator_} in filter '{jsonPath}' should be detected");
    }

    #endregion

    #region All RFC 9535 Functions Coverage

    [Fact]
    public void IsRfc9535Expression_AllRfcFunctions_Detected()
    {
        // Arrange
        var functionsToTest = new Dictionary<string, string>
        {
            ["length"] = "$[?length(@.name) > 0]",
            ["count"] = "$[?count(@.items[*]) > 0]",
            ["match"] = "$[?match(@.email, '.*')]",
            ["search"] = "$[?search(@.text, 'keyword')]",
            ["value"] = "$[?value(@.items[0]) == 'first']"
        };

        // Act & Assert
        foreach (var (functionName, query) in functionsToTest)
        {
            var result = JsonPathHelper.HasFeatures(query);
            result.ShouldBeTrue($"RFC 9535 function '{functionName}' should be detected in '{query}'");
        }
    }

    #endregion

    #region False Positives Prevention

    [Theory]
    [InlineData("$.length", "Property named 'length' (no parentheses)")]
    [InlineData("$.count", "Property named 'count' (no parentheses)")]
    [InlineData("$.match", "Property named 'match' (no parentheses)")]
    [InlineData("$.data.length.value", "Nested property with 'length'")]
    public void IsRfc9535Expression_SimilarNamesWithoutParens_ReturnsFalse(string jsonPath, string description)
    {
        // Act
        var result = JsonPathHelper.HasFeatures(jsonPath);

        // Assert
        result.ShouldBeFalse($"{description}: '{jsonPath}' should NOT be detected as function");
    }

    #endregion

    #region Span Overload Tests

    [Theory]
    [InlineData("$[?@.price < 10]")]
    [InlineData("$.book[?@.author == 'Smith']")]
    [InlineData("$[0:5]")]
    [InlineData("$..author")]
    [InlineData("$[?length(@.name) > 5]")]
    public void IsRfc9535Expression_SpanOverload_SameResultAsString(string jsonPath)
    {
        // Arrange
        ReadOnlySpan<char> span = jsonPath.AsSpan();

        // Act
        var stringResult = JsonPathHelper.HasFeatures(jsonPath);
        var spanResult = JsonPathHelper.HasFeatures(span);

        // Assert
        spanResult.ShouldBe(stringResult, $"Span and string overloads should return same result for '{jsonPath}'");
        spanResult.ShouldBeTrue($"Both overloads should detect RFC feature in '{jsonPath}'");
    }

    [Theory]
    [InlineData("$")]
    [InlineData("$.store")]
    [InlineData("$.store.book[0]")]
    [InlineData("$['name']")]
    public void IsRfc9535Expression_SpanOverload_NonRfcQueries_SameResultAsString(string jsonPath)
    {
        // Arrange
        ReadOnlySpan<char> span = jsonPath.AsSpan();

        // Act
        var stringResult = JsonPathHelper.HasFeatures(jsonPath);
        var spanResult = JsonPathHelper.HasFeatures(span);

        // Assert
        spanResult.ShouldBe(stringResult, $"Span and string overloads should return same result for '{jsonPath}'");
        spanResult.ShouldBeFalse($"Both overloads should NOT detect RFC feature in '{jsonPath}'");
    }

    [Theory]
    [InlineData("$.store", QueryComplexity.Simple)]
    [InlineData("$[0:5]", QueryComplexity.Moderate)]
    [InlineData("$..author", QueryComplexity.Moderate)]
    [InlineData("$[?@.price < 10]", QueryComplexity.Complex)]
    [InlineData("$[?length(@.name) > 5]", QueryComplexity.Complex)]
    public void AnalyzeComplexity_SpanOverload_SameResultAsString(string jsonPath, QueryComplexity expected)
    {
        // Arrange
        ReadOnlySpan<char> span = jsonPath.AsSpan();

        // Act
        var stringResult = JsonPathHelper.AnalyzeComplexity(jsonPath);
        var spanResult = JsonPathHelper.AnalyzeComplexity(span);

        // Assert
        spanResult.ShouldBe(stringResult, $"Span and string overloads should return same result for '{jsonPath}'");
        spanResult.ShouldBe(expected, $"Both overloads should return {expected} for '{jsonPath}'");
    }

    [Fact]
    public void IsRfc9535Expression_SpanOverload_SupportsSubstring()
    {
        // Arrange
        string fullPath = "prefix_$[?@.price < 10]_suffix";
        ReadOnlySpan<char> spanSlice = fullPath.AsSpan(7, 18); // Just the JSONPath part

        // Act
        var result = JsonPathHelper.HasFeatures(spanSlice);

        // Assert
        result.ShouldBeTrue("Span slice containing filter should be detected");
    }

    [Fact]
    public void AnalyzeComplexity_SpanOverload_SupportsSubstring()
    {
        // Arrange
        string fullPath = "query:$[?@.price < 10]:end";
        ReadOnlySpan<char> spanSlice = fullPath.AsSpan(6, 18); // Just the JSONPath part

        // Act
        var result = JsonPathHelper.AnalyzeComplexity(spanSlice);

        // Assert
        result.ShouldBe(QueryComplexity.Complex, "Span slice containing filter should be Complex");
    }

    #endregion
}
