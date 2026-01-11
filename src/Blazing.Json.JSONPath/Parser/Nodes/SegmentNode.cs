using System.Collections.Immutable;

namespace Blazing.Json.JSONPath.Parser.Nodes;

/// <summary>
/// Base class for all segment nodes in a JSONPath query.
/// Segments define navigation steps through JSON structure.
/// </summary>
public abstract record SegmentNode : JsonPathNode;

/// <summary>
/// Represents a child segment that navigates to direct children.
/// Example: $.store or $[*] or $['name']
/// </summary>
/// <param name="Selectors">The selectors to apply to select children.</param>
public sealed record ChildSegment(
    ImmutableArray<SelectorNode> Selectors
) : SegmentNode
{
    /// <summary>
    /// Gets a value indicating whether this segment has any selectors.
    /// </summary>
    public bool HasSelectors => !Selectors.IsDefaultOrEmpty;

    /// <summary>
    /// Gets the number of selectors in this segment.
    /// </summary>
    public int SelectorCount => Selectors.IsDefault ? 0 : Selectors.Length;

    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitChildSegment(this);
    }

    /// <summary>
    /// Creates a child segment with a single selector.
    /// </summary>
    /// <param name="selector">The selector for this segment.</param>
    /// <returns>A new child segment.</returns>
    public static ChildSegment Create(SelectorNode selector)
    {
        return new ChildSegment(ImmutableArray.Create(selector));
    }

    /// <summary>
    /// Creates a child segment with multiple selectors.
    /// </summary>
    /// <param name="selectors">The selectors for this segment.</param>
    /// <returns>A new child segment.</returns>
    public static ChildSegment Create(params SelectorNode[] selectors)
    {
        return new ChildSegment(selectors.ToImmutableArray());
    }

    /// <summary>
    /// Creates a child segment with multiple selectors.
    /// </summary>
    /// <param name="selectors">The selectors for this segment.</param>
    /// <returns>A new child segment.</returns>
    public static ChildSegment Create(IEnumerable<SelectorNode> selectors)
    {
        return new ChildSegment(selectors.ToImmutableArray());
    }
}

/// <summary>
/// Represents a descendant segment that recursively navigates to all descendants.
/// Example: $..author or $..[*]
/// </summary>
/// <param name="Selectors">The selectors to apply to each descendant.</param>
public sealed record DescendantSegment(
    ImmutableArray<SelectorNode> Selectors
) : SegmentNode
{
    /// <summary>
    /// Gets a value indicating whether this segment has any selectors.
    /// </summary>
    public bool HasSelectors => !Selectors.IsDefaultOrEmpty;

    /// <summary>
    /// Gets the number of selectors in this segment.
    /// </summary>
    public int SelectorCount => Selectors.IsDefault ? 0 : Selectors.Length;

    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitDescendantSegment(this);
    }

    /// <summary>
    /// Creates a descendant segment with a single selector.
    /// </summary>
    /// <param name="selector">The selector for this segment.</param>
    /// <returns>A new descendant segment.</returns>
    public static DescendantSegment Create(SelectorNode selector)
    {
        return new DescendantSegment(ImmutableArray.Create(selector));
    }

    /// <summary>
    /// Creates a descendant segment with multiple selectors.
    /// </summary>
    /// <param name="selectors">The selectors for this segment.</param>
    /// <returns>A new descendant segment.</returns>
    public static DescendantSegment Create(params SelectorNode[] selectors)
    {
        return new DescendantSegment(selectors.ToImmutableArray());
    }

    /// <summary>
    /// Creates a descendant segment with multiple selectors.
    /// </summary>
    /// <param name="selectors">The selectors for this segment.</param>
    /// <returns>A new descendant segment.</returns>
    public static DescendantSegment Create(IEnumerable<SelectorNode> selectors)
    {
        return new DescendantSegment(selectors.ToImmutableArray());
    }
}
