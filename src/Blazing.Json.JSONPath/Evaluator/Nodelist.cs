using System.Collections;
using System.Collections.Immutable;
using System.Text.Json;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Represents an immutable list of nodes (query result).
/// Implements RFC 9535 nodelist semantics including deduplication.
/// </summary>
/// <remarks>
/// Nodelists are immutable and can be efficiently composed during query evaluation.
/// Supports path-based deduplication to ensure uniqueness.
/// </remarks>
public sealed class Nodelist : IReadOnlyList<JsonNode>
{
    private readonly ImmutableArray<JsonNode> _nodes;

    /// <summary>
    /// Gets an empty nodelist.
    /// </summary>
    public static readonly Nodelist Empty = new(ImmutableArray<JsonNode>.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref="Nodelist"/> class.
    /// </summary>
    /// <param name="nodes">The nodes in this nodelist.</param>
    private Nodelist(ImmutableArray<JsonNode> nodes)
    {
        _nodes = nodes;
    }

    /// <summary>
    /// Gets the number of nodes in this nodelist.
    /// </summary>
    public int Count => _nodes.Length;

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the node to get.</param>
    /// <returns>The <see cref="JsonNode"/> at the specified index.</returns>
    public JsonNode this[int index] => _nodes[index];

    /// <summary>
    /// Creates a nodelist from a single root node.
    /// </summary>
    /// <param name="value">The root JSON value.</param>
    /// <returns>A <see cref="Nodelist"/> containing only the root node.</returns>
    public static Nodelist FromRoot(JsonElement value) =>
        new(ImmutableArray.Create(JsonNode.FromRoot(value)));

    /// <summary>
    /// Creates a nodelist from a collection of nodes.
    /// </summary>
    /// <param name="nodes">The nodes to include.</param>
    /// <returns>A <see cref="Nodelist"/> containing the specified nodes.</returns>
    public static Nodelist Create(IEnumerable<JsonNode> nodes) =>
        new(nodes.ToImmutableArray());

    /// <summary>
    /// Creates a nodelist from a span of nodes (zero-allocation).
    /// </summary>
    /// <param name="nodes">The nodes to include.</param>
    /// <returns>A <see cref="Nodelist"/> containing the specified nodes.</returns>
    public static Nodelist Create(ReadOnlySpan<JsonNode> nodes) =>
        new(ImmutableArray.Create(nodes));

    /// <summary>
    /// Gets all normalized paths in this nodelist.
    /// </summary>
    /// <returns>An immutable array of normalized paths.</returns>
    public ImmutableArray<string> GetNormalizedPaths() =>
        _nodes.Select(n => n.NormalizedPath).ToImmutableArray();

    /// <summary>
    /// Gets all JSON values in this nodelist.
    /// </summary>
    /// <returns>An immutable array of JSON values.</returns>
    public ImmutableArray<JsonElement> GetValues() =>
        _nodes.Select(n => n.Value).ToImmutableArray();

    /// <summary>
    /// Deduplicates nodes by normalized path (RFC 9535 requirement).
    /// </summary>
    /// <returns>A <see cref="Nodelist"/> with unique paths.</returns>
    public Nodelist Deduplicate()
    {
        if (_nodes.Length <= 1)
            return this;

        var seen = new HashSet<string>(_nodes.Length);
        var builder = ImmutableArray.CreateBuilder<JsonNode>(_nodes.Length);

        foreach (var node in _nodes)
        {
            if (seen.Add(node.NormalizedPath))
            {
                builder.Add(node);
            }
        }

        return new Nodelist(builder.ToImmutable());
    }

    /// <summary>
    /// Converts this nodelist to LogicalTrue or LogicalFalse.
    /// Empty nodelist ? LogicalFalse, non-empty ? LogicalTrue (RFC 9535 Section 2.4.2).
    /// </summary>
    /// <returns>True if nodelist is non-empty, false otherwise.</returns>
    public bool ToLogical() => _nodes.Length > 0;

    /// <summary>
    /// Returns an enumerator that iterates through the nodelist.
    /// </summary>
    public IEnumerator<JsonNode> GetEnumerator() => ((IEnumerable<JsonNode>)_nodes).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the nodelist.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns a string representation of this nodelist.
    /// </summary>
    public override string ToString() => $"Nodelist[{Count} nodes]";
}
