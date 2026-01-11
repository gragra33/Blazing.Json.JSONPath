using System.Text.Json;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Represents a comparable value in a filter expression.
/// Can be a JSON value from a query result, a literal value, or Nothing (empty nodelist).
/// Implements RFC 9535 Section 2.3.5.2.2 comparison semantics.
/// </summary>
public readonly struct ComparableValue
{
    /// <summary>
    /// Gets the JSON value, or null if this represents Nothing.
    /// </summary>
    public JsonElement? Value { get; init; }

    /// <summary>
    /// Gets a value indicating whether this represents Nothing (empty nodelist or no value).
    /// </summary>
    public bool IsNothing => Value is null;

    /// <summary>
    /// Creates a ComparableValue from a JSON element.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A ComparableValue containing the JSON value.</returns>
    public static ComparableValue FromValue(JsonElement value) => new() { Value = value };

    /// <summary>
    /// Creates a ComparableValue representing Nothing (empty nodelist).
    /// </summary>
    /// <returns>A ComparableValue representing Nothing.</returns>
    public static ComparableValue Nothing => new() { Value = null };

    /// <summary>
    /// Creates a ComparableValue from a literal object.
    /// </summary>
    /// <param name="literal">The literal value (string, int, double, bool, or null).</param>
    /// <returns>A ComparableValue containing the literal as JSON.</returns>
    public static ComparableValue FromLiteral(object? literal)
    {
        // Don't treat JSON null as Nothing - null is a valid JSON value to compare against
        // Only queries that return empty nodelists should be Nothing
        
        // Convert literal to JsonElement (including null ? JSON null)
        using var doc = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(literal));
        return FromValue(doc.RootElement.Clone());
    }

    /// <summary>
    /// Creates a ComparableValue from a nodelist result.
    /// If the nodelist is empty, returns Nothing.
    /// If the nodelist contains exactly one element, returns that element.
    /// If the nodelist contains multiple elements, returns Nothing (RFC: singular query required).
    /// </summary>
    /// <param name="nodelist">The nodelist from a query.</param>
    /// <returns>A ComparableValue.</returns>
    public static ComparableValue FromNodelist(Nodelist nodelist)
    {
        if (nodelist.Count == 0)
            return Nothing;

        if (nodelist.Count == 1)
            return FromValue(nodelist[0].Value);

        // Multiple values - RFC says this is Nothing for comparison purposes
        return Nothing;
    }

    /// <summary>
    /// Returns a string representation of this comparable value.
    /// </summary>
    public override string ToString()
    {
        return IsNothing ? "Nothing" : Value!.Value.ToString();
    }
}
