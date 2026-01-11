using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// RFC 9535 compliance tests for 2-parameter array slice syntax: [start:end]
/// Validates that step defaults to 1 when omitted, as per RFC 9535 Section 2.3.4.
/// </summary>
public sealed class ArraySliceTwoParameterTests
{
    private readonly JsonPathEvaluator _evaluator = new();

    #region Basic 2-Parameter Slices (start:end with default step=1)

    [Fact]
    public void Evaluate_TwoParamSlice_StartAndEnd_DefaultStepOne()
    {
        // RFC 9535: $[1:4] should select indices 1, 2, 3 (step defaults to 1)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[1:4]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$[1]");
        result[1].Value.GetInt32().ShouldBe(2);
        result[1].NormalizedPath.ShouldBe("$[2]");
        result[2].Value.GetInt32().ShouldBe(3);
        result[2].NormalizedPath.ShouldBe("$[3]");
    }

    [Fact]
    public void Evaluate_TwoParamSlice_ZeroStart_SelectsFromBeginning()
    {
        // RFC 9535: $[0:3] should select first 3 elements
        // Arrange
        var json = JsonDocument.Parse("""["a", "b", "c", "d", "e"]""").RootElement;
        var query = JsonPathParser.Parse("$[0:3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetString().ShouldBe("a");
        result[1].Value.GetString().ShouldBe("b");
        result[2].Value.GetString().ShouldBe("c");
    }

    [Fact]
    public void Evaluate_TwoParamSlice_MidRange_SelectsCorrectElements()
    {
        // RFC 9535: $[2:7] should select indices 2 through 6
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[2:7]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(5);
        result[0].Value.GetInt32().ShouldBe(2);
        result[1].Value.GetInt32().ShouldBe(3);
        result[2].Value.GetInt32().ShouldBe(4);
        result[3].Value.GetInt32().ShouldBe(5);
        result[4].Value.GetInt32().ShouldBe(6);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_ToEnd_SelectsUntilEndIndex()
    {
        // RFC 9535: $[5:10] should select indices 5 through 9 (10 is exclusive)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[5:10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(5);
        result[0].Value.GetInt32().ShouldBe(5);
        result[4].Value.GetInt32().ShouldBe(9);
    }

    #endregion

    #region 2-Parameter Slices with Negative Indices

    [Fact]
    public void Evaluate_TwoParamSlice_NegativeStart_CountsFromEnd()
    {
        // RFC 9535: $[-5:-2] should select from 5th-last to 2nd-last (exclusive)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[-5:-2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(5); // -5 -> index 5
        result[1].Value.GetInt32().ShouldBe(6); // -4 -> index 6
        result[2].Value.GetInt32().ShouldBe(7); // -3 -> index 7
    }

    [Fact]
    public void Evaluate_TwoParamSlice_NegativeStartOpenEnd_SelectsLastNElements()
    {
        // RFC 9535: $[-3:] should select last 3 elements (from -3 to end)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[-3:]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(7); // -3 -> index 7
        result[0].NormalizedPath.ShouldBe("$[7]");
        result[1].Value.GetInt32().ShouldBe(8); // -2 -> index 8
        result[1].NormalizedPath.ShouldBe("$[8]");
        result[2].Value.GetInt32().ShouldBe(9); // -1 -> index 9
        result[2].NormalizedPath.ShouldBe("$[9]");
    }

    [Fact]
    public void Evaluate_TwoParamSlice_NegativeEnd_ExcludesFromEnd()
    {
        // RFC 9535: $[2:-2] should select from index 2 to 2nd-last (exclusive)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[2:-2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(6);
        result[0].Value.GetInt32().ShouldBe(2);
        result[5].Value.GetInt32().ShouldBe(7); // up to but not including -2 (index 8)
    }

    [Fact]
    public void Evaluate_TwoParamSlice_BothNegative_SelectsCorrectRange()
    {
        // RFC 9535: $[-8:-3] should work with both negative indices
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[-8:-3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(5);
        result[0].Value.GetInt32().ShouldBe(2); // -8 -> index 2
        result[4].Value.GetInt32().ShouldBe(6); // -3 -> index 7 (exclusive)
    }

    [Fact]
    public void Evaluate_TwoParamSlice_MixedNegativePositive_WorksCorrectly()
    {
        // RFC 9535: $[-7:5] mixing negative start and positive end
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[-7:5]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetInt32().ShouldBe(3); // -7 -> index 3
        result[1].Value.GetInt32().ShouldBe(4); // up to index 5 (exclusive)
    }

    #endregion

    #region Edge Cases for 2-Parameter Slices

    [Fact]
    public void Evaluate_TwoParamSlice_StartEqualsEnd_ReturnsEmpty()
    {
        // RFC 9535: $[3:3] should return empty (no elements)
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[3:3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_StartGreaterThanEnd_ReturnsEmpty()
    {
        // RFC 9535: $[5:2] should return empty with positive step
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[5:2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_EndExceedsLength_ClampedToLength()
    {
        // RFC 9535: $[2:100] should clamp to array length
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[2:100]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(4);
        result[0].Value.GetInt32().ShouldBe(2);
        result[3].Value.GetInt32().ShouldBe(5);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_NegativeStartExceedsLength_ClampsToZero()
    {
        // RFC 9535: $[-100:3] should clamp negative start to 0
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[-100:3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(0);
        result[2].Value.GetInt32().ShouldBe(2);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_OnEmptyArray_ReturnsEmpty()
    {
        // RFC 9535: Any slice on empty array returns empty
        // Arrange
        var json = JsonDocument.Parse("""[]""").RootElement;
        var query = JsonPathParser.Parse("$[0:5]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_OnSingleElement_WorksCorrectly()
    {
        // RFC 9535: $[0:1] on single element array
        // Arrange
        var json = JsonDocument.Parse("""[42]""").RootElement;
        var query = JsonPathParser.Parse("$[0:1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetInt32().ShouldBe(42);
    }

    #endregion

    #region Complex JSON Structures

    [Fact]
    public void Evaluate_TwoParamSlice_OnObjectArray_SelectsObjects()
    {
        // RFC 9535: 2-param slice works on arrays of objects
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "users": [
                {"id": 1, "name": "Alice"},
                {"id": 2, "name": "Bob"},
                {"id": 3, "name": "Charlie"},
                {"id": 4, "name": "Diana"},
                {"id": 5, "name": "Eve"}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.users[1:4]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Bob");
        result[1].Value.GetProperty("name").GetString().ShouldBe("Charlie");
        result[2].Value.GetProperty("name").GetString().ShouldBe("Diana");
    }

    [Fact]
    public void Evaluate_TwoParamSlice_OnNestedArrays_SelectsCorrectly()
    {
        // RFC 9535: 2-param slice on nested structure
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "matrix": [
                [1, 2, 3],
                [4, 5, 6],
                [7, 8, 9],
                [10, 11, 12]
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.matrix[1:3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetArrayLength().ShouldBe(3);
        result[0].Value[0].GetInt32().ShouldBe(4);
        result[1].Value[0].GetInt32().ShouldBe(7);
    }

    #endregion

    #region Combined with Other Selectors

    [Fact]
    public void Evaluate_TwoParamSlice_FollowedByMemberAccess_WorksCorrectly()
    {
        // RFC 9535: Chaining 2-param slice with member access
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "items": [
                {"value": 10},
                {"value": 20},
                {"value": 30},
                {"value": 40},
                {"value": 50}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.items[1:4].value");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(20);
        result[1].Value.GetInt32().ShouldBe(30);
        result[2].Value.GetInt32().ShouldBe(40);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_WithMultipleSelectors_SelectsAll()
    {
        // RFC 9535: Combining 2-param slice with other selectors
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]""").RootElement;
        var query = JsonPathParser.Parse("$[0, 2:5, 8]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(5);
        result[0].Value.GetInt32().ShouldBe(0);  // index selector
        result[1].Value.GetInt32().ShouldBe(2);  // slice start
        result[2].Value.GetInt32().ShouldBe(3);
        result[3].Value.GetInt32().ShouldBe(4);  // slice end
        result[4].Value.GetInt32().ShouldBe(8);  // index selector
    }

    #endregion

    #region RFC 9535 Specific Examples

    [Theory]
    [InlineData("$[0:2]", new[] { "a", "b" })]
    [InlineData("$[1:3]", new[] { "b", "c" })]
    [InlineData("$[2:5]", new[] { "c", "d", "e" })]
    [InlineData("$[-3:-1]", new[] { "e", "f" })]  // Fixed: -3 is index 4 (e), -1 is index 6 (g), so select 4,5
    [InlineData("$[-2:7]", new[] { "f", "g" })]
    public void Evaluate_TwoParamSlice_RFCExamples_ProduceExpectedResults(
        string queryString, 
        string[] expected)
    {
        // RFC 9535 Table 9: Array slice examples
        // Arrange
        var json = JsonDocument.Parse("""["a", "b", "c", "d", "e", "f", "g"]""").RootElement;
        var query = JsonPathParser.Parse(queryString);

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            result[i].Value.GetString().ShouldBe(expected[i]);
        }
    }

    #endregion

    #region Type Safety

    [Fact]
    public void Evaluate_TwoParamSlice_OnNonArray_ReturnsEmpty()
    {
        // RFC 9535: Slice on non-array returns empty
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1, "b": 2}""").RootElement;
        var query = JsonPathParser.Parse("$[0:2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_OnString_ReturnsEmpty()
    {
        // RFC 9535: Slice selector doesn't apply to strings
        // Arrange
        var json = JsonDocument.Parse("\"test\"").RootElement;  // Fixed: double-quoted string for JSON
        var query = JsonPathParser.Parse("$[0:4]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_TwoParamSlice_OnNumber_ReturnsEmpty()
    {
        // RFC 9535: Slice selector doesn't apply to numbers
        // Arrange
        var json = JsonDocument.Parse("12345").RootElement;
        var query = JsonPathParser.Parse("$[0:2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    #endregion
}
