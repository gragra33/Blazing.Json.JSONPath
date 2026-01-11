using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// Tests for the <see cref="JsonPathEvaluator"/> class.
/// </summary>
public sealed class JsonPathEvaluatorTests
{
    private readonly JsonPathEvaluator _evaluator = new();

    #region Root-Only Queries

    [Fact]
    public void Evaluate_RootOnly_ReturnsRootNode()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var query = JsonPathParser.Parse("$");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$");
        result[0].Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public void Evaluate_RootOnly_WithArray_ReturnsRootArray()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var query = JsonPathParser.Parse("$");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$");
        result[0].Value.ValueKind.ShouldBe(JsonValueKind.Array);
        result[0].Value.GetArrayLength().ShouldBe(3);
    }

    #endregion

    #region Name Selector Tests

    [Fact]
    public void Evaluate_NameSelector_SelectsMember()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"store": {"book": []}}""").RootElement;
        var query = JsonPathParser.Parse("$.store");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$['store']");
        result[0].Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public void Evaluate_NameSelector_NestedPath_SelectsCorrectly()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"store": {"book": {"title": "Test"}}}""").RootElement;
        var query = JsonPathParser.Parse("$.store.book");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$['store']['book']");
        result[0].Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public void Evaluate_NameSelector_NonExistentMember_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"store": {}}""").RootElement;
        var query = JsonPathParser.Parse("$.store.book");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_NameSelector_OnArray_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var query = JsonPathParser.Parse("$.name");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    #endregion

    #region Wildcard Selector Tests

    [Fact]
    public void Evaluate_WildcardSelector_OnObject_SelectsAllMembers()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1, "b": 2, "c": 3}""").RootElement;
        var query = JsonPathParser.Parse("$.*");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        var paths = result.GetNormalizedPaths();
        paths.ShouldContain("$['a']");
        paths.ShouldContain("$['b']");
        paths.ShouldContain("$['c']");
    }

    [Fact]
    public void Evaluate_WildcardSelector_OnArray_SelectsAllElements()
    {
        // Arrange
        var json = JsonDocument.Parse("""[10, 20, 30]""").RootElement;
        var query = JsonPathParser.Parse("$[*]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].NormalizedPath.ShouldBe("$[0]");
        result[0].Value.GetInt32().ShouldBe(10);
        result[1].NormalizedPath.ShouldBe("$[1]");
        result[1].Value.GetInt32().ShouldBe(20);
        result[2].NormalizedPath.ShouldBe("$[2]");
        result[2].Value.GetInt32().ShouldBe(30);
    }

    [Fact]
    public void Evaluate_WildcardSelector_Nested_SelectsCorrectly()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "users": [
                {"name": "Alice"},
                {"name": "Bob"}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.users[*].name");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetString().ShouldBe("Alice");
        result[1].Value.GetString().ShouldBe("Bob");
    }

    #endregion

    #region Index Selector Tests

    [Fact]
    public void Evaluate_IndexSelector_PositiveIndex_SelectsElement()
    {
        // Arrange
        var json = JsonDocument.Parse("""["a", "b", "c"]""").RootElement;
        var query = JsonPathParser.Parse("$[1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$[1]");
        result[0].Value.GetString().ShouldBe("b");
    }

    [Fact]
    public void Evaluate_IndexSelector_NegativeIndex_SelectsFromEnd()
    {
        // Arrange
        var json = JsonDocument.Parse("""["a", "b", "c"]""").RootElement;
        var query = JsonPathParser.Parse("$[-1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$[2]");
        result[0].Value.GetString().ShouldBe("c");
    }

    [Fact]
    public void Evaluate_IndexSelector_OutOfBounds_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""["a", "b"]""").RootElement;
        var query = JsonPathParser.Parse("$[10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_IndexSelector_OnObject_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var query = JsonPathParser.Parse("$[0]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    #endregion

    #region Slice Selector Tests

    [Fact]
    public void Evaluate_SliceSelector_StartAndEnd_SelectsRange()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[1:4]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(1);
        result[1].Value.GetInt32().ShouldBe(2);
        result[2].Value.GetInt32().ShouldBe(3);
    }

    [Fact]
    public void Evaluate_SliceSelector_WithStep_SelectsEveryNth()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4, 5]""").RootElement;
        var query = JsonPathParser.Parse("$[::2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(0);
        result[1].Value.GetInt32().ShouldBe(2);
        result[2].Value.GetInt32().ShouldBe(4);
    }

    [Fact]
    public void Evaluate_SliceSelector_NegativeStep_ReversesSelection()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4]""").RootElement;
        var query = JsonPathParser.Parse("$[::-1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(5);
        result[0].Value.GetInt32().ShouldBe(4);
        result[1].Value.GetInt32().ShouldBe(3);
        result[2].Value.GetInt32().ShouldBe(2);
        result[3].Value.GetInt32().ShouldBe(1);
        result[4].Value.GetInt32().ShouldBe(0);
    }

    [Fact]
    public void Evaluate_SliceSelector_OnlyStart_SelectsToEnd()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4]""").RootElement;
        var query = JsonPathParser.Parse("$[2:]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(2);
        result[1].Value.GetInt32().ShouldBe(3);
        result[2].Value.GetInt32().ShouldBe(4);
    }

    [Fact]
    public void Evaluate_SliceSelector_OnlyEnd_SelectsFromStart()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4]""").RootElement;
        var query = JsonPathParser.Parse("$[:3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Value.GetInt32().ShouldBe(0);
        result[1].Value.GetInt32().ShouldBe(1);
        result[2].Value.GetInt32().ShouldBe(2);
    }

    [Fact]
    public void Evaluate_SliceSelector_NegativeIndices_SelectsCorrectly()
    {
        // Arrange
        var json = JsonDocument.Parse("""[0, 1, 2, 3, 4]""").RootElement;
        var query = JsonPathParser.Parse("$[-3:-1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetInt32().ShouldBe(2);
        result[1].Value.GetInt32().ShouldBe(3);
    }

    #endregion

    #region Descendant Segment Tests

    [Fact]
    public void Evaluate_DescendantSegment_SelectsAllDescendants()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "store": {
                "book": [
                    {"title": "Book1"},
                    {"title": "Book2"}
                ]
            }
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$..title");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetString().ShouldBe("Book1");
        result[1].Value.GetString().ShouldBe("Book2");
    }

    [Fact]
    public void Evaluate_DescendantSegment_WithWildcard_SelectsAll()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "a": 1,
            "b": {
                "c": 2,
                "d": 3
            }
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$..*");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Multiple Selectors Tests

    [Fact]
    public void Evaluate_MultipleSelectors_SelectsAll()
    {
        // Arrange
        var json = JsonDocument.Parse("""["a", "b", "c", "d"]""").RootElement;
        var query = JsonPathParser.Parse("$[0,2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetString().ShouldBe("a");
        result[1].Value.GetString().ShouldBe("c");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_EmptyObject_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""{}""").RootElement;
        var query = JsonPathParser.Parse("$.nonexistent");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_EmptyArray_ReturnsEmpty()
    {
        // Arrange
        var json = JsonDocument.Parse("""[]""").RootElement;
        var query = JsonPathParser.Parse("$[0]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_NullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var json = JsonDocument.Parse("""{}""").RootElement;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _evaluator.Evaluate(null!, json));
    }

    #endregion

    #region Normalized Path Tests

    [Fact]
    public void Evaluate_NormalizedPaths_CorrectFormat()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "store": {
                "book": ["Book1", "Book2"]
            }
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.store.book[*]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].NormalizedPath.ShouldBe("$['store']['book'][0]");
        result[1].NormalizedPath.ShouldBe("$['store']['book'][1]");
    }

    [Fact]
    public void Evaluate_SpecialCharactersInMemberName_EscapedInPath()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"key with\tspace": 1}""").RootElement;
        var query = JsonPathParser.Parse("$['key with\tspace']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldContain(@"\t");
    }

    #endregion
}
