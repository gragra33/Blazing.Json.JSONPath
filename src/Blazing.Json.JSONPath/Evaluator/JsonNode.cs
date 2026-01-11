using System.Text.Json;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Represents a single node in a JSONPath query result.
/// Tracks both the JSON value and its location (normalized path).
/// </summary>
/// <remarks>
/// Immutable value type for zero-allocation node tracking.
/// Conforms to RFC 9535 normalized path requirements.
/// </remarks>
public readonly record struct JsonNode
{
    /// <summary>
    /// Gets the JSON value at this node.
    /// </summary>
    public JsonElement Value { get; init; }

    /// <summary>
    /// Gets the normalized path to this node (RFC 9535 format: $['name'][index]...).
    /// </summary>
    public string NormalizedPath { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonNode"/> struct.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <param name="normalizedPath">The normalized path to this node.</param>
    public JsonNode(JsonElement value, string normalizedPath)
    {
        Value = value;
        NormalizedPath = normalizedPath;
    }

    /// <summary>
    /// Creates a root node from a JSON value.
    /// </summary>
    /// <param name="value">The root JSON value.</param>
    /// <returns>A <see cref="JsonNode"/> representing the root ($).</returns>
    public static JsonNode FromRoot(JsonElement value) => new(value, "$");

    /// <summary>
    /// Returns a string representation of this node (normalized path: value kind).
    /// </summary>
    public override string ToString() => $"{NormalizedPath}: {Value.ValueKind}";
}
