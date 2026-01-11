using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// Tests for filter expression evaluation.
/// Covers RFC 9535 Section 2.3.5 filter selectors and Table 11 comparison semantics.
/// </summary>
public sealed class FilterExpressionTests
{
    private readonly JsonPathEvaluator _evaluator = new();

    #region Comparison Operator Tests

    [Fact]
    public void Filter_Equality_MatchesValue()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "store": {
                "book": [
                    {"title": "Book1", "price": 10.00},
                    {"title": "Book2", "price": 15.00},
                    {"title": "Book3", "price": 10.00}
                ]
            }
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.store.book[?@.price == 10.00]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("title").GetString().ShouldBe("Book1");
        result[1].Value.GetProperty("title").GetString().ShouldBe("Book3");
    }

    [Fact]
    public void Filter_NotEqual_ExcludesValue()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "items": [
                {"status": "active"},
                {"status": "inactive"},
                {"status": "active"}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.items[?@.status != 'inactive']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(n => n.Value.GetProperty("status").GetString() == "active");
    }

    [Fact]
    public void Filter_LessThan_ComparesNumbers()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "products": [
                {"name": "A", "price": 5},
                {"name": "B", "price": 15},
                {"name": "C", "price": 8}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.products[?@.price < 10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("name").GetString().ShouldBe("A");
        result[1].Value.GetProperty("name").GetString().ShouldBe("C");
    }

    [Fact]
    public void Filter_LessEqual_IncludesEqualValue()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"x": 5}, {"x": 10}, {"x": 15}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.x <= 10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("x").GetInt32().ShouldBe(5);
        result[1].Value.GetProperty("x").GetInt32().ShouldBe(10);
    }

    [Fact]
    public void Filter_GreaterThan_ComparesNumbers()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"x": 5}, {"x": 10}, {"x": 15}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.x > 10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("x").GetInt32().ShouldBe(15);
    }

    [Fact]
    public void Filter_GreaterEqual_IncludesEqualValue()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"x": 5}, {"x": 10}, {"x": 15}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.x >= 10]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("x").GetInt32().ShouldBe(10);
        result[1].Value.GetProperty("x").GetInt32().ShouldBe(15);
    }

    #endregion

    #region String Comparison Tests

    [Fact]
    public void Filter_StringComparison_Ordinal()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"name": "Alice"},
            {"name": "Bob"},
            {"name": "Charlie"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.name < 'C']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Alice");
        result[1].Value.GetProperty("name").GetString().ShouldBe("Bob");
    }

    [Fact]
    public void Filter_StringEquality_CaseSensitive()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"name": "Test"}, {"name": "test"}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.name == 'Test']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Test");
    }

    #endregion

    #region Type Mismatch Tests

    [Fact]
    public void Filter_TypeMismatch_NumberAndString_NotEqual()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"value": 13}, {"value": "13"}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.value == 13]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("value").GetInt32().ShouldBe(13);
    }

    [Fact]
    public void Filter_TypeMismatch_AlwaysNotEqual()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"v": "text"}, {"v": 42}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.v != 42]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1); // Only string "text" is != 42 (number)
        result[0].Value.GetProperty("v").GetString().ShouldBe("text");
    }

    #endregion

    #region Logical Operator Tests

    [Fact]
    public void Filter_LogicalAnd_BothConditionsTrue()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"price": 5, "category": "A"},
            {"price": 15, "category": "A"},
            {"price": 5, "category": "B"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.price < 10 && @.category == 'A']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("price").GetInt32().ShouldBe(5);
        result[0].Value.GetProperty("category").GetString().ShouldBe("A");
    }

    [Fact]
    public void Filter_LogicalOr_EitherConditionTrue()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"price": 5, "discount": 0},
            {"price": 15, "discount": 20},
            {"price": 8, "discount": 5}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.price < 10 || @.discount > 15]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(3); // All items match one condition or the other
    }

    [Fact]
    public void Filter_LogicalNot_NegatesCondition()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"active": true}, {"active": false}]""").RootElement;
        var query = JsonPathParser.Parse("$[?!@.active]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("active").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public void Filter_ComplexLogicalExpression()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"x": 5, "y": 10, "z": 15},
            {"x": 15, "y": 10, "z": 5},
            {"x": 10, "y": 20, "z": 10}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?(@.x < 10 && @.y == 10) || @.z > 12]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("x").GetInt32().ShouldBe(5);
    }

    #endregion

    #region Existence Test Tests

    [Fact]
    public void Filter_ExistenceTest_PropertyExists()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"name": "A", "isbn": "123"},
            {"name": "B"},
            {"name": "C", "isbn": "456"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.isbn]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("name").GetString().ShouldBe("A");
        result[1].Value.GetProperty("name").GetString().ShouldBe("C");
    }

    [Fact]
    public void Filter_ExistenceTest_NestedProperty()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"book": {"author": "Smith"}},
            {"book": {}},
            {"book": {"author": "Jones"}}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.book.author]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
    }

    #endregion

    #region Relative and Absolute Query Tests

    [Fact]
    public void Filter_RelativeQuery_UsesCurrentNode()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"value": 5, "threshold": 10},
            {"value": 15, "threshold": 10},
            {"value": 8, "threshold": 10}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.value < @.threshold]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("value").GetInt32().ShouldBe(5);
        result[1].Value.GetProperty("value").GetInt32().ShouldBe(8);
    }

    [Fact]
    public void Filter_AbsoluteQuery_UsesRootNode()
    {
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "threshold": 10,
            "items": [
                {"value": 5},
                {"value": 15},
                {"value": 8}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.items[?@.value < $.threshold]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("value").GetInt32().ShouldBe(5);
        result[1].Value.GetProperty("value").GetInt32().ShouldBe(8);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Filter_EmptyNodelist_ReturnsNoResults()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"x": 1}, {"x": 2}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.y == 1]"); // y doesn't exist

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Filter_BooleanComparison_Works()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"active": true}, {"active": false}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("active").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void Filter_NullComparison_Works()
    {
        // Arrange
        var json = JsonDocument.Parse("""[{"value": null}, {"value": 1}]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.value == null]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("value").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    #endregion
}
