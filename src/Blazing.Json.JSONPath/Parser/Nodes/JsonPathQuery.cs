using System.Collections.Immutable;

namespace Blazing.Json.JSONPath.Parser.Nodes;

/// <summary>
/// Represents a complete JSONPath query as an Abstract Syntax Tree.
/// This is the root node of the AST hierarchy.
/// </summary>
/// <param name="Segments">The sequence of segments that make up the query path.</param>
public sealed record JsonPathQuery(
    ImmutableArray<SegmentNode> Segments
) : JsonPathNode
{
    /// <summary>
    /// Gets a value indicating whether this query is empty (no segments).
    /// </summary>
    public bool IsEmpty => Segments.IsDefaultOrEmpty;

    /// <summary>
    /// Gets the number of segments in this query.
    /// </summary>
    public int SegmentCount => Segments.IsDefault ? 0 : Segments.Length;

    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitQuery(this);
    }

    /// <summary>
    /// Creates an empty JSONPath query (root only: $).
    /// </summary>
    public static JsonPathQuery Empty => new(ImmutableArray<SegmentNode>.Empty);

    /// <summary>
    /// Creates a JSONPath query from a collection of segments.
    /// </summary>
    /// <param name="segments">The segments to include in the query.</param>
    /// <returns>A new JSONPath query.</returns>
    public static JsonPathQuery Create(params SegmentNode[] segments)
    {
        return new JsonPathQuery(segments.ToImmutableArray());
    }

    /// <summary>
    /// Creates a JSONPath query from a collection of segments.
    /// </summary>
    /// <param name="segments">The segments to include in the query.</param>
    /// <returns>A new JSONPath query.</returns>
    public static JsonPathQuery Create(IEnumerable<SegmentNode> segments)
    {
        return new JsonPathQuery(segments.ToImmutableArray());
    }
}
