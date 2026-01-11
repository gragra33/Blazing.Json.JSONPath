using Blazing.Json.JSONPath.Lexer;

namespace Blazing.Json.JSONPath.Parser.Nodes;

/// <summary>
/// Base class for all filter expression nodes.
/// Filter expressions are used in filter selectors to test conditions.
/// </summary>
public abstract record FilterExpressionNode : JsonPathNode;

/// <summary>
/// Represents a logical AND expression (&amp;&amp;).
/// Example: @.price &lt; 10 &amp;&amp; @.category == 'fiction'
/// </summary>
/// <param name="Left">The left operand expression.</param>
/// <param name="Right">The right operand expression.</param>
public sealed record LogicalAndExpression(
    FilterExpressionNode Left,
    FilterExpressionNode Right
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitLogicalAndExpression(this);
    }
}

/// <summary>
/// Represents a logical OR expression (||).
/// Example: @.price &lt; 10 || @.discount &gt; 20
/// </summary>
/// <param name="Left">The left operand expression.</param>
/// <param name="Right">The right operand expression.</param>
public sealed record LogicalOrExpression(
    FilterExpressionNode Left,
    FilterExpressionNode Right
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitLogicalOrExpression(this);
    }
}

/// <summary>
/// Represents a logical NOT expression (!).
/// Example: !@.available
/// </summary>
/// <param name="Operand">The operand expression to negate.</param>
public sealed record LogicalNotExpression(
    FilterExpressionNode Operand
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitLogicalNotExpression(this);
    }
}

/// <summary>
/// Represents a comparison expression.
/// Example: @.price &lt; 10 or @.category == 'fiction'
/// </summary>
/// <param name="Left">The left comparable value.</param>
/// <param name="Operator">The comparison operator.</param>
/// <param name="Right">The right comparable value.</param>
public sealed record ComparisonExpression(
    ComparableNode Left,
    ComparisonOperator Operator,
    ComparableNode Right
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitComparisonExpression(this);
    }
}

/// <summary>
/// Represents an existence test (query without comparison).
/// Example: @.isbn (tests if isbn property exists)
/// </summary>
/// <param name="Query">The query expression to test for existence.</param>
public sealed record ExistenceTest(
    QueryExpression Query
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitExistenceTest(this);
    }
}

/// <summary>
/// Represents a function call expression.
/// Example: length(@.title) &gt; 10
/// </summary>
/// <param name="FunctionName">The name of the function to call.</param>
/// <param name="Arguments">The arguments to pass to the function.</param>
public sealed record FunctionExpression(
    string FunctionName,
    params FunctionArgument[] Arguments
) : FilterExpressionNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitFunctionExpression(this);
    }
}

/// <summary>
/// Base class for comparable values in comparison expressions.
/// </summary>
public abstract record ComparableNode : JsonPathNode;

/// <summary>
/// Represents a literal value (string, number, boolean, null).
/// </summary>
/// <param name="Value">The literal value.</param>
/// <param name="Type">The token type indicating the kind of literal.</param>
public sealed record LiteralNode(
    object? Value,
    TokenType Type
) : ComparableNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        throw new NotSupportedException("Literal nodes cannot be visited directly");
    }
}

/// <summary>
/// Represents a query expression that yields a value.
/// </summary>
/// <param name="IsRelative">Whether this is a relative query (starts with @) or absolute (starts with $).</param>
/// <param name="Segments">The segments of the query.</param>
public sealed record QueryExpression(
    bool IsRelative,
    System.Collections.Immutable.ImmutableArray<SegmentNode> Segments
) : ComparableNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        throw new NotSupportedException("Query expression nodes cannot be visited directly");
    }

    /// <summary>
    /// Gets a value indicating whether this query is empty (no segments).
    /// </summary>
    public bool IsEmpty => Segments.IsDefaultOrEmpty;
}

/// <summary>
/// Represents a function call that yields a comparable value.
/// </summary>
/// <param name="Function">The function expression.</param>
public sealed record FunctionCallNode(
    FunctionExpression Function
) : ComparableNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        throw new NotSupportedException("Function call nodes cannot be visited directly");
    }
}

/// <summary>
/// Represents an argument to a function call.
/// </summary>
public abstract record FunctionArgument;

/// <summary>
/// Represents a literal argument.
/// </summary>
/// <param name="Value">The literal value.</param>
public sealed record LiteralArgument(object? Value) : FunctionArgument;

/// <summary>
/// Represents a query argument.
/// </summary>
/// <param name="Query">The query expression.</param>
public sealed record QueryArgument(QueryExpression Query) : FunctionArgument;

/// <summary>
/// Defines comparison operators supported in filter expressions.
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// Equality operator (==).
    /// </summary>
    Equal,

    /// <summary>
    /// Inequality operator (!=).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Less than operator (&lt;).
    /// </summary>
    Less,

    /// <summary>
    /// Less than or equal operator (&lt;=).
    /// </summary>
    LessEqual,

    /// <summary>
    /// Greater than operator (&gt;).
    /// </summary>
    Greater,

    /// <summary>
    /// Greater than or equal operator (&gt;=).
    /// </summary>
    GreaterEqual
}
