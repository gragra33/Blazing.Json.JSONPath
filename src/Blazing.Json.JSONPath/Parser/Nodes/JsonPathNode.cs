namespace Blazing.Json.JSONPath.Parser.Nodes;

/// <summary>
/// Base class for all JSONPath Abstract Syntax Tree nodes.
/// All nodes are immutable records to ensure thread safety and enable structural equality.
/// </summary>
public abstract record JsonPathNode
{
    /// <summary>
    /// Accept method for visitor pattern implementation.
    /// </summary>
    /// <typeparam name="T">The return type of the visitor.</typeparam>
    /// <param name="visitor">The visitor to accept.</param>
    /// <returns>The result of visiting this node.</returns>
    public abstract T Accept<T>(IJsonPathNodeVisitor<T> visitor);
}

/// <summary>
/// Visitor interface for traversing the JSONPath AST.
/// Implements the Visitor pattern for extensible node processing.
/// </summary>
/// <typeparam name="T">The return type of visit operations.</typeparam>
public interface IJsonPathNodeVisitor<out T>
{
    /// <summary>
    /// Visits a JSONPath query node.
    /// </summary>
    T VisitQuery(JsonPathQuery query);

    /// <summary>
    /// Visits a child segment node.
    /// </summary>
    T VisitChildSegment(ChildSegment segment);

    /// <summary>
    /// Visits a descendant segment node.
    /// </summary>
    T VisitDescendantSegment(DescendantSegment segment);

    /// <summary>
    /// Visits a name selector node.
    /// </summary>
    T VisitNameSelector(NameSelector selector);

    /// <summary>
    /// Visits a wildcard selector node.
    /// </summary>
    T VisitWildcardSelector(WildcardSelector selector);

    /// <summary>
    /// Visits an index selector node.
    /// </summary>
    T VisitIndexSelector(IndexSelector selector);

    /// <summary>
    /// Visits a slice selector node.
    /// </summary>
    T VisitSliceSelector(SliceSelector selector);

    /// <summary>
    /// Visits a filter selector node.
    /// </summary>
    T VisitFilterSelector(FilterSelector selector);

    /// <summary>
    /// Visits a logical AND expression node.
    /// </summary>
    T VisitLogicalAndExpression(LogicalAndExpression expression);

    /// <summary>
    /// Visits a logical OR expression node.
    /// </summary>
    T VisitLogicalOrExpression(LogicalOrExpression expression);

    /// <summary>
    /// Visits a logical NOT expression node.
    /// </summary>
    T VisitLogicalNotExpression(LogicalNotExpression expression);

    /// <summary>
    /// Visits a comparison expression node.
    /// </summary>
    T VisitComparisonExpression(ComparisonExpression expression);

    /// <summary>
    /// Visits an existence test expression node.
    /// </summary>
    T VisitExistenceTest(ExistenceTest expression);

    /// <summary>
    /// Visits a function expression node.
    /// </summary>
    T VisitFunctionExpression(FunctionExpression expression);
}
