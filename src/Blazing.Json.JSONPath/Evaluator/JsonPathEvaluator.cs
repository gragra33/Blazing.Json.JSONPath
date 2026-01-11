using System.Text.Json;
using Blazing.Json.JSONPath.Parser.Nodes;
using Blazing.Json.JSONPath.Utilities;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Evaluates JSONPath queries against JSON documents.
/// Implements RFC 9535 query semantics.
/// </summary>
public sealed class JsonPathEvaluator
{
    private readonly FilterEvaluator _filterEvaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathEvaluator"/> class.
    /// </summary>
    public JsonPathEvaluator()
    {
        _filterEvaluator = new FilterEvaluator(this);
    }

    /// <summary>
    /// Evaluates a JSONPath query against a JSON value.
    /// </summary>
    /// <param name="query">The parsed JSONPath query (AST).</param>
    /// <param name="value">The JSON value to query.</param>
    /// <returns>A <see cref="Nodelist"/> containing matching nodes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    public Nodelist Evaluate(JsonPathQuery query, JsonElement value)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Start with root node
        var nodelist = Nodelist.FromRoot(value);

        // Apply each segment
        foreach (var segment in query.Segments)
        {
            nodelist = ApplySegment(segment, nodelist, value);

            // Short-circuit if nodelist becomes empty
            if (nodelist.Count == 0)
                break;
        }

