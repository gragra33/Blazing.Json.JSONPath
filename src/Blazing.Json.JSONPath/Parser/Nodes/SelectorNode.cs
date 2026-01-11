namespace Blazing.Json.JSONPath.Parser.Nodes;

/// <summary>
/// Base class for all selector nodes in a JSONPath query.
/// Selectors define how to select nodes from the current context.
/// </summary>
public abstract record SelectorNode : JsonPathNode;

/// <summary>
/// Represents a name selector that selects a member by name.
/// Example: 'name' or "name" in $.store['name']
/// </summary>
/// <param name="Name">The unescaped name of the member to select.</param>
public sealed record NameSelector(string Name) : SelectorNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitNameSelector(this);
    }
}

/// <summary>
/// Represents a wildcard selector that selects all members or elements.
/// Example: * in $[*] or $.store.*
/// </summary>
public sealed record WildcardSelector : SelectorNode
{
    /// <summary>
    /// Singleton instance of the wildcard selector.
    /// </summary>
    public static readonly WildcardSelector Instance = new();

    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitWildcardSelector(this);
    }
}

/// <summary>
/// Represents an index selector that selects an array element by index.
/// Example: 0 or -1 in $[0] or $[-1]
/// </summary>
/// <param name="Index">The index to select (can be negative for reverse indexing).</param>
public sealed record IndexSelector(int Index) : SelectorNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitIndexSelector(this);
    }

    /// <summary>
    /// Normalizes the index based on array length.
    /// Negative indices count from the end of the array.
    /// </summary>
    /// <param name="arrayLength">The length of the array.</param>
    /// <returns>The normalized index, or -1 if out of bounds.</returns>
    public int Normalize(int arrayLength)
    {
        var normalized = Index >= 0 ? Index : arrayLength + Index;
        return normalized >= 0 && normalized < arrayLength ? normalized : -1;
    }
}

/// <summary>
/// Represents a slice selector that selects a range of array elements.
/// Example: 1:5:2 in $[1:5:2]
/// </summary>
/// <param name="Start">The start index (inclusive), or null for default.</param>
/// <param name="End">The end index (exclusive), or null for default.</param>
/// <param name="Step">The step size (must not be zero).</param>
public sealed record SliceSelector(
    int? Start,
    int? End,
    int Step
) : SelectorNode
{
    /// <summary>
    /// Gets a value indicating whether step is valid (non-zero).
    /// </summary>
    public bool IsValid => Step != 0;

    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitSliceSelector(this);
    }

    /// <summary>
    /// Creates a slice selector with default step of 1.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="end">The end index.</param>
    /// <returns>A new slice selector.</returns>
    public static SliceSelector Create(int? start, int? end)
    {
        return new SliceSelector(start, end, 1);
    }

    /// <summary>
    /// Gets the default start value based on step direction.
    /// </summary>
    public int GetDefaultStart(int arrayLength) => Step >= 0 ? 0 : arrayLength - 1;

    /// <summary>
    /// Gets the default end value based on step direction.
    /// </summary>
    public int GetDefaultEnd(int arrayLength) => Step >= 0 ? arrayLength : -arrayLength - 1;
}

/// <summary>
/// Represents a filter selector that selects elements matching a filter expression.
/// Example: ?@.price&lt;10 in $[?@.price&lt;10]
/// </summary>
/// <param name="Expression">The filter expression to evaluate.</param>
public sealed record FilterSelector(
    FilterExpressionNode Expression
) : SelectorNode
{
    /// <inheritdoc/>
    public override T Accept<T>(IJsonPathNodeVisitor<T> visitor)
    {
        return visitor.VisitFilterSelector(this);
    }
}
