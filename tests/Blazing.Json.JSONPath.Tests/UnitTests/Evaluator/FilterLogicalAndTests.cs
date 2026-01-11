using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// RFC 9535 compliance tests for complex filter expressions with logical AND (&&).
/// Validates filter expressions like: $[?@.Age > 25 && @.IsActive == true]
/// Tests RFC 9535 Section 2.3.5 (Filter Selector) with compound logical expressions.
/// </summary>
public sealed class FilterLogicalAndTests
{
    private readonly JsonPathEvaluator _evaluator = new();

    #region Basic Logical AND Tests

    [Fact]
    public void Evaluate_FilterLogicalAnd_BothConditionsTrue_SelectsElement()
    {
        // RFC 9535: Filter with AND - both conditions must be true
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "active": true},
            {"age": 20, "active": true},
            {"age": 35, "active": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
        result[0].Value.GetProperty("active").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_FirstConditionFalse_ReturnsEmpty()
    {
        // RFC 9535: AND short-circuits when first condition is false
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 20, "active": true},
            {"age": 22, "active": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_SecondConditionFalse_ReturnsEmpty()
    {
        // RFC 9535: AND requires both conditions to be true
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "active": false},
            {"age": 35, "active": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_BothConditionsFalse_ReturnsEmpty()
    {
        // RFC 9535: AND with both false conditions
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 20, "active": false},
            {"age": 22, "active": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    #endregion

    #region Multiple AND Conditions

    [Fact]
    public void Evaluate_FilterLogicalAnd_ThreeConditions_AllMustBeTrue()
    {
        // RFC 9535: Chaining multiple AND conditions
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "active": true, "verified": true},
            {"age": 35, "active": true, "verified": false},
            {"age": 40, "active": false, "verified": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true && @.verified == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_FourConditions_SelectsCorrectly()
    {
        // RFC 9535: Multiple AND conditions (4 conditions)
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "active": true, "verified": true, "premium": true},
            {"age": 35, "active": true, "verified": true, "premium": false},
            {"age": 40, "active": true, "verified": false, "premium": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse(
            "$[?@.age > 25 && @.active == true && @.verified == true && @.premium == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
    }

    #endregion

    #region Different Comparison Operators with AND

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithLessThan_WorksCorrectly()
    {
        // RFC 9535: AND with < operator
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"price": 50, "inStock": true},
            {"price": 150, "inStock": true},
            {"price": 75, "inStock": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.price < 100 && @.inStock == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("price").GetInt32().ShouldBe(50);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithGreaterEqual_WorksCorrectly()
    {
        // RFC 9535: AND with >= operator
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"score": 85, "passed": true},
            {"score": 92, "passed": true},
            {"score": 88, "passed": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.score >= 90 && @.passed == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("score").GetInt32().ShouldBe(92);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithNotEqual_WorksCorrectly()
    {
        // RFC 9535: AND with != operator
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"status": "active", "level": 2},
            {"status": "inactive", "level": 2},
            {"status": "active", "level": 1}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.status != 'inactive' && @.level == 2]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("status").GetString().ShouldBe("active");
        result[0].Value.GetProperty("level").GetInt32().ShouldBe(2);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_MixedOperators_SelectsCorrectly()
    {
        // RFC 9535: AND with different comparison operators
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 25, "score": 85, "name": "Alice"},
            {"age": 30, "score": 92, "name": "Bob"},
            {"age": 28, "score": 78, "name": "Charlie"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age >= 25 && @.score > 80 && @.name != 'Charlie']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Alice");
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Bob");
    }

    #endregion

    #region String Comparisons with AND

    [Fact]
    public void Evaluate_FilterLogicalAnd_StringEquality_WorksCorrectly()
    {
        // RFC 9535: AND with string comparisons
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"category": "electronics", "brand": "Samsung"},
            {"category": "electronics", "brand": "Apple"},
            {"category": "furniture", "brand": "IKEA"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.category == 'electronics' && @.brand == 'Apple']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("brand").GetString().ShouldBe("Apple");
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_StringComparison_LessThan_WorksCorrectly()
    {
        // RFC 9535: AND with string < operator (Unicode comparison)
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"code": "A123", "active": true},
            {"code": "B456", "active": true},
            {"code": "C789", "active": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.code < 'C' && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("code").GetString().ShouldBe("A123");
        result[1].Value.GetProperty("code").GetString().ShouldBe("B456");
    }

    #endregion

    #region Null and Boolean Comparisons

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithNull_WorksCorrectly()
    {
        // RFC 9535: AND with null comparison
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"value": 10, "optional": null},
            {"value": 20, "optional": "data"},
            {"value": 30, "optional": null}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.value > 15 && @.optional == null]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("value").GetInt32().ShouldBe(30);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_BothBooleans_WorksCorrectly()
    {
        // RFC 9535: AND with multiple boolean comparisons
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"isActive": true, "isVerified": true, "isPremium": true},
            {"isActive": true, "isVerified": false, "isPremium": true},
            {"isActive": false, "isVerified": true, "isPremium": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.isActive == true && @.isVerified == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("isPremium").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithFalse_WorksCorrectly()
    {
        // RFC 9535: AND comparing with false
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "deleted": false},
            {"age": 35, "deleted": true},
            {"age": 40, "deleted": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.deleted == false]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
        result[1].Value.GetProperty("age").GetInt32().ShouldBe(40);
    }

    #endregion

    #region Existence Tests with AND

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithExistenceTest_WorksCorrectly()
    {
        // RFC 9535: AND with existence test
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "email": "alice@example.com"},
            {"age": 35},
            {"age": 40, "email": "charlie@example.com"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.email]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetProperty("email").GetString() == "alice@example.com");
        result.ShouldContain(r => r.Value.GetProperty("email").GetString() == "charlie@example.com");
    }

    #endregion

    #region Nested Property Access with AND

    [Fact]
    public void Evaluate_FilterLogicalAnd_NestedProperties_WorksCorrectly()
    {
        // RFC 9535: AND with nested property access
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"user": {"age": 30, "verified": true}, "status": "active"},
            {"user": {"age": 35, "verified": false}, "status": "active"},
            {"user": {"age": 40, "verified": true}, "status": "inactive"}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.user.age > 25 && @.status == 'active']");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
    }

    #endregion

    #region Parenthesized Expressions

    [Fact]
    public void Evaluate_FilterLogicalAnd_WithParentheses_WorksCorrectly()
    {
        // RFC 9535: AND with parenthesized expressions
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30, "active": true},
            {"age": 20, "active": true},
            {"age": 35, "active": false}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?(@.age > 25 && @.active == true)]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
    }

    #endregion

    #region Complex Real-World Scenarios

    [Fact]
    public void Evaluate_FilterLogicalAnd_UserScenario_SelectsQualifiedUsers()
    {
        // Real-world: Select users who are adults and active
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "users": [
                {"id": 1, "name": "Alice", "age": 28, "isActive": true},
                {"id": 2, "name": "Bob", "age": 17, "isActive": true},
                {"id": 3, "name": "Charlie", "age": 35, "isActive": false},
                {"id": 4, "name": "Diana", "age": 42, "isActive": true}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.users[?@.age >= 18 && @.isActive == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Alice");
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Diana");
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_ProductScenario_SelectsAvailableProducts()
    {
        // Real-world: Select products in stock and within price range
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "products": [
                {"name": "Laptop", "price": 1200, "inStock": true},
                {"name": "Mouse", "price": 25, "inStock": true},
                {"name": "Keyboard", "price": 80, "inStock": false},
                {"name": "Monitor", "price": 300, "inStock": true}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.products[?@.price < 500 && @.inStock == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Mouse");
        result.ShouldContain(r => r.Value.GetProperty("name").GetString() == "Monitor");
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_OrderScenario_SelectsPaidOrders()
    {
        // Real-world: Select orders that are shipped and paid
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "orders": [
                {"orderId": "A1", "status": "shipped", "paid": true},
                {"orderId": "A2", "status": "pending", "paid": true},
                {"orderId": "A3", "status": "shipped", "paid": false},
                {"orderId": "A4", "status": "shipped", "paid": true}
            ]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.orders[?@.status == 'shipped' && @.paid == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetProperty("orderId").GetString() == "A1");
        result.ShouldContain(r => r.Value.GetProperty("orderId").GetString() == "A4");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Evaluate_FilterLogicalAnd_MissingProperty_FirstCondition_ReturnsFalse()
    {
        // RFC 9535: Missing property in first condition evaluates to false
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"active": true},
            {"age": 30, "active": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_MissingProperty_SecondCondition_ReturnsFalse()
    {
        // RFC 9535: Missing property in second condition evaluates to false
        // Arrange
        var json = JsonDocument.Parse("""
        [
            {"age": 30},
            {"age": 35, "active": true}
        ]
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(35);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_EmptyArray_ReturnsEmpty()
    {
        // RFC 9535: Filter on empty array returns empty
        // Arrange
        var json = JsonDocument.Parse("""[]""").RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Evaluate_FilterLogicalAnd_OnObject_FiltersMemberValues()
    {
        // RFC 9535: Filter works on object members too
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "user1": {"age": 30, "active": true},
            "user2": {"age": 20, "active": true},
            "user3": {"age": 35, "active": false}
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$[?@.age > 25 && @.active == true]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("age").GetInt32().ShouldBe(30);
    }

    #endregion

    #region RFC 9535 Table 12 Examples

    [Fact]
    public void Evaluate_FilterLogicalAnd_RFCExample_ObjectValues()
    {
        // RFC 9535 Table 12: $.o[?@>1 && @<4]
        // Arrange
        var json = JsonDocument.Parse("""
        {
            "o": {"p": 1, "q": 2, "r": 3, "s": 5, "t": {"u": 6}},
            "a": [3, 5, 1, 2, 4, 6]
        }
        """).RootElement;
        var query = JsonPathParser.Parse("$.o[?@>1 && @<4]");

        // Act
        var result = _evaluator.Evaluate(query, json);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(r => r.Value.GetInt32() == 2);
        result.ShouldContain(r => r.Value.GetInt32() == 3);
    }

    #endregion
}