        return nodelist;
    }

    /// <summary>
    /// Applies a segment to a nodelist, producing a new nodelist.
    /// </summary>
    private Nodelist ApplySegment(SegmentNode segment, Nodelist input, JsonElement rootNode)
    {
        return segment switch
        {
            ChildSegment child => ApplyChildSegment(child, input, rootNode),
            DescendantSegment desc => ApplyDescendantSegment(desc, input, rootNode),
            _ => throw new NotSupportedException($"Unknown segment type: {segment.GetType().Name}")
        };
    }

    /// <summary>
    /// Applies a child segment to a nodelist.
    /// </summary>
    private Nodelist ApplyChildSegment(ChildSegment segment, Nodelist input, JsonElement rootNode)
    {
        using var builder = new NodelistBuilder();

        foreach (var node in input)
        {
            foreach (var selector in segment.Selectors)
            {
                var selected = ApplySelector(selector, node, rootNode);
                builder.AddRange(selected);
            }
        }

        return builder.ToNodelist();
    }

    /// <summary>
    /// Applies a descendant segment to a nodelist (recursive descent).
    /// </summary>
    private Nodelist ApplyDescendantSegment(DescendantSegment segment, Nodelist input, JsonElement rootNode)
    {
        using var builder = new NodelistBuilder();

        foreach (var node in input)
        {
            // Visit node and all descendants (depth-first)
            var descendants = GetDescendants(node);

            foreach (var descendant in descendants)
            {
                foreach (var selector in segment.Selectors)
                {
                    var selected = ApplySelector(selector, descendant, rootNode);
                    builder.AddRange(selected);
                }
            }
        }

        return builder.ToNodelist();
    }

    /// <summary>
    /// Gets all descendants of a node (including the node itself).
    /// Visits in depth-first order as per RFC 9535.
    /// </summary>
    private IEnumerable<JsonNode> GetDescendants(JsonNode node)
    {
        // Include input node
        yield return node;

        // Recursively visit children
        foreach (var child in GetChildren(node))
        {
            foreach (var descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets the direct children of a node.
    /// </summary>
    private IEnumerable<JsonNode> GetChildren(JsonNode node)
    {
        switch (node.Value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in node.Value.EnumerateObject())
                {
                    var childPath = AppendMemberToPath(node.NormalizedPath, property.Name);
                    yield return new JsonNode(property.Value, childPath);
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var element in node.Value.EnumerateArray())
                {
                    var childPath = AppendIndexToPath(node.NormalizedPath, index);
                    yield return new JsonNode(element, childPath);
                    index++;
                }
                break;
        }
    }

    /// <summary>
    /// Applies a selector to a node, producing matching child nodes.
    /// </summary>
    private IEnumerable<JsonNode> ApplySelector(SelectorNode selector, JsonNode node, JsonElement rootNode)
    {
        return selector switch
        {
            NameSelector name => SelectByName(name, node),
            WildcardSelector => SelectAllChildren(node),
            IndexSelector index => SelectByIndex(index, node),
            SliceSelector slice => SelectBySlice(slice, node),
            FilterSelector filter => SelectByFilter(filter, node, rootNode),
            _ => throw new NotSupportedException($"Unknown selector type: {selector.GetType().Name}")
        };
    }

    /// <summary>
    /// Selects a child by member name.
    /// </summary>
    private IEnumerable<JsonNode> SelectByName(NameSelector selector, JsonNode node)
    {
        if (node.Value.ValueKind != JsonValueKind.Object)
            yield break;

        if (node.Value.TryGetProperty(selector.Name, out var value))
        {
            var childPath = AppendMemberToPath(node.NormalizedPath, selector.Name);
            yield return new JsonNode(value, childPath);
        }
    }

    /// <summary>
    /// Selects all children (wildcard selector).
    /// </summary>
    private IEnumerable<JsonNode> SelectAllChildren(JsonNode node)
    {
        return GetChildren(node);
    }

    /// <summary>
    /// Selects an array element by index (supports negative indices).
    /// </summary>
    private IEnumerable<JsonNode> SelectByIndex(IndexSelector selector, JsonNode node)
    {
        if (node.Value.ValueKind != JsonValueKind.Array)
            yield break;

        var arrayLength = node.Value.GetArrayLength();
        var normalizedIndex = selector.Normalize(arrayLength);

        if (normalizedIndex < 0)
            yield break; // Out of bounds

        var currentIndex = 0;
        foreach (var element in node.Value.EnumerateArray())
        {
            if (currentIndex == normalizedIndex)
            {
                var childPath = AppendIndexToPath(node.NormalizedPath, normalizedIndex);
                yield return new JsonNode(element, childPath);
                yield break;
            }
            currentIndex++;
        }
    }

    /// <summary>
    /// Selects a slice of array elements (RFC 9535 Section 2.3.4.2.2).
    /// </summary>
    private IEnumerable<JsonNode> SelectBySlice(SliceSelector selector, JsonNode node)
    {
        if (node.Value.ValueKind != JsonValueKind.Array)
            yield break;

        if (!selector.IsValid) // Step cannot be zero
            yield break;

        var arrayLength = node.Value.GetArrayLength();

        // Get default values based on step direction (RFC 9535 Table 8)
        var start = selector.Start ?? selector.GetDefaultStart(arrayLength);
        var end = selector.End ?? selector.GetDefaultEnd(arrayLength);
        var step = selector.Step;

        // Normalize negative indices
        var normStart = NormalizeIndex(start, arrayLength);
        var normEnd = NormalizeIndex(end, arrayLength);

        // Determine bounds based on step direction
        int lower, upper;
        if (step >= 0)
        {
            lower = Math.Max(Math.Min(normStart, arrayLength), 0);
            upper = Math.Max(Math.Min(normEnd, arrayLength), 0);
        }
        else
        {
            // Negative step: reverse direction
            lower = Math.Max(Math.Min(normEnd, arrayLength - 1), -1);
            upper = Math.Max(Math.Min(normStart, arrayLength - 1), -1);
        }

        // Generate indices
        var indices = GenerateSliceIndices(lower, upper, step);

        // Materialize array into list for random access
        var elements = node.Value.EnumerateArray().ToList();

        foreach (var index in indices)
        {
            if (index >= 0 && index < elements.Count)
            {
                var childPath = AppendIndexToPath(node.NormalizedPath, index);
                yield return new JsonNode(elements[index], childPath);
            }
        }
    }

    /// <summary>
    /// Generates slice indices according to RFC 9535 semantics.
    /// </summary>
    private static IEnumerable<int> GenerateSliceIndices(int lower, int upper, int step)
    {
        if (step > 0)
        {
            for (int i = lower; i < upper; i += step)
                yield return i;
        }
        else if (step < 0)
        {
            for (int i = upper; i > lower; i += step)
                yield return i;
        }
        // step == 0: no elements (already checked)
    }

    /// <summary>
    /// Normalizes an array index (handles negative indices).
    /// </summary>
    private static int NormalizeIndex(int index, int arrayLength)
    {
        return index >= 0 ? index : arrayLength + index;
    }

    /// <summary>
    /// Selects elements matching a filter expression.
    /// </summary>
    private IEnumerable<JsonNode> SelectByFilter(FilterSelector selector, JsonNode node, JsonElement rootNode)
    {
        // Filter only applies to arrays and objects
        if (node.Value.ValueKind != JsonValueKind.Array && node.Value.ValueKind != JsonValueKind.Object)
            yield break;

        // Apply filter to each child
        foreach (var child in GetChildren(node))
        {
            if (_filterEvaluator.Evaluate(selector.Expression, child.Value, rootNode))
            {
                yield return child;
            }
        }
    }

    /// <summary>
    /// Appends a member name to a normalized path.
    /// Format: $['name']
    /// </summary>
    private static string AppendMemberToPath(string path, string memberName)
    {
        var escapedName = StringEscaping.EscapeForNormalizedPath(memberName);
        return $"{path}['{escapedName}']";
    }

    /// <summary>
    /// Appends an array index to a normalized path.
    /// Format: $[0]
    /// </summary>
    private static string AppendIndexToPath(string path, int index)
    {
        return $"{path}[{index}]";
    }
}
