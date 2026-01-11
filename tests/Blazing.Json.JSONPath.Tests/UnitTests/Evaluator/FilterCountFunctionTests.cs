using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// RFC 9535 compliance tests for count() function in filter expressions.
/// Validates RFC 9535 Section 2.4.5 (count() function) usage within filters.
/// Note: count() counts NODES in a nodelist, use length() to count array elements.
/// </summary>
public sealed class FilterCountFunctionTests
{
    private readonly JsonPathEvaluator _evaluator = new();

    #region Basic count() Function in Filters

    [Fact]
    public void Evaluate_FilterWithCount_GreaterThan_SelectsMatchingElements()
    {
        // RFC 9535: count() function in filter - select orders with more than 2 items
        // Query: $[?count(@.items[*]) > 2]  (count NODES in the items array)
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["item1", "item2", "item3"]},
            {"orderId": 2, "items": ["item1"]},
            {"orderId": 3, "items": ["item1", "item2"]},
            {"orderId": 4, "items": ["item1", "item2", "item3", "item4"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) > 2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[0].Value.GetProperty("items").GetArrayLength().ShouldBe(3);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(4);
        result[1].Value.GetProperty("items").GetArrayLength().ShouldBe(4);
    }

    [Fact]
    public void Evaluate_FilterWithCount_Equals_SelectsMatchingElements()
    {
        // RFC 9535: count() with equality comparison
        // Query: $[?count(@.tags[*]) == 2]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"id": 1, "tags": ["tag1", "tag2"]},
            {"id": 2, "tags": ["tag1"]},
            {"id": 3, "tags": ["tag1", "tag2", "tag3"]},
            {"id": 4, "tags": ["tag1", "tag2"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.tags[*]) == 2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("id").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("id").GetInt32().ShouldBe(4);
    }

    [Fact]
    public void Evaluate_FilterWithCount_LessThan_SelectsMatchingElements()
    {
        // RFC 9535: count() with less than comparison
        // Query: $[?count(@.items[*]) < 2]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["item1"]},
            {"orderId": 2, "items": ["item1", "item2"]},
            {"orderId": 3, "items": []},
            {"orderId": 4, "items": ["item1", "item2", "item3"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) < 2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(3);
    }

    #endregion

    #region count() with Different Comparison Operators

    [Fact]
    public void Evaluate_FilterWithCount_GreaterOrEqual_SelectsMatchingElements()
    {
        // RFC 9535: count() with >= operator
        // Query: $[?count(@.items[*]) >= 3]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["a", "b", "c"]},
            {"orderId": 2, "items": ["a", "b"]},
            {"orderId": 3, "items": ["a", "b", "c", "d"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) >= 3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(3);
    }

    [Fact]
    public void Evaluate_FilterWithCount_LessOrEqual_SelectsMatchingElements()
    {
        // RFC 9535: count() with <= operator
        // Query: $[?count(@.tags[*]) <= 1]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"id": 1, "tags": []},
            {"id": 2, "tags": ["tag1"]},
            {"id": 3, "tags": ["tag1", "tag2"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.tags[*]) <= 1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("id").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("id").GetInt32().ShouldBe(2);
    }

    [Fact]
    public void Evaluate_FilterWithCount_NotEqual_SelectsMatchingElements()
    {
        // RFC 9535: count() with != operator
        // Query: $[?count(@.items[*]) != 2]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["a"]},
            {"orderId": 2, "items": ["a", "b"]},
            {"orderId": 3, "items": ["a", "b", "c"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) != 2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(3);
    }

    #endregion

    #region count() with Empty Arrays

    [Fact]
    public void Evaluate_FilterWithCount_EmptyArray_ReturnsZero()
    {
        // RFC 9535: count() on empty array returns 0
        // Query: $[?count(@.items[*]) == 0]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": []},
            {"orderId": 2, "items": ["item1"]},
            {"orderId": 3, "items": []}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) == 0]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(3);
    }

    #endregion

    #region count() Combined with Logical Operators

    [Fact]
    public void Evaluate_FilterWithCount_AndLogicalOperator_BothConditionsTrue()
    {
        // RFC 9535: count() combined with AND operator
        // Query: $[?count(@.items[*]) > 1 && @.status == 'active']
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["a", "b"], "status": "active"},
            {"orderId": 2, "items": ["a"], "status": "active"},
            {"orderId": 3, "items": ["a", "b", "c"], "status": "inactive"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) > 1 && @.status == 'active']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
    }

    [Fact]
    public void Evaluate_FilterWithCount_OrLogicalOperator_EitherConditionTrue()
    {
        // RFC 9535: count() combined with OR operator
        // Query: $[?count(@.items[*]) < 2 || @.priority == 'high']
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["a"], "priority": "low"},
            {"orderId": 2, "items": ["a", "b", "c"], "priority": "high"},
            {"orderId": 3, "items": ["a", "b"], "priority": "low"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) < 2 || @.priority == 'high']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(2);
    }

    #endregion

    #region count() on Nested Structures

    [Fact]
    public void Evaluate_FilterWithCount_NestedArrays_CountsCorrectly()
    {
        // RFC 9535: count() on nested array property
        // Query: $.users[?count(@.orders[*]) > 1]
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "users": [
                {"name": "Alice", "orders": [{"id": 1}, {"id": 2}]},
                {"name": "Bob", "orders": [{"id": 3}]},
                {"name": "Charlie", "orders": [{"id": 4}, {"id": 5}, {"id": 6}]}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.users[?count(@.orders[*]) > 1]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Alice");
        result[1].Value.GetProperty("name").GetString().ShouldBe("Charlie");
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public void Evaluate_FilterWithCount_OrdersScenario_SelectsLargeOrders()
    {
        // Real-world: E-commerce - Select orders with 3+ items
        // Query: $.orders[?count(@.items[*]) >= 3]
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "orders": [
                {
                    "orderId": "ORD001",
                    "items": [
                        {"product": "Laptop", "qty": 1},
                        {"product": "Mouse", "qty": 2},
                        {"product": "Keyboard", "qty": 1}
                    ]
                },
                {
                    "orderId": "ORD002",
                    "items": [
                        {"product": "Phone", "qty": 1}
                    ]
                },
                {
                    "orderId": "ORD003",
                    "items": [
                        {"product": "Monitor", "qty": 2},
                        {"product": "Cable", "qty": 3},
                        {"product": "Stand", "qty": 1},
                        {"product": "Adapter", "qty": 2}
                    ]
                }
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.orders[?count(@.items[*]) >= 3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetString().ShouldBe("ORD001");
        result[1].Value.GetProperty("orderId").GetString().ShouldBe("ORD003");
    }

    [Fact]
    public void Evaluate_FilterWithCount_ProjectsScenario_SelectsActiveProjects()
    {
        // Real-world: Project management - Projects with multiple team members
        // Query: $.projects[?count(@.team[*]) > 2 && @.status == 'active']
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "projects": [
                {
                    "name": "Project Alpha",
                    "team": ["Alice", "Bob", "Charlie"],
                    "status": "active"
                },
                {
                    "name": "Project Beta",
                    "team": ["David"],
                    "status": "active"
                },
                {
                    "name": "Project Gamma",
                    "team": ["Eve", "Frank", "Grace", "Henry"],
                    "status": "inactive"
                }
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.projects[?count(@.team[*]) > 2 && @.status == 'active']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Project Alpha");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_FilterWithCount_MissingProperty_ReturnsEmpty()
    {
        // RFC 9535: count() on missing property evaluates to 0
        // Query: $[?count(@.items[*]) > 0]
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"orderId": 1, "items": ["a"]},
            {"orderId": 2},
            {"orderId": 3, "items": ["b", "c"]}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count(@.items[*]) > 0]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("orderId").GetInt32().ShouldBe(1);
        result[1].Value.GetProperty("orderId").GetInt32().ShouldBe(3);
    }

    [Fact]
    public void Evaluate_FilterWithCount_CountRootLevelNodes_WorksCorrectly()
    {
        // RFC 9535: count() counting root-level query results
        // Query: $[?count($.items[*]) == 3]
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "items": ["a", "b", "c"],
            "data": [
                {"id": 1},
                {"id": 2}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$[?count($.items[*]) == 3]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2); // Both properties match the count condition
    }

    #endregion
}
