using Blazing.Json.JSONPath.Parser;
using Blazing.Json.JSONPath.Parser.Nodes;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Parser;

/// <summary>
/// Unit tests for parsing filter expressions.
/// </summary>
public class FilterExpressionParserTests
{
    [Fact]
    public void Parse_SimpleComparison_ReturnsComparisonExpression()
    {
        // Arrange
        const string query = "$[?@.price < 10]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        comparison.Operator.ShouldBe(ComparisonOperator.Less);
    }

    [Fact]
    public void Parse_EqualityComparison_ReturnsCorrectOperator()
    {
        // Arrange
        const string query = "$[?@.category == 'fiction']";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        comparison.Operator.ShouldBe(ComparisonOperator.Equal);
        
        var literal = comparison.Right.ShouldBeOfType<LiteralNode>();
        literal.Value.ShouldBe("fiction");
    }

    [Theory]
    [InlineData("$[?@.price == 10]", ComparisonOperator.Equal)]
    [InlineData("$[?@.price != 10]", ComparisonOperator.NotEqual)]
    [InlineData("$[?@.price < 10]", ComparisonOperator.Less)]
    [InlineData("$[?@.price <= 10]", ComparisonOperator.LessEqual)]
    [InlineData("$[?@.price > 10]", ComparisonOperator.Greater)]
    [InlineData("$[?@.price >= 10]", ComparisonOperator.GreaterEqual)]
    public void Parse_AllComparisonOperators_ReturnsCorrectOperator(string query, ComparisonOperator expectedOperator)
    {
        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        comparison.Operator.ShouldBe(expectedOperator);
    }

    [Fact]
    public void Parse_LogicalAnd_ReturnsAndExpression()
    {
        // Arrange
        const string query = "$[?@.price < 10 && @.category == 'fiction']";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var andExpr = filter.Expression.ShouldBeOfType<LogicalAndExpression>();
        
        andExpr.Left.ShouldBeOfType<ComparisonExpression>();
        andExpr.Right.ShouldBeOfType<ComparisonExpression>();
    }

    [Fact]
    public void Parse_LogicalOr_ReturnsOrExpression()
    {
        // Arrange
        const string query = "$[?@.price < 5 || @.price > 20]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var orExpr = filter.Expression.ShouldBeOfType<LogicalOrExpression>();
        
        orExpr.Left.ShouldBeOfType<ComparisonExpression>();
        orExpr.Right.ShouldBeOfType<ComparisonExpression>();
    }

    [Fact]
    public void Parse_LogicalNot_ReturnsNotExpression()
    {
        // Arrange
        const string query = "$[?!@.available]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var notExpr = filter.Expression.ShouldBeOfType<LogicalNotExpression>();
        
        notExpr.Operand.ShouldBeOfType<ExistenceTest>();
    }

    [Fact]
    public void Parse_ExistenceTest_ReturnsExistenceExpression()
    {
        // Arrange
        const string query = "$[?@.isbn]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var existence = filter.Expression.ShouldBeOfType<ExistenceTest>();
        
        existence.Query.IsRelative.ShouldBeTrue();
        existence.Query.Segments.Length.ShouldBe(1);
    }

    [Fact]
    public void Parse_ParenthesizedExpression_ReturnsCorrectStructure()
    {
        // Arrange
        const string query = "$[?(@.price < 10 && @.available) || @.discount > 20]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var orExpr = filter.Expression.ShouldBeOfType<LogicalOrExpression>();
        
        orExpr.Left.ShouldBeOfType<LogicalAndExpression>();
        orExpr.Right.ShouldBeOfType<ComparisonExpression>();
    }

    [Fact]
    public void Parse_RelativeQuery_ReturnsCurrentIdentifier()
    {
        // Arrange
        const string query = "$[?@.price < 10]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        
        var queryNode = comparison.Left.ShouldBeOfType<QueryExpression>();
        queryNode.IsRelative.ShouldBeTrue();
    }

    [Fact]
    public void Parse_AbsoluteQuery_ReturnsRootIdentifier()
    {
        // Arrange
        const string query = "$[?@.price < $.maxPrice]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        
        var queryNode = comparison.Right.ShouldBeOfType<QueryExpression>();
        queryNode.IsRelative.ShouldBeFalse();
    }

    [Fact]
    public void Parse_LiteralValues_ReturnCorrectTypes()
    {
        // Arrange & Act
        var stringQuery = JsonPathParser.Parse("$[?@.name == 'test']");
        var numberQuery = JsonPathParser.Parse("$[?@.age == 42]");
        var boolQuery = JsonPathParser.Parse("$[?@.active == true]");
        var nullQuery = JsonPathParser.Parse("$[?@.value == null]");

        // Assert
        var stringComparison = ((stringQuery.Segments[0] as ChildSegment)!.Selectors[0] as FilterSelector)!
            .Expression.ShouldBeOfType<ComparisonExpression>();
        stringComparison.Right.ShouldBeOfType<LiteralNode>().Value.ShouldBe("test");

        var numberComparison = ((numberQuery.Segments[0] as ChildSegment)!.Selectors[0] as FilterSelector)!
            .Expression.ShouldBeOfType<ComparisonExpression>();
        numberComparison.Right.ShouldBeOfType<LiteralNode>().Value.ShouldBe(42);

        var boolComparison = ((boolQuery.Segments[0] as ChildSegment)!.Selectors[0] as FilterSelector)!
            .Expression.ShouldBeOfType<ComparisonExpression>();
        boolComparison.Right.ShouldBeOfType<LiteralNode>().Value.ShouldBe(true);

        var nullComparison = ((nullQuery.Segments[0] as ChildSegment)!.Selectors[0] as FilterSelector)!
            .Expression.ShouldBeOfType<ComparisonExpression>();
        nullComparison.Right.ShouldBeOfType<LiteralNode>().Value.ShouldBeNull();
    }

    [Fact]
    public void Parse_FunctionCall_ReturnsFunctionExpression()
    {
        // Arrange
        const string query = "$[?length(@.name) > 5]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        var comparison = filter.Expression.ShouldBeOfType<ComparisonExpression>();
        
        var functionCall = comparison.Left.ShouldBeOfType<FunctionCallNode>();
        var function = functionCall.Function;
        function.FunctionName.ShouldBe("length");
        function.Arguments.Length.ShouldBe(1);
    }

    [Fact]
    public void Parse_ComplexFilter_ReturnsCorrectStructure()
    {
        // Arrange
        const string query = "$[?(@.price < 10 && @.category == 'fiction') || @.discount > 20]";

        // Act
        var result = JsonPathParser.Parse(query);

        // Assert
        var segment = result.Segments[0].ShouldBeOfType<ChildSegment>();
        var filter = segment.Selectors[0].ShouldBeOfType<FilterSelector>();
        
        // Root should be OR
        var orExpr = filter.Expression.ShouldBeOfType<LogicalOrExpression>();
        
        // Left should be AND
        var andExpr = orExpr.Left.ShouldBeOfType<LogicalAndExpression>();
        andExpr.Left.ShouldBeOfType<ComparisonExpression>();
        andExpr.Right.ShouldBeOfType<ComparisonExpression>();
        
        // Right should be comparison
        orExpr.Right.ShouldBeOfType<ComparisonExpression>();
    }
}
